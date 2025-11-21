using Zilor.AICopilot.SharedKernel.Domain;

namespace Zilor.AICopilot.Core.AiGateway.Aggregates.Sessions;

public class Message : IEntity<int>
{
    protected Message()
    {
    }

    internal Message(Session session, string content, MessageType type)
    {
        Session = session;
        SessionId = session.Id;
        Content = content;
        Type = type;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid SessionId { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public MessageType Type { get; private set; }

    public virtual Session Session { get; private set; }
    public int Id { get; set; }
}