using System.Text;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zilor.AICopilot.AiGatewayService.Agents;
using Zilor.AICopilot.AiGatewayService.Models;
using Zilor.AICopilot.Services.Common.Contracts;
using Zilor.AICopilot.Services.Common.Helper;
#pragma warning disable MEAI001

namespace Zilor.AICopilot.AiGatewayService.Workflows;

/// <summary>
/// 最终处理执行器
/// 职责：利用聚合后的上下文构建 Agent，注入 RAG 提示词，并执行流式生成。
/// </summary>
public class FinalProcessExecutor(
    IServiceProvider serviceProvider,
    ChatAgentFactory agentFactory, 
    IDataQueryService dataQuery,
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
            var session = await dataQuery.FirstOrDefaultAsync(dataQuery.Sessions.Where(s => s.Id == request.SessionId));
            
            if (session == null) throw new InvalidOperationException("会话不存在");

            // 2. 创建基础 Agent 实例
            // 此时 Agent 拥有的是数据库中定义的静态 System Prompt
            var agent = await agentFactory.CreateAgentAsync(session.TemplateId);
            
            // 3. 构建消息列表
            var inputMessages = new List<ChatMessage>();
            string finalUserPrompt;

            // [修改] RAG/数据增强 上下文注入策略
            // 检查是否存在 知识库上下文 或 数据分析上下文
            bool hasKnowledge = !string.IsNullOrWhiteSpace(genContext.KnowledgeContext);
            bool hasDataAnalysis = !string.IsNullOrWhiteSpace(genContext.DataAnalysisContext);
            bool hasContext = hasKnowledge || hasDataAnalysis;
            
            if (hasContext)
            {
                // 构建混合上下文内容
                var contextBuilder = new StringBuilder();

                if (hasDataAnalysis)
                {
                    
                    contextBuilder.AppendLine("数据库查询结果：");
                    contextBuilder.AppendLine(genContext.DataAnalysisContext);
                    contextBuilder.AppendLine();
                }

                if (hasKnowledge)
                {
                    contextBuilder.AppendLine("知识库检索参考信息：");
                    contextBuilder.AppendLine(genContext.KnowledgeContext);
                    contextBuilder.AppendLine();
                }
                
                finalUserPrompt = $"""
                                   请基于以下参考信息（包含数据库查询结果或检索文档）回答我的问题：

                                   <context>
                                   {contextBuilder}
                                   </context>

                                   回答要求：
                                   1. 引用参考信息时，请标注来源 ID（例如 [^1]）。
                                   2. 针对数据分析结果，请结合用户问题进行自然语言解释。
                                   3. 在回答结尾，如果引用了知识库文档，请生成“参考资料”列表。
                                   4. 如果参考信息不足以回答问题，请直接说明，严禁编造。
                                   5. 保持回答专业、简洁。

                                   我的问题：
                                   {request.Message}
                                   """;
                
                logger.LogDebug("增强模式激活：注入知识({KSize})，注入数据({DSize})。", 
                    genContext.KnowledgeContext?.Length ?? 0, 
                    genContext.DataAnalysisContext?.Length ?? 0);
            }
            else
            {
                // 无上下文模式：直接透传用户问题
                finalUserPrompt = request.Message;
                logger.LogDebug("增强模式未激活：仅使用用户原始输入。");
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

            // 如果有注入任何上下文（知识或数据），都降低温度以保证事实性
            if (hasContext)
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
                if (update.Contents.Any(content => content is FunctionApprovalRequestContent))
                {
                    ApprovalContext.Save(request.SessionId, agent, agentThread);
                }
                // 将 Agent 的更新事件（文本块、工具调用状态等）转发到工作流事件流
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