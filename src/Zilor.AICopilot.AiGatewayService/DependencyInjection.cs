using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zilor.AICopilot.AiGatewayService.Agents;

namespace Zilor.AICopilot.AiGatewayService;

public static class DependencyInjection
{
    public static void AddAiGatewayService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        
        builder.Services.AddScoped<ChatAgentFactory>();
        
        builder.Services.AddHttpClient("OpenAI", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
    }
}