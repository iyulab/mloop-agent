using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;

namespace MLoop.Agent.Cli;

/// <summary>
/// Builds an OpenAI-compatible <see cref="IChatClient"/> from GpuStack env vars.
/// This is a CLI-side convenience; the core SDK stays provider-agnostic.
/// </summary>
public static class GpuStackChatClientFactory
{
    public static IChatClient FromEnvironment()
    {
        var endpoint = Require("GPUSTACK_ENDPOINT");
        var apiKey = Require("GPUSTACK_API_KEY");
        var model = Require("GPUSTACK_MODEL");

        var client = new OpenAIClient(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(NormalizeEndpoint(endpoint)) });

        return client.GetChatClient(model).AsIChatClient();
    }

    /// <summary>
    /// GpuStack exposes its OpenAI-compatible API under <c>/v1</c>. The OpenAI SDK appends operation
    /// paths (e.g. <c>chat/completions</c>) to the configured endpoint, so a bare host must carry the
    /// <c>/v1</c> segment or requests 404. Appends it when absent; idempotent otherwise.
    /// </summary>
    public static string NormalizeEndpoint(string endpoint)
    {
        var trimmed = endpoint.TrimEnd('/');
        return trimmed.EndsWith("/v1", StringComparison.Ordinal) ? trimmed : $"{trimmed}/v1";
    }

    private static string Require(string name)
        => Environment.GetEnvironmentVariable(name)
           ?? throw new InvalidOperationException($"{name} 환경변수가 필요합니다.");
}
