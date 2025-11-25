using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.AI;

namespace Zilor.AICopilot.AgentPlugin;

public abstract class AgentPluginBase : IAgentPlugin
{
    // 默认实现：直接使用类名作为插件名称
    public virtual string Name { get; } 
    
    public virtual string Description { get; protected set; } = string.Empty;

    protected AgentPluginBase()
    {
        Name = GetType().Name;
    }

    /// <summary>
    /// 核心逻辑：扫描当前类中所有标记了 [Description] 的公共方法。
    /// 只有带有描述的方法才会被视为 AI 工具。
    /// </summary>
    private IEnumerable<MethodInfo> GetToolMethods()
    {
        var type = GetType();
        return type.GetMethods(BindingFlags.Instance | BindingFlags.Public)      
            .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null);
    }
    
    /// <summary>
    /// 利用 Microsoft.Extensions.AI 库，将 C# 方法自动转换为 AITool。
    /// </summary>
    public IEnumerable<AITool>? GetAITools()
    {
        // AIFunctionFactory.Create 是微软提供的工具，
        // 它会读取方法签名、参数类型和 Description 特性，生成 JSON Schema。
        // 'this' 参数确保了当工具被调用时，是在当前插件实例上执行的。
        var tools = GetToolMethods()
            .Select(method => AIFunctionFactory.Create(method, this));
        return tools;
    }
}