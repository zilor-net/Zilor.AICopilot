namespace Zilor.AICopilot.Core.AiGateway.Aggregates.LanguageModel;

public record ModelParameters
{
    public int MaxTokens { get; set; }
    public double Temperature { get; set; } = 0.7;
}