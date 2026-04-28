using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zilor.AICopilot.AgentPlugin;
using Zilor.AICopilot.Dapper;
using Zilor.AICopilot.DataAnalysisService.Services;
using Zilor.AICopilot.Visualization;

namespace Zilor.AICopilot.DataAnalysisService;

public static class DependencyInjection
{
    public static void AddDataAnalysisService(this IHostApplicationBuilder builder)
    {
        builder.AddDapper();
        builder.Services.AddScoped<VisualizationContext>();
        // 注册插件加载器
        builder.Services.AddAgentPlugin(registrar =>
        {
            registrar.RegisterPluginFromAssembly(Assembly.GetExecutingAssembly());
        });
    }
}