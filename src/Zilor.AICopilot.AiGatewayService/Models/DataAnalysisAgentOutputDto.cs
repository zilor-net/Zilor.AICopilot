using System.Text.Json;
using System.Text.Json.Serialization;
using Zilor.AICopilot.Visualization.Widgets;

namespace Zilor.AICopilot.AiGatewayService.Models;

public record DataAnalysisAgentOutputDto
{
    // 对应 Agent 返回的 "Analysis" 数组
    // 这里使用 object 或 JsonElement 都可以，因为我们不需要修改它，只需透传
    [JsonPropertyName("analysis")]
    public JsonElement Analysis { get; set; }

    // 对应 Agent 返回的 "visual_decision" 对象
    [JsonPropertyName("visual_decision")]
    public VisualDecisionDto? Decision { get; set; }
}

public record VisualDecisionDto
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WidgetType Type { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("chart_config")]
    public ChartConfig? ChartConfig { get; set; }
    
    [JsonPropertyName("unit")]
    public string? Unit { get; set; }
}

public record ChartConfig
{
    [JsonPropertyName("category")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChartCategory Category { get; set; }

    [JsonPropertyName("x")]
    public string X { get; set; } = string.Empty;
    
    [JsonPropertyName("y")]
    public string Y { get; set; } = string.Empty;
    
    [JsonPropertyName("series")]
    public string? Series { get; set; }
}
