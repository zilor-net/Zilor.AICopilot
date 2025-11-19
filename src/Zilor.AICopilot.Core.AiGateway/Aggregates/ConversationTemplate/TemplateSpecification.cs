namespace Zilor.AICopilot.Core.AiGateway.Aggregates.ConversationTemplate;

public record TemplateSpecification
{
    public int? MaxTokens { get; set; }
    public double? Temperature { get; set; }
}