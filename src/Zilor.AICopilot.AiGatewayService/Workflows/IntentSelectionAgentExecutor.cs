using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Zilor.AICopilot.AiGatewayService.Agents;
using Zilor.AICopilot.AiGatewayService.Commands.Sessions;

namespace Zilor.AICopilot.AiGatewayService.Workflows;

public class IntentSelectionAgentExecutor(IntentSelectionAgentBuilder agentBuilder) :
    ReflectingExecutor<IntentSelectionAgentExecutor>("IntentSelectionAgentExecutor"),
    IMessageHandler<SendUserMessageCommand, List<IntentResult>>
{
    public async ValueTask<List<IntentResult>> HandleAsync(SendUserMessageCommand message, IWorkflowContext context,
        CancellationToken cancellationToken = new())
    {
        
        var agent = await agentBuilder.BuildAsync("IntentSelection");
        var response = await agent.RunAsync<List<IntentResult>>(
            message.Content,
            useJsonSchemaResponseFormat: true,
            cancellationToken: cancellationToken);
        
        return response.Result;
    }
}