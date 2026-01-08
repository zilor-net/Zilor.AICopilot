using System.ComponentModel;
using Zilor.AICopilot.AgentPlugin;

namespace Zilor.AICopilot.AiGatewayService.Plugins;

public class SystemOpsPlugin : AgentPluginBase
{
    public override string Description => "提供系统级别的运维操作能力，如时间查询、服务重启等。";
    
    public override IEnumerable<string> HighRiskTools => [nameof(RestartServer)];
    
    [Description("获取当前系统时间")]
    public string GetSystemTime() => DateTime.Now.ToString("O");

    [Description("执行服务器重启操作")]
    public string RestartServer()
    {
        // 实际逻辑中这里会调用 OS 指令
        return "Server restart command issued.";
    }
}