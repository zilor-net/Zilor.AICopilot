using MediatR;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Zilor.AICopilot.AiGatewayService.Models;
using Zilor.AICopilot.AiGatewayService.Workflows;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.Services.Common.Contracts;
using Zilor.AICopilot.Services.Common.Helper;
#pragma warning disable MEAI001

namespace Zilor.AICopilot.AiGatewayService.Agents;

[AuthorizeRequirement("AiGateway.Chat")]
public record ChatStreamRequest(Guid SessionId, string Message) : IStreamRequest<ChatChunk>;

public class ChatStreamHandler(
    IDataQueryService queryService, 
    [FromKeyedServices(nameof(IntentWorkflow))]Workflow workflow) 
    : IStreamRequestHandler<ChatStreamRequest, ChatChunk>
{
    public async IAsyncEnumerable<ChatChunk> Handle(ChatStreamRequest request, CancellationToken ct)
    {
        if (!queryService.Sessions.Any(session => session.Id == request.SessionId))
        {
            throw new Exception("未找到会话");
        }
        
        await using var run = await InProcessExecution.StreamAsync(workflow, request, cancellationToken: ct);
        await foreach (var workflowEvent in run.WatchStreamAsync(ct))
        {
            switch (workflowEvent)
            {
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
                                    id = content.FunctionCall.CallId,
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