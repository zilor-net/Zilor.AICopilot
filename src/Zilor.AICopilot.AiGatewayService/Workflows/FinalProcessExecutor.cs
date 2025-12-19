using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zilor.AICopilot.AiGatewayService.Agents;
using Zilor.AICopilot.Services.Common.Contracts;

namespace Zilor.AICopilot.AiGatewayService.Workflows;

/// <summary>
/// 最终处理执行器
/// 职责：利用聚合后的上下文构建 Agent，注入 RAG 提示词，并执行流式生成。
/// </summary>
public class FinalProcessExecutor(
    ChatAgentFactory agentFactory, 
    IServiceProvider serviceProvider,
    ILogger<FinalProcessExecutor> logger):
    ReflectingExecutor<FinalProcessExecutor>("FinalProcessExecutor"),
    IMessageHandler<GenerationContext> // <-- 输入类型变更为聚合上下文
{
    public async ValueTask HandleAsync(
        GenerationContext genContext, 
        IWorkflowContext context,
        CancellationToken cancellationToken = new())
    {
        try
        {
            var request = genContext.Request;
            logger.LogInformation("开始最终生成，SessionId: {SessionId}", request.SessionId);

            // 1. 获取会话关联的模板配置
            // 我们需要知道当前会话使用的是哪个 Agent 模板（例如"通用助手"或"HR助手"）
            using var scope = serviceProvider.CreateScope();
            var queryService = scope.ServiceProvider.GetRequiredService<IDataQueryService>();
            
            var session = await queryService .FirstOrDefaultAsync(queryService.Sessions.Where(s => s.Id == request.SessionId));
            
            if (session == null) throw new InvalidOperationException("会话不存在");

            // 2. 创建基础 Agent 实例
            // 此时 Agent 拥有的是数据库中定义的静态 System Prompt
            var agent = await agentFactory.CreateAgentAsync(session.TemplateId);
            
            // 3. 构建消息列表
            var inputMessages = new List<ChatMessage>();
            string finalUserPrompt;

            // RAG 上下文注入策略：User Context Injection
            if (!string.IsNullOrWhiteSpace(genContext.KnowledgeContext))
            {
                // 使用 XML 标签 <context> 是一种最佳实践
                // 它能帮助模型明确区分“指令(Instruction)”和“数据(Data)”
                finalUserPrompt = $"""
                                   请基于以下检索到的参考信息回答我的问题：

                                   <context>
                                   {genContext.KnowledgeContext}
                                   </context>

                                   回答要求：
                                   1. 引用参考信息时，请标注来源 ID（例如 [^1]）。
                                   2. 在回答结尾，请务必生成一个“参考资料”列表，列出所有被引用的文档来源（去重），格式为：
                                   - [ID]: 文档名称
                                   3. 如果参考信息不足以回答问题，请直接说明，严禁编造。
                                   4. 保持回答专业、简洁。

                                   用户问题：
                                   {request.Message}
                                   """;
                
                logger.LogDebug("RAG 模式激活：已注入 {Length} 字符的上下文。", genContext.KnowledgeContext.Length);
            }
            else
            {
                // 无上下文模式：直接透传用户问题
                finalUserPrompt = request.Message;
                logger.LogDebug("RAG 模式未激活：仅使用用户原始输入。");
            }
            
            // 将组合后的提示作为单条 User 消息添加
            // 利用近因效应，让模型在读取完长文本后立刻看到问题，提升注意力。
            inputMessages.Add(new ChatMessage(ChatRole.User, finalUserPrompt));

            // 4. 准备执行参数 (ChatOptions)
            // 将动态加载的工具集挂载到本次执行的选项中
            var runOptions = new ChatClientAgentRunOptions
            {
                ChatOptions = new ChatOptions
                {
                    Tools = genContext.Tools // <-- 动态挂载工具
                }
            };

            if (!string.IsNullOrWhiteSpace(genContext.KnowledgeContext))
            {
                runOptions.ChatOptions.Temperature = 0.3f;
            } 

            // 5. 恢复会话状态 (Thread)
            // 从持久化存储中恢复之前的对话历史
            var storeThread = new { storeState = new SessionSoreState(request.SessionId) };
            var agentThread = agent.DeserializeThread(JsonSerializer.SerializeToElement(storeThread));

            // 6. 执行流式生成
            await foreach (var update in agent.RunStreamingAsync(
                               inputMessages, 
                               agentThread, 
                               runOptions, 
                               cancellationToken))
            {
                // 将 Agent 的更新事件（文本块、工具调用状态等）转发到工作流事件流
                // 这样前端就能通过 SSE 收到实时打字机效果
                await context.AddEventAsync(new AgentRunUpdateEvent(Id, update), cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "最终生成阶段发生错误");
            // 发送失败事件，让前端能感知到错误
            await context.AddEventAsync(new ExecutorFailedEvent(Id, e), cancellationToken);
            throw;
        }
    }
}