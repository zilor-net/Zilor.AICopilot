using Microsoft.Extensions.AI;

namespace Zilor.AICopilot.AgentPlugin;

/// <summary>
/// 定义 Agent 插件的标准接口。
/// 所有希望被系统发现的工具集都必须实现此接口。
/// </summary>
public interface IAgentPlugin
{
    /// <summary>
    /// 插件的唯一标识名称（通常使用类名）。
    /// 用于在运行时筛选和查找插件。
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 插件的功能描述。
    /// 可以提供给 Agent 进行“元认知”判断，决定是否需要使用此插件。
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// 获取该插件包含的所有 AI 工具定义（AITool）。
    /// 这些定义将被直接传递给 LLM。
    /// </summary>
    IEnumerable<AITool>? GetAITools();
}