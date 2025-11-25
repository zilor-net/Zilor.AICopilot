using System;
using System.ComponentModel;
using Zilor.AICopilot.Services.Common.Plugins;

namespace Zilor.AICopilot.AiGatewayService.Plugins;

public class TimeAgentPlugin : AgentPluginBase
{
    public override string Description { get; protected set; } = "提供时间相关的功能";
    
    [Description("获取当前系统时间")]
    public string GetCurrentTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}