using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Zilor.AICopilot.AgentPlugin;
using Zilor.AICopilot.AiGatewayService.Models;
using Zilor.AICopilot.Services.Common.Helper;

namespace Zilor.AICopilot.AiGatewayService.Agents;

public class ApprovalMiddleware(
    AgentPluginLoader pluginLoader,
    ILogger<ApprovalMiddleware> logger)
{
    /// <summary>
    /// 记录需要审批的函数调用
    /// </summary>
    public RequiresApproval? RequiresApproval;
    
    public async ValueTask<object?> CheckApprovalRequirementAsync(
        AIAgent agent,
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
        CancellationToken cancellationToken)
    {
        // 1. 获取插件名称和函数名称
        var fullname = context.CallContent.Name.Split(".");
        var pluginName = fullname[0];
        var functionName = fullname[1];
        logger.LogInformation("开始检查函数调用：{Plugin}.{Function}", pluginName, functionName);

        // 2. 从插件加载器中查找对应的插件定义
        var plugin = pluginLoader.GetPlugin(pluginName)!;
        
        // 3. 检查该函数是否在插件的 HighRiskTools 列表中
        if (plugin.HighRiskTools != null && plugin.HighRiskTools.Contains(functionName))
        {
            logger.LogWarning("拦截到高风险操作：{Plugin}.{Function}, CallId: {Id}",
                pluginName, functionName, context.CallContent.CallId);
        
            // 4. Terminate 表示是否立即终止循环。
            context.Terminate = true;
            // 5. 添加需要审批的函数调用信息
            RequiresApproval = new RequiresApproval(
                context.CallContent.CallId,
                pluginName,
                functionName,
                context.Function,
                context.Arguments);
            return Task.CompletedTask;
        }
        
        // 调用下一个中间件（继续执行后续操作，并正常返回）
        var result = await next(context, cancellationToken);
        return result;
    }
}