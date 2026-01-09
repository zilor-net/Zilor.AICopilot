using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

#pragma warning disable MEAI001

namespace Zilor.AICopilot.AiGatewayService.Workflows.Executors;

/// <summary>
/// Agent 流式运行执行器
/// </summary>
public class FinalAgentRunExecutor(
    ILogger<FinalAgentRunExecutor> logger):
    Executor<FinalAgentContext, FinalAgentContext>("FinalAgentRunExecutor")
{
    public override async ValueTask<FinalAgentContext> HandleAsync(
        FinalAgentContext agentContext, 
        IWorkflowContext context,
        CancellationToken cancellationToken = new())
    {
        try
        {
            List<ChatMessage> message = [];
            if (agentContext.FunctionApprovalRequestContents.Count != 0 && agentContext.FunctionApprovalCallIds.Count != 0)
            {
                foreach (var callId in agentContext.FunctionApprovalCallIds)
                {
                    var requestContent = agentContext.FunctionApprovalRequestContents
                        .FirstOrDefault(rc => rc.FunctionCall.CallId == callId);
                    if (requestContent == null) continue;
                    
                    var response = requestContent.CreateResponse(agentContext.InputText == "批准");
                    message.Add(new ChatMessage(ChatRole.User,[response]));
                    agentContext.FunctionApprovalRequestContents.Remove(requestContent);
                }
                
                agentContext.FunctionApprovalCallIds.Clear();

            } else {
                message.Add(new ChatMessage(ChatRole.User, agentContext.InputText));
            }
            
            await foreach (var update in agentContext.Agent.RunStreamingAsync(
                               message,
                               agentContext.Thread,
                               agentContext.RunOptions,
                               cancellationToken))
            {

                foreach (var content in update.Contents)
                {
                    if (content is FunctionApprovalRequestContent requestContent)
                    {
                        agentContext.FunctionApprovalRequestContents.Add(requestContent);
                    }
                };
                
                // 将 Agent 的更新事件（文本块、工具调用状态等）转发到工作流事件流
                await context.AddEventAsync(new AgentRunUpdateEvent(Id, update), cancellationToken);
            }
            return agentContext;
        }
        catch (Exception e)
        {
            logger.LogError(e, "最终Agent运行阶段发生错误");
            // 发送失败事件，让前端能感知到错误
            await context.AddEventAsync(new ExecutorFailedEvent(Id, e), cancellationToken);
            throw;
        }
    }
}