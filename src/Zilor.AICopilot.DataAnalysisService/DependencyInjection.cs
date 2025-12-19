using System.Reflection;
using Microsoft.Extensions.Hosting;
using Zilor.AICopilot.AgentPlugin;
using Zilor.AICopilot.Dapper;

namespace Zilor.AICopilot.DataAnalysisService;

public static class DependencyInjection
{
    public static void AddDataAnalysisService(this IHostApplicationBuilder builder)
    {
        builder.AddDapper();
        // 注册插件加载器
        builder.Services.AddAgentPlugin(registrar =>
        {
            registrar.RegisterPluginFromAssembly(Assembly.GetExecutingAssembly());
        });
    }
}