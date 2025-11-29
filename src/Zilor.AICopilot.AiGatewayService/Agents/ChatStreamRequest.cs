using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using MediatR;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Zilor.AICopilot.AiGatewayService.Workflows;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.Services.Common.Contracts;

namespace Zilor.AICopilot.AiGatewayService.Agents;

[AuthorizeRequirement("AiGateway.Chat")]
public record ChatStreamRequest(Guid SessionId, string Message) : IStreamRequest<object>;

public class ChatStreamHandler(
    IDataQueryService queryService, 
    [FromKeyedServices(nameof(IntentWorkflow))]Workflow workflow) 
    : IStreamRequestHandler<ChatStreamRequest, object>
{
    public async IAsyncEnumerable<object> Handle(ChatStreamRequest request, CancellationToken cancellationToken)
    {
        if (!queryService.Sessions.Any(session => session.Id == request.SessionId))
        {
            throw new Exception("未找到会话");
        }
        
        await using var run = await InProcessExecution.StreamAsync(workflow, request, cancellationToken: cancellationToken);
        await foreach (var workflowEvent in run.WatchStreamAsync(cancellationToken))
        {
            switch (workflowEvent)
            {
                case ExecutorFailedEvent evt:
                    yield return new { content = $"发生错误：{evt.Data.Message}" };
                    break;
                case AgentRunResponseEvent evt:
                    yield return new
                    {
                        content =
                            $"\n\n\n```json\n //意图分类\n {evt.Response.Text}\n```\n\n"
                    };
                    break;
                case AgentRunUpdateEvent evt:
                    foreach (var content in evt.Update.Contents)
                    {
                        switch (content)
                        {
                            case TextContent callContent:
                                yield return new { content = callContent.Text };
                                break;
                            case FunctionCallContent callContent:
                                yield return new
                                {
                                    content =
                                        $"\n\n```\n正在执行工具：{callContent.Name} \n请求参数：{JsonSerializer.Serialize(callContent.Arguments)}"
                                };
                                break;
                            case FunctionResultContent callContent:
                                yield return new
                                    { content = $"\n\n执行结果：{JsonSerializer.Serialize(callContent.Result)}\n```\n\n" };
                                break;
                        }
                    }
                    break;
            }
        }
    }
}