using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Zilor.AICopilot.Core.AiGateway.Aggregates.Sessions;
using Zilor.AICopilot.Services.Common.Contracts;
using Zilor.AICopilot.SharedKernel.Repository;

namespace Zilor.AICopilot.AiGatewayService.Agents;

public class SessionChatMessageStore : ChatMessageStore
{
    private readonly IServiceProvider _serviceProvider;

    public SessionChatMessageStore(IServiceProvider serviceProvider, JsonElement storeState)
    {
        _serviceProvider = serviceProvider;
        if (storeState.ValueKind is JsonValueKind.String)
        {
            ThreadDbKey = storeState.Deserialize<Guid>();
        }
    }

    private Guid? ThreadDbKey { get; set; }
    
    public override async Task<IEnumerable<ChatMessage>> GetMessagesAsync(CancellationToken cancellationToken = new())
    {
        using var scope = _serviceProvider.CreateScope();
        var queryService = scope.ServiceProvider.GetRequiredService<IDataQueryService>();

        // 从数据库查询历史消息
        var queryable = queryService.Messages
            .Where(m => m.SessionId == ThreadDbKey)
            .OrderByDescending(m => m.CreatedAt)
            .Take(50);
        
        var dbMessages = await queryService.ToListAsync(queryable); 
        
        var orderedMessages = dbMessages.OrderBy(m => m.CreatedAt);
        
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

        return chatMessages;
    }

    public override async Task AddMessagesAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = new())
    {
        ThreadDbKey ??= Guid.NewGuid();
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Session>>();

        // 加载聚合根
        var session = await repo.GetByIdAsync(ThreadDbKey, cancellationToken);
        if (session == null) return;

        var hasNewMessage = false;
        foreach (var msg in messages)
        {
            // 将 Agent Role 转换为枚举
            // msg.Role.ToString() 可能返回 "user", "assistant" 等
            var roleStr = msg.Role.ToString().ToLower(); 
            var msgType = roleStr switch
            {
                "user" => MessageType.User,
                "assistant" => MessageType.Assistant,
                "system" => MessageType.System,
                _ => MessageType.Assistant
            };

            // 获取文本内容
            if (string.IsNullOrWhiteSpace(msg.Text)) continue;
            
            session.AddMessage(msg.Text, msgType);
            hasNewMessage = true;
        }

        if (hasNewMessage)
        {
            repo.Update(session);
            await repo.SaveChangesAsync(cancellationToken);
        }
    }

    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return JsonSerializer.SerializeToElement(ThreadDbKey);
    }
}