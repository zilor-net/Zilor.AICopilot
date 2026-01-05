using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace Zilor.AICopilot.McpService;

public class McpServerManager(
    IServiceScopeFactory scopeFactory,
    ILogger<McpServerManager> logger)
    : IHostedService
{
    private readonly IList<McpClient> _mcpClients = [];
    
    public async Task StartAsync(CancellationToken ct)
    {
        logger.LogInformation("MCP Server Manager 启动中...");

        using var scope = scopeFactory.CreateScope();
        var bootstrap = scope.ServiceProvider
            .GetRequiredService<IMcpServerBootstrap>();

        await foreach (var mcpClient in bootstrap.StartAsync(ct))
        {
            _mcpClients.Add(mcpClient);
        }

        logger.LogInformation("MCP Server Manager 启动完成");
    }

    /// <summary>
    /// 应用停止时触发
    /// </summary>
    public async Task StopAsync(CancellationToken ct)
    {
        logger.LogInformation("正在关闭 MCP 服务连接...");

        // 优雅关闭：并行释放所有客户端资源
        // 我们不希望一个客户端的关闭卡死阻碍其他客户端的关闭
        var closeTasks = _mcpClients.Select(async client => 
        {
            try
            {
                // DisposeAsync 会发送关闭信号，对于 Stdio 传输，这会 Kill 掉子进程
                await client.DisposeAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "关闭 MCP 客户端时发生错误");
            }
        });

        await Task.WhenAll(closeTasks);
        
        _mcpClients.Clear();
        logger.LogInformation("所有 MCP 服务资源已释放");
    }
}