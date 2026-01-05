using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using Zilor.AICopilot.AgentPlugin;
using Zilor.AICopilot.Core.McpServer.Aggregates.McpServerInfo;
using Zilor.AICopilot.McpService;
using Zilor.AICopilot.Services.Common.Contracts;

public class McpServerBootstrap(
    IDataQueryService dataQueryService,
    AgentPluginLoader agentPluginLoader,
    ILogger<McpServerBootstrap> logger)
    : IMcpServerBootstrap
{
    public async IAsyncEnumerable<McpClient> StartAsync([EnumeratorCancellation] CancellationToken ct)
    {
        var query = dataQueryService.McpServerInfos
            .Where(m => m.IsEnabled);

        var mcpServerInfos = await dataQueryService.ToListAsync(query);

        foreach (var mcpServerInfo in mcpServerInfos)
        {
            McpClient mcpClient = null!;
            switch (mcpServerInfo.TransportType)
            {
                case McpTransportType.Stdio:
                    mcpClient = await CreateStdioClientAsync(mcpServerInfo, ct);
                    yield return mcpClient;
                    break;
                case McpTransportType.Sse:
                    mcpClient = await CreateSseClientAsync(mcpServerInfo, ct);
                    yield return mcpClient;
                    break;
            }

            logger.LogInformation(
                "已连接到 MCP 服务器 - {Name}",
                mcpServerInfo.Name);

            var tools = await mcpClient.ListToolsAsync(
                cancellationToken: ct);

            logger.LogInformation(
                "已发现 {ToolsCount} 个工具",
                tools.Count);
            
            RegisterMcpPlugin(mcpServerInfo, tools);

            logger.LogInformation(
                "已注册 MCP 插件 - {Name}",
                mcpServerInfo.Name);

            yield return mcpClient;
        }
    }

    private void RegisterMcpPlugin(McpServerInfo mcpServerInfo, IEnumerable<AITool> tools)
    {
        var mcpPlugin = new GenericBridgePlugin
        {
            Name = mcpServerInfo.Name,
            Description = mcpServerInfo.Description,
            AITools = tools
        };

        agentPluginLoader.RegisterAgentPlugin(mcpPlugin);
    }

    private async Task<McpClient> CreateStdioClientAsync(McpServerInfo mcpServerInfo, CancellationToken ct)
    {
        var transportOptions = new StdioClientTransportOptions
        {
            Command = "npx",
            Arguments = mcpServerInfo.Arguments.Split(' ')
        };

        var transport = new StdioClientTransport(transportOptions);
        return await McpClient.CreateAsync(
            transport,
            cancellationToken: ct);
    }
    
    private async Task<McpClient> CreateSseClientAsync(McpServerInfo mcpServerInfo, CancellationToken ct)
    {
        var transportOptions = new HttpClientTransportOptions
        {
            Endpoint = new Uri(mcpServerInfo.Arguments)
        };

        var transport = new HttpClientTransport(transportOptions);
        return await McpClient.CreateAsync(
            transport,
            cancellationToken: ct);
    }
}