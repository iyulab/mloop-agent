using IronHive.Agent.Mcp;
using Microsoft.Extensions.AI;

namespace MLoop.Agent.Mcp;

/// <summary>
/// Connects mloop-mcp as an MCP stdio plugin (`node &lt;entry&gt;`) and exposes its tools.
/// This is how mloop-agent dogfoods mloop-mcp.
/// </summary>
public sealed class McpMloopToolProvider(string mcpEntryPath, string? mloopPath = null)
    : IMloopToolProvider, IAsyncDisposable
{
    private readonly McpPluginManager _manager = new();

    public static McpPluginConfig BuildConfig(string mcpEntryPath, string? mloopPath) => new()
    {
        Transport = McpTransportType.Stdio,
        Command = "node",
        Arguments = [mcpEntryPath],
        Environment = mloopPath is null
            ? null
            : new Dictionary<string, string> { ["MLOOP_PATH"] = mloopPath },
    };

    public async Task<IReadOnlyList<AITool>> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        await _manager.ConnectAsync("mloop", BuildConfig(mcpEntryPath, mloopPath), cancellationToken);
        return await _manager.GetToolsAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync() => await _manager.DisconnectAllAsync();
}
