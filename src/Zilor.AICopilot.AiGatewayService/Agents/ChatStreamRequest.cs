using System.Runtime.CompilerServices;
using MediatR;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Zilor.AICopilot.AiGatewayService.Models;
using Zilor.AICopilot.AiGatewayService.Workflows;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.Services.Common.Contracts;
using Zilor.AICopilot.Services.Common.Helper;
#pragma warning disable MEAI001

namespace Zilor.AICopilot.AiGatewayService.Agents;

[AuthorizeRequirement("AiGateway.Chat")]
public record ChatStreamRequest(Guid SessionId, string Message, List<string>? CallId) : IStreamRequest<ChatChunk>;

public class ChatStreamHandler(
    IDataQueryService queryService, 
    WorkflowFactory workflowFactory) 
    : IStreamRequestHandler<ChatStreamRequest, ChatChunk>
{
    private static readonly Dictionary<Guid, FinalAgentContext> AgentContexts = new();

    public async IAsyncEnumerable<ChatChunk> Handle(ChatStreamRequest request, [EnumeratorCancellation] CancellationToken ct)
    {
        if (!queryService.Sessions.Any(session => session.Id == request.SessionId))
        {
            throw new Exception("未找到会话");
        }
        
        if (request.CallId != null && request.CallId.Count != 0)
        {
            AgentContexts.TryGetValue(request.SessionId, out var agentContext);
            if (agentContext == null) throw new Exception("未找到会话");

            agentContext.InputText = request.Message;
            agentContext.FunctionApprovalCallIds = request.CallId;
            var workflow = workflowFactory.CreateFinalAgentRunWorkflow();
            await using var workflowRun = await InProcessExecution.StreamAsync(workflow, agentContext, cancellationToken: ct);
            await foreach (var chatChunk in RunWorkflow(workflowRun, request.SessionId, ct))
            {
                yield return chatChunk;
            }

            if (agentContext.FunctionApprovalRequestContents.Count == 0)
            {
                AgentContexts.Remove(request.SessionId);
            }
        }
        else
        {
            var workflow = workflowFactory.CreateIntentWorkflow();
            await using var workflowRun = await InProcessExecution.StreamAsync(workflow, request, cancellationToken: ct);
            await foreach (var chatChunk in RunWorkflow(workflowRun, request.SessionId, ct))
            {
                yield return chatChunk;
            };
        }
    }

    private async IAsyncEnumerable<ChatChunk> RunWorkflow(StreamingRun workflowRun, Guid sessionId, CancellationToken ct)
    {
        await foreach (var workflowEvent in workflowRun.WatchStreamAsync(ct))
        {
            Console.WriteLine(workflowEvent);
            switch (workflowEvent)
            {
                case WorkflowOutputEvent evt:
                    if (evt.Data is FinalAgentContext agentContext && agentContext.FunctionApprovalRequestContents.Count != 0)
                    {
                        AgentContexts.TryAdd(sessionId, agentContext);
                    }
                    break;
                case ExecutorFailedEvent evt:
                    yield return new ChatChunk(evt.ExecutorId, ChunkType.Error, evt.Data?.Message ?? string.Empty);
                    break;
                case AgentRunResponseEvent evt:
                    switch (evt.ExecutorId)
                    {
                        case "IntentRoutingExecutor":
                            yield return new ChatChunk(evt.ExecutorId, ChunkType.Intent, evt.Response.Text);
                            break;
                        case "DataAnalysisExecutor":
                            yield return new ChatChunk(evt.ExecutorId, ChunkType.Widget, evt.Response.Text);
                            break;
                    }
                    break;
                case AgentRunUpdateEvent evt:
                    foreach (var evtContent in evt.Update.Contents)
                    {
                        switch (evtContent)
                        {
                            case TextContent content:
                                yield return new ChatChunk(evt.ExecutorId, ChunkType.Text, content.Text);
                                break;
                            case FunctionCallContent content:
                                var fun = new
                                {
                                    id = content.CallId,
                                    name = content.Name, 
                                    args = content.Arguments
                                };
                                yield return new ChatChunk(evt.ExecutorId, ChunkType.FunctionCall, fun.ToJson());
                                break;
                            case FunctionResultContent content:
                                var result = new
                                {
                                    id = content.CallId,
                                    result = content.Result
                                };
                                yield return new ChatChunk(evt.ExecutorId, ChunkType.FunctionResult,
                                    result.ToJson());
                                break;
                            case FunctionApprovalRequestContent content:
                                var approval = new
                                {
                                    callId = content.FunctionCall.CallId,
                                    name = content.FunctionCall.Name,
                                    args = content.FunctionCall.Arguments
                                };
                                yield return new ChatChunk(evt.ExecutorId, ChunkType.ApprovalRequest,
                                    approval.ToJson());
                                break;
                        }
                    }
                    break;
            }
        }
    }
}