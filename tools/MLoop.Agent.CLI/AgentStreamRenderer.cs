using System.Text;
using IronHive.Agent.Loop;

namespace MLoop.Agent.Cli;

/// <summary>
/// Translates streamed <see cref="AgentResponseChunk"/>s into console-visible text.
///
/// The agent loop streams three kinds of activity: assistant text (<c>TextDelta</c>), reasoning
/// (<c>ThinkingDelta</c>), and tool invocations (<c>ToolCallDelta</c>). Rendering only text makes a
/// reasoning model driving a multi-minute MLOps flow (init→train) look hung — the operator sees a
/// frozen prompt while every <c>mloop_*</c> call happens silently. This surfaces tool calls (which
/// MLOps operation is running, with its arguments) and a lightweight reasoning marker so progress
/// is always visible. Tool <em>results</em> are fed back to the model internally; the model's own
/// text summary reports them.
/// </summary>
public sealed class AgentStreamRenderer
{
    private const int MaxArgsLength = 200;

    private readonly StringBuilder _toolName = new();
    private readonly StringBuilder _toolArgs = new();
    private bool _reasoningMarkerShown;

    /// <summary>Returns the text to write for this chunk, or <c>null</c> when there is nothing to show yet.</summary>
    public string? Render(AgentResponseChunk chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        if (chunk.ToolCallDelta is { } call)
            return RenderToolCall(call);

        if (chunk.TextDelta is { Length: > 0 } text)
        {
            _reasoningMarkerShown = false;
            return text;
        }

        if (chunk.ThinkingDelta is { Length: > 0 })
        {
            if (_reasoningMarkerShown) return null;
            _reasoningMarkerShown = true;
            return "\n💭 추론 중…\n";
        }

        return null;
    }

    private string? RenderToolCall(ToolCallChunk call)
    {
        if (call.NameDelta is { Length: > 0 } name) _toolName.Append(name);
        if (call.ArgumentsDelta is { Length: > 0 } args) _toolArgs.Append(args);

        if (!call.IsComplete) return null;

        var line = $"\n🔧 {_toolName} {Truncate(_toolArgs.ToString())}\n";
        _toolName.Clear();
        _toolArgs.Clear();
        _reasoningMarkerShown = false;
        return line;
    }

    private static string Truncate(string args)
    {
        var trimmed = args.Trim();
        return trimmed.Length <= MaxArgsLength ? trimmed : trimmed[..MaxArgsLength] + "…";
    }
}
