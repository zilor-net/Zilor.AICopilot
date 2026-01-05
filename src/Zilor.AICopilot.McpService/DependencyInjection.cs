using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Zilor.AICopilot.McpService;

public static class DependencyInjection
{
    public static void AddMcpService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IMcpServerBootstrap, McpServerBootstrap>();
        builder.Services.AddHostedService<McpServerManager>();
    }
}