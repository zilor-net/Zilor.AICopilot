using Zilor.AICopilot.SharedKernel.Domain;

namespace Zilor.AICopilot.Core.AiGateway.Aggregates.ConversationTemplate;

public class ConversationTemplate : IAggregateRoot
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string SystemPrompt { get; set; }
    
    public TemplateSpecification Specification { get; set; }
    
    public bool IsEnabled { get; set; }
    
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