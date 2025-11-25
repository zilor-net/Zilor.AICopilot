using System.Reflection;

namespace Zilor.AICopilot.AgentPlugin;

public interface IAgentPluginRegistrar
{
    // 注册包含插件的程序集
    void RegisterPluginFromAssembly(Assembly assembly);
}

public class AgentPluginRegistrar : IAgentPluginRegistrar
{
    // 存储待扫描的程序集列表
    public List<Assembly> Assemblies { get; } = [];

    public void RegisterPluginFromAssembly(Assembly assembly)
    {
        Assemblies.Add(assembly);
    }
}