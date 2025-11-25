using System.Collections.Generic;
using System.Reflection;

namespace Zilor.AICopilot.Services.Common.Plugins;

public class AgentPluginRegistrar : IAgentPluginRegistrar
{
    public List<Assembly> Assemblies { get; } = new();

    public void RegisterPluginFromAssembly(Assembly assembly)
    {
        Assemblies.Add(assembly);
    }
}
