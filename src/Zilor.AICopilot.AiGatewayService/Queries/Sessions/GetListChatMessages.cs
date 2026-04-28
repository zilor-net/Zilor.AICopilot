using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Zilor.AICopilot.Core.AiGateway.Aggregates.Sessions;
using Zilor.AICopilot.Services.Common.Contracts;
using Zilor.AICopilot.SharedKernel.Messaging;
using Zilor.AICopilot.SharedKernel.Result;

namespace Zilor.AICopilot.AiGatewayService.Queries.Sessions;

public record GetListChatMessagesQuery(Guid SessionId, int Count, bool IsDesc = true) : IQuery<Result<List<ChatMessage>>>;

public class GetListChatMessagesQueryHandler(
    IDataQueryService queryService) : IQueryHandler<GetListChatMessagesQuery, Result<List<ChatMessage>>>
{
    public async Task<Result<List<ChatMessage>>> Handle(GetListChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var query = queryService.Messages
            .Where(m => m.SessionId == request.SessionId)
            .Take(4);

        query = request.IsDesc ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt);
        
        var messages = await queryService.ToListAsync(query);
        var orderedMessages = messages.OrderBy(m => m.CreatedAt);
        
        // 将实体转换为 Agent 框架的 ChatMessage
        var chatMessages = new List<ChatMessage>();
        
        foreach (var msg in orderedMessages)
        {
            var role = msg.Type switch
            {
                MessageType.User => ChatRole.User,
                MessageType.Assistant =>  ChatRole.Assistant,
                MessageType.System => ChatRole.System,
                _ => ChatRole.User
            };
            chatMessages.Add(new ChatMessage(role, msg.Content));
        }

        return Result.Success(chatMessages);
    }
}