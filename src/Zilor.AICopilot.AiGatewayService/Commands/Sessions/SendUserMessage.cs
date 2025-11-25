using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Zilor.AICopilot.AgentPlugin;
using Zilor.AICopilot.AiGatewayService.Agents;
using Zilor.AICopilot.AiGatewayService.Plugins;
using Zilor.AICopilot.Core.AiGateway.Aggregates.Sessions;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.SharedKernel.Messaging;
using Zilor.AICopilot.SharedKernel.Repository;

namespace Zilor.AICopilot.AiGatewayService.Commands.Sessions;

[AuthorizeRequirement("AiGateway.SendUserMessage")]
public record SendUserMessageCommand(Guid SessionId, string Content) : ICommand<IAsyncEnumerable<string>>;

public class SendUserMessageCommandHandler(
    IRepository<Session> repo,
    ChatAgentFactory chatAgent,
    AgentPluginLoader pluginLoader)
    : ICommandHandler<SendUserMessageCommand, IAsyncEnumerable<string>>
{
    public async Task<IAsyncEnumerable<string>> Handle(SendUserMessageCommand request,
        CancellationToken cancellationToken)
    {
        var session = await repo.GetByIdAsync(request.SessionId, cancellationToken);
        if (session == null) throw new Exception("未找到会话");

        var agent = await chatAgent.CreateAgentAsync(session.TemplateId);
        var storeThread = new { storeState = request.SessionId };
        var agentThread = agent.DeserializeThread(JsonSerializer.SerializeToElement(storeThread));
        
        var tools = pluginLoader.GetAITools(nameof(TimeAgentPlugin));

        // 返回迭代器函数
        return await Task.FromResult(GetStreamAsync(agent, agentThread, tools, request.Content, cancellationToken));
    }

    private async IAsyncEnumerable<string> GetStreamAsync(
        ChatClientAgent agent, AgentThread thread, AITool[] tools, string input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // 调用Agent流式读取响应
        await foreach (var update in agent.RunStreamingAsync(input, thread,
                           new ChatClientAgentRunOptions
                           {
                               ChatOptions = new ChatOptions
                               {
                                   Tools =  tools
                               }
                           }, cancellationToken: cancellationToken))
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