using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.AI;

namespace Zilor.AICopilot.Services.Common.Plugins;

public class AgentPluginLoader
{
    private readonly Dictionary<string, IAgentPlugin> _plugins = new();
    private readonly Dictionary<string, AITool[]> _aiTools = new();
    
    public AgentPluginLoader(IEnumerable<AgentPluginRegistrar> registrars)
    {
        var assemblies = registrars
            .SelectMany(r => r.Assemblies)
            .Distinct()
            .ToList();

        foreach (var assembly in assemblies)
        {
            LoadPluginsFromAssembly(assembly);
        }
    }
    
    private void LoadPluginsFromAssembly(Assembly assembly)
    {
        var pluginTypes = assembly.GetTypes()
            .Where(t =>
                typeof(IAgentPlugin).IsAssignableFrom(t) &&
                t.IsClass &&
                !t.IsAbstract);

        foreach (var type in pluginTypes)
        {
            var plugin = (IAgentPlugin)Activator.CreateInstance(type)!;
            _plugins[plugin.Name] = plugin;
            _aiTools[plugin.Name] = plugin.GetAITools()?.ToArray() ?? [];
        }
    }
    
    public IAgentPlugin? GetPlugin(string name)
    {
        _plugins.TryGetValue(name, out var plugin);
        return plugin;
    }
    
    public AITool[] GetAITools(params string[] names)
    {
        var aiTools = new List<AITool>();
        foreach (var name in names)
        {
            aiTools.AddRange(_aiTools[name]);
        }
        
        return aiTools.ToArray();
    }
    
    public IReadOnlyDictionary<string, IAgentPlugin> GetAllPlugins() => _plugins;
}
