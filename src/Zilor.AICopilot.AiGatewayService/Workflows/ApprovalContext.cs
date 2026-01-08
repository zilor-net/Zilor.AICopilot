using Microsoft.Agents.AI;

namespace Zilor.AICopilot.AiGatewayService.Workflows;

public static class ApprovalContext
{ 
    private static readonly Dictionary<Guid, AIAgent> AIAgents = new ();
    private static readonly Dictionary<Guid, AgentThread> AgentThreads = new ();
    
    public static void Save(Guid sessionId, AIAgent agent, AgentThread thread)
    {
        if (!AIAgents.TryAdd(sessionId, agent))
        {
            AIAgents[sessionId] = agent;
        }
        
        if (!AgentThreads.TryAdd(sessionId, thread))
        {
            AgentThreads[sessionId] = thread;
        }
    }
    
    public static (AIAgent?, AgentThread?) Resume(Guid sessionId)
    {
        var agent = AIAgents.GetValueOrDefault(sessionId);
        var agentThread = AgentThreads.GetValueOrDefault(sessionId);
        return (agent, agentThread);
    }
}