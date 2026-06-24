using Microsoft.Extensions.AI;

namespace MLoop.Agent.Mcp;

/// <summary>Supplies the AI tools the agent can call to drive MLoop.</summary>
public interface IMloopToolProvider
{
    Task<IReadOnlyList<AITool>> GetToolsAsync(CancellationToken cancellationToken = default);
}
