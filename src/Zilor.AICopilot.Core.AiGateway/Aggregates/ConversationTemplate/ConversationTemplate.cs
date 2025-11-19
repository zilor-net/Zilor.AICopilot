using Zilor.AICopilot.SharedKernel.Domain;

namespace Zilor.AICopilot.Core.AiGateway.Aggregates.ConversationTemplate;

public class ConversationTemplate : IAggregateRoot<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; private set; }
    
    public string Description { get; set; }
    
    public string SystemPrompt { get; set; }
    
    public TemplateSpecification Specification { get; private set; }
    
    public bool IsEnabled { get; private set; }
    
    protected ConversationTemplate() { }
    
    public ConversationTemplate(
        string name,
        string description,
        string systemPrompt,
        TemplateSpecification specification)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        SystemPrompt = systemPrompt;
        Specification = specification;
        IsEnabled = true;
    }


    public void UpdateSpecification(TemplateSpecification spec)
    {
        Specification = spec;
    }
}