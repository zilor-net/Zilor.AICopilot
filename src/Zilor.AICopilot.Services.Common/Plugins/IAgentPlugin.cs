using System.Collections.Generic;
using Microsoft.Extensions.AI;

namespace Zilor.AICopilot.Services.Common.Plugins;

public interface IAgentPlugin
{
    string Name { get; }
    
    string Description { get; }
    
    IEnumerable<AITool>? GetAITools();
}