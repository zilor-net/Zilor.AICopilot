using System.Reflection;

namespace Zilor.AICopilot.Services.Common.Plugins;

public interface IAgentPluginRegistrar
{
    void RegisterPluginFromAssembly(Assembly assembly);
}