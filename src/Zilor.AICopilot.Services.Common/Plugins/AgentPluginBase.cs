using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.AI;

namespace Zilor.AICopilot.Services.Common.Plugins;

public abstract class AgentPluginBase : IAgentPlugin
{
    public virtual string Name { get; } 
    
    public virtual string Description { get; protected set; } = string.Empty;

    protected AgentPluginBase()
    {
        Name = GetType().Name;
    }

    public IEnumerable<MethodInfo> GetToolMethods()
    {
        var type = GetType();
        return type.GetMethods(BindingFlags.Instance | BindingFlags.Public)      
            .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null);
    }
    
    public IEnumerable<AITool>? GetAITools()
    {
        var tools = GetToolMethods()
            .Select(method => AIFunctionFactory.Create(method, this));
        return tools;
    }
}