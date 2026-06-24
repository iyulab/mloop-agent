using Microsoft.Extensions.AI;

namespace MLoop.Agent;

/// <summary>
/// Configuration for <see cref="MloopAgent"/>. Provider-agnostic: the consumer supplies the
/// <see cref="IChatClient"/>; mloop-agent never forces a specific LLM provider.
/// </summary>
public sealed class MloopAgentOptions
{
    /// <summary>LLM chat client (any Microsoft.Extensions.AI-compatible provider).</summary>
    public required IChatClient ChatClient { get; init; }

    /// <summary>Absolute path to the MLoop project the agent operates on.</summary>
    public required string ProjectPath { get; init; }

    /// <summary>Model id, used for token pricing/telemetry. Optional.</summary>
    public string? ModelId { get; init; }

    /// <summary>Path to the built mloop-mcp entry (e.g. mcp/build/index.js). Used by the default MCP tool provider.</summary>
    public string? McpEntryPath { get; init; }

    /// <summary>Path to the mloop executable, passed to mloop-mcp via the MLOOP_PATH env var.</summary>
    public string? MloopPath { get; init; }

    /// <summary>Sampling temperature. Optional.</summary>
    public float? Temperature { get; init; }

    /// <summary>Max output tokens. Optional.</summary>
    public int? MaxTokens { get; init; }

    /// <summary>Override the built-in MLOps system prompt. Optional.</summary>
    public string? SystemPromptOverride { get; init; }
}
