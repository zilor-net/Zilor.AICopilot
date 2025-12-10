using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Zilor.AICopilot.AiGatewayService.Workflows;

public static class IntentWorkflow
{
    public static void AddIntentWorkflow(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<IntentRoutingExecutor>();
        builder.Services.AddTransient<ToolsPackExecutor>();
        builder.Services.AddTransient<KnowledgeRetrievalExecutor>();
        builder.Services.AddTransient<FinalProcessExecutor>();
        
        builder.AddWorkflow(nameof(IntentWorkflow), (sp, key) =>
        {
            var intentRoutingExecutor = sp.GetRequiredService<IntentRoutingExecutor>();
            var toolsPackExecutor = sp.GetRequiredService<ToolsPackExecutor>();
            var finalProcessExecutor = sp.GetRequiredService<FinalProcessExecutor>();
            
            var workflowBuilder = new WorkflowBuilder(intentRoutingExecutor);
            workflowBuilder.WithName(key)
                .AddEdge(intentRoutingExecutor, toolsPackExecutor)
                .AddEdge(toolsPackExecutor, finalProcessExecutor);
            
            return workflowBuilder.Build();
        });
    }
}