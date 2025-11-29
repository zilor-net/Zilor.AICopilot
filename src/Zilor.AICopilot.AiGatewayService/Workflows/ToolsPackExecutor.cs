using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using Zilor.AICopilot.AgentPlugin;
using Zilor.AICopilot.AiGatewayService.Agents;

namespace Zilor.AICopilot.AiGatewayService.Workflows;

public class ToolsPackExecutor(AgentPluginLoader pluginLoader):
    ReflectingExecutor<ToolsPackExecutor>("ToolsPackExecutor"),
    IMessageHandler<List<IntentResult>, AITool[]>
{
    public async ValueTask<AITool[]> HandleAsync(List<IntentResult> intentResults, IWorkflowContext context,
        CancellationToken cancellationToken = new())
    {
        try
        {
            var intent = intentResults
                .Where(i => i.Confidence >= 0.9)
                .Select(i => i.Intent).ToArray();
            var tools = pluginLoader.GetAITools(intent);
        
            return tools;
        }
        catch (Exception e)
        {
            await context.AddEventAsync(new ExecutorFailedEvent(Id, e), cancellationToken);
            throw;
        }
    }
}