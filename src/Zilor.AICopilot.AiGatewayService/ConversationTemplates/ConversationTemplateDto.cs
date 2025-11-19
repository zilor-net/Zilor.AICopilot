namespace Zilor.AICopilot.AiGatewayService.ConversationTemplates;

public record ConversationTemplateDto
{
    public Guid Id;
    public required string Name;
    public required string Description;
    public required string SystemPrompt;
    public int? MaxTokens;
    public double? Temperature;
    public bool IsEnabled;
}
