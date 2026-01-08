using Microsoft.Extensions.AI;

namespace Zilor.AICopilot.AgentPlugin;

public class GenericBridgePlugin : IAgentPlugin
{
    public required string Name { get; init; }
    
    public required string Description { get; init; }
    
    public IEnumerable<AITool>? AITools { get; init; }
    
    public IEnumerable<string>? HighRiskTools { get; init; }
    
    public IEnumerable<AITool>? GetAITools()
    {
        return AITools;
    }
}