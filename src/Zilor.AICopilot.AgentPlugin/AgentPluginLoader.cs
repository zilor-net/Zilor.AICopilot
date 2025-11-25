using System.Reflection;
using Microsoft.Extensions.AI;

namespace Zilor.AICopilot.AgentPlugin;

public class AgentPluginLoader
{
    // 缓存插件实例：Key=插件名, Value=插件实例
    private readonly Dictionary<string, IAgentPlugin> _plugins = new();
    
    // 缓存工具定义：Key=插件名, Value=AITool数组
    private readonly Dictionary<string, AITool[]> _aiTools = new();
    
    // 构造函数注入所有的注册器
    public AgentPluginLoader(IEnumerable<AgentPluginRegistrar> registrars)
    {
        // 1. 汇总所有需要扫描的程序集，去重
        var assemblies = registrars
            .SelectMany(r => r.Assemblies)
            .Distinct()
            .ToList();

        // 2. 扫描并加载
        foreach (var assembly in assemblies)
        {
            LoadPluginsFromAssembly(assembly);
        }
        
    }
    
    private void LoadPluginsFromAssembly(Assembly assembly)
    {
        // 反射查找：实现了 IAgentPlugin 且不是抽象类的具体类
        var pluginTypes = assembly.GetTypes()
            .Where(t =>
                typeof(IAgentPlugin).IsAssignableFrom(t) &&
                t is { IsClass: true, IsAbstract: false });

        foreach (var type in pluginTypes)
        {
            // 创建实例
            var plugin = (IAgentPlugin)Activator.CreateInstance(type)!;
            
            // 存入缓存
            _plugins[plugin.Name] = plugin;
            _aiTools[plugin.Name] = plugin.GetAITools()?.ToArray() ?? [];
        }
    }
    
    /// <summary>
    /// 核心功能：根据名称动态获取工具集。
    /// 支持一次获取多个插件的工具，实现工具的动态混搭。
    /// </summary>
    public AITool[] GetAITools(params string[] names)
    {
        var aiTools = new List<AITool>();
        foreach (var name in names)
        {
            if (_aiTools.TryGetValue(name, out var tools))
            {
                aiTools.AddRange(tools);
            }
        }
        return aiTools.ToArray();
    }
    
    public IAgentPlugin? GetPlugin(string name)
    {
        _plugins.TryGetValue(name, out var plugin);
        return plugin;
    }
}