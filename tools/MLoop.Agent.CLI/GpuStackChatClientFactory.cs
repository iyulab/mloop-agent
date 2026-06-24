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
        var host = Require("GPUSTACK_HOST");
        var apiKey = Require("GPUSTACK_API_KEY");
        var model = Require("GPUSTACK_MODEL");

        var client = new OpenAIClient(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(host) });

        return client.GetChatClient(model).AsIChatClient();
    }

    private static string Require(string name)
        => Environment.GetEnvironmentVariable(name)
           ?? throw new InvalidOperationException($"{name} 환경변수가 필요합니다.");
}
