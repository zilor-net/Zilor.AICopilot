using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zilor.AICopilot.AiGatewayService.Workflows.Executors;

namespace Zilor.AICopilot.AiGatewayService.Workflows;

public static class IntentWorkflow
{
    public static void AddIntentWorkflow(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IntentRoutingExecutor>();
        builder.Services.AddScoped<ToolsPackExecutor>();
        builder.Services.AddScoped<KnowledgeRetrievalExecutor>();
        builder.Services.AddScoped<DataAnalysisExecutor>();
        builder.Services.AddScoped<ContextAggregatorExecutor>();
        builder.Services.AddScoped<FinalAgentBuildExecutor>();
        builder.Services.AddScoped<FinalAgentRunExecutor>();
        builder.Services.AddScoped<WorkflowFactory>();

        // builder.AddWorkflow(nameof(IntentWorkflow), (sp, key) =>
        // {
        //     var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
        //     var scope = scopeFactory.CreateScope();
        //     var intentRouting = scope.ServiceProvider.GetRequiredService<IntentRoutingExecutor>();
        //     var toolsPack = scope.ServiceProvider.GetRequiredService<ToolsPackExecutor>();
        //     var knowledgeRetrieval = scope.ServiceProvider.GetRequiredService<KnowledgeRetrievalExecutor>();
        //     var dataAnalysis = scope.ServiceProvider.GetRequiredService<DataAnalysisExecutor>();
        //     var aggregator = scope.ServiceProvider.GetRequiredService<ContextAggregatorExecutor>();
        //     var agentBuild = scope.ServiceProvider.GetRequiredService<FinalAgentBuildExecutor>();
        //     var agentRun = scope.ServiceProvider.GetRequiredService<FinalAgentRunExecutor>();
        //
        //     var workflowBuilder = new WorkflowBuilder(intentRouting);
        //     workflowBuilder.WithName(key)
        //         // 1. 扇出 (Fan-out): 意图识别 -> [工具打包, 知识检索]
        //         // IntentRoutingExecutor 输出的 List<IntentResult> 会被广播给 targets 列表中的每一个节点
        //         .AddFanOutEdge(intentRouting, [toolsPack, knowledgeRetrieval, dataAnalysis])
        //         // 2. 扇入 (Fan-in): [工具打包, 知识检索] -> 聚合器
        //         // 聚合器接收来自 sources 列表的所有输出
        //         .AddFanInEdge([toolsPack, knowledgeRetrieval, dataAnalysis], aggregator)
        //         // 3. 线性连接: 聚合器 -> 最终处理
        //         .AddEdge(aggregator, agentBuild)
        //         .AddEdge(agentBuild, agentRun);
        //
        //     return workflowBuilder.Build();
        // });
    }
}