using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Zilor.AICopilot.AiGatewayService.Agents;
using Zilor.AICopilot.Services.Common.Contracts;

namespace Zilor.AICopilot.AiGatewayService.Workflows;

public class FinalProcessExecutor(ChatAgentFactory agentFactory, IServiceProvider serviceProvider):
    ReflectingExecutor<FinalProcessExecutor>("FinalProcessExecutor"),
    IMessageHandler<AITool[]>
{
    public async ValueTask HandleAsync(AITool[] aiTools, IWorkflowContext context,
        CancellationToken cancellationToken = new())
    {
        try
        {
            var request = await context.ReadStateAsync<ChatStreamRequest>("ChatStreamRequest", "Chat", cancellationToken: cancellationToken);
            if (request == null) return;

            var scope = serviceProvider.CreateScope();
            var queryService = scope.ServiceProvider.GetRequiredService<IDataQueryService>();
            var queryable = queryService.Sessions
                .Where(s => s.Id == request.SessionId)
                .Select(s => s.TemplateId);
        
            var templateId = queryable.FirstOrDefault();
        
            var agent = await agentFactory.CreateAgentAsync(templateId);
            var storeThread = new { storeState = new SessionSoreState(request.SessionId) };
            var agentThread = agent.DeserializeThread(JsonSerializer.SerializeToElement(storeThread));

            await foreach (var update in agent.RunStreamingAsync(request.Message, agentThread, new ChatClientAgentRunOptions
                           {
                               ChatOptions = new ChatOptions
                               {
                                   Tools = aiTools
                               }
                           }, cancellationToken))
            {
                await context.AddEventAsync(new AgentRunUpdateEvent(Id, update), cancellationToken);
            }
            
        }
        catch (Exception e)
        {
            await context.AddEventAsync(new ExecutorFailedEvent(Id, e), cancellationToken);
            throw;
        }
    }
}