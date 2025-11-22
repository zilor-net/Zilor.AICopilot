using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Zilor.AICopilot.AiGatewayService.Agents;
using Zilor.AICopilot.Core.AiGateway.Aggregates.Sessions;
using Zilor.AICopilot.Services.Common.Attributes;
using Zilor.AICopilot.SharedKernel.Messaging;
using Zilor.AICopilot.SharedKernel.Repository;

namespace Zilor.AICopilot.AiGatewayService.Commands.Sessions;

[AuthorizeRequirement("AiGateway.SendUserMessage")]
public record SendUserMessageCommand(Guid SessionId, string Content) : ICommand<IAsyncEnumerable<string>>;

public class SendUserMessageCommandHandler(IRepository<Session> repo, ChatAgentFactory chatAgent)
    : ICommandHandler<SendUserMessageCommand, IAsyncEnumerable<string>>
{
    public async Task<IAsyncEnumerable<string>> Handle(SendUserMessageCommand request, CancellationToken cancellationToken)
    {
        var session = await repo.GetByIdAsync(request.SessionId, cancellationToken);
        if (session == null) throw new Exception("未找到会话");

        var agent = await chatAgent.CreateAgentAsync(session.TemplateId);
        
        // 返回迭代器函数
        return await Task.FromResult(GetStreamAsync(agent, request.Content, cancellationToken));
    }

    private async IAsyncEnumerable<string> GetStreamAsync(
        ChatClientAgent agent, string content, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var thread = agent.GetNewThread();
        // 调用Agent流式读取响应
        await foreach (var update in agent.RunStreamingAsync(content, thread, cancellationToken: cancellationToken))
        {
            yield return update.Text;
        }

        var json = thread.Serialize();
        Console.WriteLine(json);
    }
}