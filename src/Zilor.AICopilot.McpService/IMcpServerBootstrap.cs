using ModelContextProtocol.Client;

namespace Zilor.AICopilot.McpService;

public interface IMcpServerBootstrap
{
    IAsyncEnumerable<McpClient> StartAsync(CancellationToken cancellationToken);
}