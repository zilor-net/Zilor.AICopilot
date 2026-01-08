using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Zilor.AICopilot.AiGatewayService.Models;

/// <summary>
/// 该模型用于记录审批请求所需的元数据
/// </summary>
public class RequiresApproval
{
    /// <summary>
    /// 触发审批的工具调用 ID（由 LLM 生成的唯一标识）。
    /// </summary>
    [JsonPropertyName("call_id")]
    public string CallId { get; }
    
    /// <summary>
    /// 插件名称
    /// </summary>
    [JsonPropertyName("plugin_name")]
    public string PluginName { get; }

    /// <summary>
    /// 待调用的函数名称
    /// </summary>
    [JsonPropertyName("function_name")]
    public string FunctionName { get; }
    
    /// <summary>
    /// 待调用的函数定义
    /// </summary>
    [JsonIgnore]
    public AIFunction Function { get; }

    /// <summary>
    /// 待调用的函数参数
    /// </summary>
    [JsonPropertyName("arguments")]
    public AIFunctionArguments Arguments { get; }
    
    public RequiresApproval(
        string callId, 
        string pluginName, 
        string functionName,
        AIFunction function,
        AIFunctionArguments arguments)
    {
        CallId = callId;
        PluginName = pluginName;
        FunctionName = functionName;
        Function = function;
        Arguments = arguments;
    }
}