using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Zilor.AICopilot.AgentPlugin;
using Zilor.AICopilot.AiGatewayService.Agents;
using Zilor.AICopilot.AiGatewayService.Plugins;
using Zilor.AICopilot.AiGatewayService.Workflows;
using Zilor.AICopilot.Core.AiGateway.Aggregates.Sessions;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.SharedKernel.Messaging;
using Zilor.AICopilot.SharedKernel.Repository;

namespace Zilor.AICopilot.AiGatewayService.Commands.Sessions;

[AuthorizeRequirement("AiGateway.SendUserMessage")]
public record SendUserMessageCommand(Guid SessionId, string Content) : ICommand<IAsyncEnumerable<string>>;

public class SendUserMessageCommandHandler(
    IRepository<Session> repo,
    IntentSelectionAgentBuilder agentBuilder)
    : ICommandHandler<SendUserMessageCommand, IAsyncEnumerable<string>>
{
    public async Task<IAsyncEnumerable<string>> Handle(SendUserMessageCommand request,
        CancellationToken cancellationToken)
    {
        // var session = await repo.GetByIdAsync(request.SessionId, cancellationToken);
        // if (session == null) throw new Exception("未找到会话");
        //
        var agent = await agentBuilder.BuildAsync("意图识别");
        // var storeThread = new { storeState = request.SessionId };
        // var agentThread = agent.DeserializeThread(JsonSerializer.SerializeToElement(storeThread));

        // var run = await InProcessExecution.RunAsync(workflow, request, cancellationToken: cancellationToken);
        // foreach (WorkflowEvent evt in run.NewEvents)
        // {
        //     if (evt is WorkflowCompletedEvent completed)
        //     {
        //         Console.WriteLine($"Final result: {completed.Data}");
        //     }
        // }

        // 返回迭代器函数
        return await Task.FromResult(GetStreamAsync(agent, request.Content, cancellationToken));
    }

    private async IAsyncEnumerable<string> GetStreamAsync(
        ChatClientAgent agent, string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // 调用Agent流式读取响应
        await foreach (var update in agent.RunStreamingAsync(input, cancellationToken: cancellationToken))
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