using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
#pragma warning disable MEAI001

namespace Zilor.AICopilot.AiGatewayService.Workflows;

public class FinalAgentContext
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public required Guid SessionId { get; init; }
    
    /// <summary>
    /// 最终 Agent
    /// </summary>
    public required ChatClientAgent Agent { get; init; }
    
    /// <summary>
    /// 输入消息
    /// </summary>
    public required string InputText { get; set; }
    
    /// <summary>
    /// 暂存的会话线程
    /// </summary>
    public AgentThread? Thread { get; set; }
    
    /// <summary>
    /// Agent 运行选项
    /// </summary>
    public required ChatClientAgentRunOptions RunOptions { get; init; }
    
    /// <summary>
    /// 待审批函数请求
    /// </summary>
    public readonly List<FunctionApprovalRequestContent> FunctionApprovalRequestContents = [];

    /// <summary>
    /// 审批函数调用ID
    /// </summary>
    public List<string> FunctionApprovalCallIds { get; set; } = [];
}