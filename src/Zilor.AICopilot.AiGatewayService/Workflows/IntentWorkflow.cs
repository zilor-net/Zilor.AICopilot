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
        builder.Services.AddTransient<DataAnalysisExecutor>();
        builder.Services.AddTransient<ContextAggregatorExecutor>();
        builder.Services.AddTransient<FinalProcessExecutor>();
        
        builder.AddWorkflow(nameof(IntentWorkflow), (sp, key) =>
        {
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var scope = scopeFactory.CreateScope();
            var intentRouting = scope.ServiceProvider.GetRequiredService<IntentRoutingExecutor>();
            var toolsPack = scope.ServiceProvider.GetRequiredService<ToolsPackExecutor>();
            var knowledgeRetrieval = scope.ServiceProvider.GetRequiredService<KnowledgeRetrievalExecutor>();
            var dataAnalysis = scope.ServiceProvider.GetRequiredService<DataAnalysisExecutor>();
            var aggregator = scope.ServiceProvider.GetRequiredService<ContextAggregatorExecutor>();
            var finalProcess = scope.ServiceProvider.GetRequiredService<FinalProcessExecutor>();
            
            var workflowBuilder = new WorkflowBuilder(intentRouting);
            workflowBuilder.WithName(key)
                // 1. 扇出 (Fan-out): 意图识别 -> [工具打包, 知识检索]
                // IntentRoutingExecutor 输出的 List<IntentResult> 会被广播给 targets 列表中的每一个节点
                .AddFanOutEdge(intentRouting, [toolsPack, knowledgeRetrieval, dataAnalysis])
                // 2. 扇入 (Fan-in): [工具打包, 知识检索] -> 聚合器
                // 聚合器接收来自 sources 列表的所有输出
                .AddFanInEdge([toolsPack, knowledgeRetrieval, dataAnalysis], aggregator)
                // 3. 线性连接: 聚合器 -> 最终处理
                .AddEdge(aggregator, finalProcess);
            
            return workflowBuilder.Build();
        });
    }
}