using System.Text;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Zilor.AICopilot.AgentPlugin;
using Zilor.AICopilot.AiGatewayService.Agents;

namespace Zilor.AICopilot.AiGatewayService.Workflows;

public class IntentSelectionAgentBuilder
{
    private readonly ChatAgentFactory _agentFactory;

    // 动态构建“意图列表”字符串
    private readonly StringBuilder _intentListBuilder = new();
    
    public IntentSelectionAgentBuilder(ChatAgentFactory agentFactory, AgentPluginLoader pluginLoader)
    {
        _agentFactory = agentFactory;
        // 添加系统内置意图
        _intentListBuilder.AppendLine("- General.Chat: 闲聊、打招呼、情感交互或无法归类的问题。");
        
        // 扫描插件系统，添加业务意图
        // 这里我们假设每个 Plugin 对应一个大类意图，实际项目中可以做得更细致
        var allPlugins = pluginLoader.GetAllPlugin(); 
        foreach (var plugin in allPlugins)
        {
            // 格式：- PluginName: Description
            _intentListBuilder.AppendLine($"- {plugin.Name}: {plugin.Description}");
        }
    }
    
    public async Task<ChatClientAgent> BuildAsync(string templateName)
    {
        var agent = await _agentFactory.CreateAgentAsync(templateName,
            template =>
            {
                // 渲染 System Prompt
                template.SystemPrompt = template.SystemPrompt
                    .Replace("{{$IntentList}}", _intentListBuilder.ToString());
            });
        
        return agent;
    }
}