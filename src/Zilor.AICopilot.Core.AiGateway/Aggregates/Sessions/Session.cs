using Zilor.AICopilot.SharedKernel.Domain;

namespace Zilor.AICopilot.Core.AiGateway.Aggregates.Sessions;

public class Session : IAggregateRoot<Guid>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TemplateId { get; set; }

    private readonly List<Message> _messages = [];
    
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();
    
    protected Session() {}
    
    public Session(Guid userId, Guid templateId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TemplateId = templateId;
    }

    public void AddMessage(string content, MessageType type)
    {
        var message = new Message(this, content, type);
        _messages.Add(message);
    }
}