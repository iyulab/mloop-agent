using Microsoft.Extensions.AI;
using MLoop.Agent.Mcp;

namespace MLoop.Agent.Tests.Fakes;

/// <summary>Returns a single stub tool without spawning any process.</summary>
public sealed class StubToolProvider : IMloopToolProvider
{
    public Task<IReadOnlyList<AITool>> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AITool> tools = [AIFunctionFactory.Create(() => "ok", "mloop_info_stub")];
        return Task.FromResult(tools);
    }
}
