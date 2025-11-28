using System;
using System.Reflection;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zilor.AICopilot.AgentPlugin;
using Zilor.AICopilot.AiGatewayService.Agents;
using Zilor.AICopilot.AiGatewayService.Plugins;
using Zilor.AICopilot.AiGatewayService.Workflows;

namespace Zilor.AICopilot.AiGatewayService;

public static class DependencyInjection
{
    public static void AddAiGatewayService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        
        builder.Services.AddSingleton<ChatAgentFactory>();
        
        builder.Services.AddHttpClient("OpenAI", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        builder.Services.AddAgentPlugin(registrar =>
        {
            registrar.RegisterPluginFromAssembly(Assembly.GetExecutingAssembly());
        });

        builder.Services.AddSingleton<IntentRoutingAgentBuilder>();
        // builder.Services.AddTransient<IntentSelectionAgentExecutor>();
        //
        // builder.AddWorkflow("my-workflow", (sp, _) =>
        // {
        //     var intentSelectionAgentExecutor = sp.GetRequiredService<IntentSelectionAgentExecutor>();
        //     var workflowBuilder = new WorkflowBuilder(intentSelectionAgentExecutor);
        //     return workflowBuilder.Build();
        // });
    }
}