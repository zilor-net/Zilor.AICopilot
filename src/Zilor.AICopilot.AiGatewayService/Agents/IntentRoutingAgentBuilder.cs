using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using Zilor.AICopilot.AgentPlugin;
using Zilor.AICopilot.Services.Common.Contracts;

namespace Zilor.AICopilot.AiGatewayService.Agents;

public class IntentRoutingAgentBuilder
{
    private const string AgentName = "IntentRoutingAgent";
    
    private readonly ChatAgentFactory _agentFactory;
    private readonly IServiceProvider _serviceProvider;

    // 动态构建“意图列表”字符串
    private readonly StringBuilder _toolIntentListBuilder = new();
    
    public IntentRoutingAgentBuilder(ChatAgentFactory agentFactory, AgentPluginLoader pluginLoader, IServiceProvider serviceProvider)
    {
        _agentFactory = agentFactory;
        _serviceProvider = serviceProvider;
        // 添加系统内置意图
        _toolIntentListBuilder.AppendLine("- General.Chat: 闲聊、打招呼、情感交互或无法归类的问题。");
        
        // 扫描插件系统，添加工具意图
        // 这里我们假设每个 Plugin 对应一个大类意图，实际项目中可以做得更细致
        var allPlugins = pluginLoader.GetAllPlugin(); 
        foreach (var plugin in allPlugins)
        {
            // 格式：- Action.{PluginName}: {Description}
            _toolIntentListBuilder.AppendLine($"- Action.{plugin.Name}: {plugin.Description}");
        }
    }
    
    /// <summary>
    /// 获取知识库意图列表
    /// </summary>
    /// <returns></returns>
    private async Task<StringBuilder> GetKnowledgeIntentListAsync()
    {
        var sb = new StringBuilder();
        
        // 从数据库获取所有启用的知识库
        using var scope = _serviceProvider.CreateScope();
        var dataQuery = scope.ServiceProvider.GetRequiredService<IDataQueryService>();
        
        // 查询知识库列表
        var kbs = await dataQuery.ToListAsync(dataQuery.KnowledgeBases);
        foreach (var kb in kbs)
        {
            // 格式：- Knowledge.{KbName}: {Description}
            sb.AppendLine($"- Knowledge.{kb.Name}: {kb.Description}");
        }

        return sb;
    }
    
    public async Task<ChatClientAgent> BuildAsync()
    {
        var intents = new StringBuilder();
        intents.Append(_toolIntentListBuilder);
        intents.Append(await GetKnowledgeIntentListAsync());
        
        var agent = await _agentFactory.CreateAgentAsync(AgentName,
            template =>
            {
                // 渲染 System Prompt
                template.SystemPrompt = template.SystemPrompt
                    .Replace("{{$IntentList}}", intents.ToString());
            });
        
        return agent;
    }
}