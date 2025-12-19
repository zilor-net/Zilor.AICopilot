using System.Text;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zilor.AICopilot.AiGatewayService.Agents;
using Zilor.AICopilot.Services.Common.Contracts;

namespace Zilor.AICopilot.AiGatewayService.Workflows;

/// <summary>
/// 数据分析执行器
/// 职责：处理 Analysis.* 意图，实例化 DBA Agent，执行 Text-to-SQL 任务。
/// </summary>
public class DataAnalysisExecutor(
    DataAnalysisAgentBuilder agentBuilder,
    IServiceProvider serviceProvider,
    ILogger<DataAnalysisExecutor> logger)
    : ReflectingExecutor<DataAnalysisExecutor>("DataAnalysisExecutor"),
      IMessageHandler<List<IntentResult>, BranchResult>
{
    private const string AnalysisIntentPrefix = "Analysis.";

    public async ValueTask<BranchResult> HandleAsync(
        List<IntentResult> intentResults, 
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        // 1. 筛选数据分析类意图
        // 过滤规则：必须以 Analysis. 开头，且置信度高于 0.6
        var analysisIntents = intentResults
            .Where(i => i.Intent.StartsWith(AnalysisIntentPrefix, StringComparison.OrdinalIgnoreCase)
                        && i.Confidence > 0.6)
            .ToList();

        if (analysisIntents.Count == 0)
        {
            logger.LogDebug("未检测到数据分析意图，跳过执行。");
            // 返回空结果，表示该分支无产出
            return BranchResult.FromDataAnalysis(string.Empty);
        }

        logger.LogInformation("启动数据分析流程，命中目标数据库数量: {Count}", analysisIntents.Count);

        // 2. 准备并行执行环境
        // 我们需要在一个新的 Scope 中获取数据库服务，避免 DbContext 并发问题
        using var scope = serviceProvider.CreateScope();
        var dataQuery = scope.ServiceProvider.GetRequiredService<IDataQueryService>();

        // 3. 并行执行多库查询
        var tasks = analysisIntents.Select(intent => 
            ProcessSingleIntentAsync(intent, dataQuery, context, cancellationToken));

        var results = await Task.WhenAll(tasks);

        // 4. 结果聚合
        // 将所有成功的查询结果拼接在一起
        var combinedOutput = string.Join("\n\n---\n\n", results.Where(r => !string.IsNullOrWhiteSpace(r)));

        return BranchResult.FromDataAnalysis(combinedOutput);
    }
    
    /// <summary>
    /// 处理单个数据库查询意图
    /// </summary>
    private async Task<string> ProcessSingleIntentAsync(
        IntentResult intent, 
        IDataQueryService dataQuery, 
        IWorkflowContext context,
        CancellationToken ct)
    {
        var dbName = intent.Intent.Substring(AnalysisIntentPrefix.Length);
        
        try
        {
            // 1. 获取数据库配置
            // 我们需要 BusinessDatabase 实体来决定方言策略
            var db = await dataQuery.FirstOrDefaultAsync(
                dataQuery.BusinessDatabases.Where(d => d.Name == dbName));

            if (db == null || !db.IsEnabled)
            {
                logger.LogWarning("意图指向数据库 '{DbName}'，但该库不存在或已禁用。", dbName);
                return $"[系统提示]: 无法连接数据库 {dbName}，请联系管理员核实配置。";
            }

            // 2. 构建 DBA Agent
            // 这里会动态注入 PG 或 SQLServer 的方言提示词
            var agent = await agentBuilder.BuildAsync(db);

            // 3. 构建 Agent 的输入消息
            // 我们明确告知 Agent 当前的目标和任务
            var chatHistory = new List<ChatMessage>
            {
                new(ChatRole.System, $"你现在的任务是针对数据库 '{db.Name}' 回答用户的问题。请直接利用工具查询数据，不要臆测。"),
                new(ChatRole.User, intent.Query)
            };

            // 4. 执行 ReAct 循环
            // Agent 会自动进行: 思考 -> GetTableNames -> 思考 -> GetTableSchema -> 思考 -> ExecuteSQL -> 总结
            
            var outputBuilder = new StringBuilder();
            outputBuilder.AppendLine($"### 数据分析报告 (来源: {db.Name})");
            
            await foreach (var update in agent.RunStreamingAsync(chatHistory, cancellationToken: ct))
            {
                // 遍历当前更新中的所有内容项
                foreach (var content in update.Contents)
                {
                    switch (content)
                    {
                        // 1. 捕获 LLM 发出的工具调用请求
                        case FunctionCallContent call:
                            await context.AddEventAsync(new AgentRunUpdateEvent(Id, update), ct);
                            break;

                        // 2. 捕获工具执行后的结果
                        case FunctionResultContent result:
                            await context.AddEventAsync(new AgentRunUpdateEvent(Id, update), ct);
                            break;

                        // 3. 捕获普通文本回复
                        case TextContent text:
                            outputBuilder.AppendLine(text.Text);
                            await context.AddEventAsync(new AgentRunUpdateEvent(Id, update), ct);
                            break;
                    }
                }
            }

            // 记录日志以便调试
            logger.LogInformation("数据库 {DbName} 查询完成。", dbName);

            return outputBuilder.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "执行数据分析意图失败。Database: {DbName}", dbName);
            return $"[系统错误]: 查询数据库 {dbName} 时发生异常 - {ex.Message}";
        }
    }
}