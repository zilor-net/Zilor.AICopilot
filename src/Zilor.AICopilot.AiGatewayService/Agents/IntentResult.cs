using System.Text.Json.Serialization;

namespace Zilor.AICopilot.AiGatewayService.Agents;

/// <summary>
/// 意图识别的标准输出结果
/// </summary>
public record IntentResult
{
    /// <summary>
    /// 识别出的意图标识符 (例如: "General.Chat")
    /// </summary>
    [JsonPropertyName("intent")]
    public string Intent { get; set; } = string.Empty;

    /// <summary>
    /// 置信度 (0.0 - 1.0)，用于后续的逻辑判断
    /// </summary>
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    /// <summary>
    /// 可选：思维链推理过程，用于调试模型为什么这么选
    /// </summary>
    [JsonPropertyName("reasoning")]
    public string? Reasoning { get; set; }
}