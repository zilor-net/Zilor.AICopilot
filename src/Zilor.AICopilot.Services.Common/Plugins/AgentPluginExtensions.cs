using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zilor.AICopilot.Services.Common.Plugins;

public static class AgentPluginExtensions
{
    public static IServiceCollection AddAgentPlugin(
        this IServiceCollection services, 
        Action<IAgentPluginRegistrar> configure)
    {
        // 构造一个 registrar
        var registrar = new AgentPluginRegistrar();

        // 用户执行操作
        configure(registrar);

        // 多次 AddPlugin 会多次添加 registrar（集合注册）
        services.AddSingleton(registrar);

        // 确保 PluginLoader 只注册一次
        services.TryAddSingleton<AgentPluginLoader>();

        return services;
    }
}
