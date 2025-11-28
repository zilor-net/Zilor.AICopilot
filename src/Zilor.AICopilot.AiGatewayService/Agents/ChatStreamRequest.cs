using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using MediatR;
using Microsoft.Extensions.AI;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.Services.Common.Contracts;

namespace Zilor.AICopilot.AiGatewayService.Agents;

[AuthorizeRequirement("AiGateway.Chat")]
public record ChatStreamRequest(Guid SessionId, string Message) : IStreamRequest<string>;

public class ChatStreamHandler(IDataQueryService queryService, IntentRoutingAgentBuilder agentBuilder) : IStreamRequestHandler<ChatStreamRequest, string>
{
    public async IAsyncEnumerable<string> Handle(ChatStreamRequest request, CancellationToken cancellationToken)
    {
        if (!queryService.Sessions.Any(session => session.Id == request.SessionId))
        {
            throw new Exception("未找到会话");
        }

        var agent = await agentBuilder.BuildAsync();
        
        await foreach (var update in agent.RunStreamingAsync(request.Message, cancellationToken: cancellationToken))
        {
            foreach (var content in update.Contents)
            {
                switch (content)
                {
                    case TextContent callContent:
                        yield return callContent.Text;
                        break;
                    case FunctionCallContent callContent:
                        yield return $"\n\n```\n正在执行工具：{callContent.Name} \n请求参数：{JsonSerializer.Serialize(callContent.Arguments)}";
                        break;
                    case FunctionResultContent callContent:
                        yield return $"\n\n执行结果：{JsonSerializer.Serialize(callContent.Result)}\n```\n\n";
                        break;
                }
            }
        }
    }
}