using Microsoft.Extensions.AI;
using MLoop.Agent;
using MLoop.Agent.Prompts;
using Xunit;

public class MlopsPromptTests
{
    private sealed class NoopChatClient : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> m, ChatOptions? o = null, CancellationToken c = default)
            => Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "")));
        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> m, ChatOptions? o = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken c = default)
        { await Task.CompletedTask; yield break; }
        public object? GetService(Type t, object? k = null) => null;
        public void Dispose() { }
    }

    private static MloopAgentOptions Opts() => new()
    {
        ChatClient = new NoopChatClient(),
        ProjectPath = "C:/kamp/seq004",
    };

    [Fact]
    public void Build_includes_procedure_directives()
    {
        var prompt = MlopsPrompt.Build(Opts());
        Assert.Contains("mloop_info", prompt);
        Assert.Contains("mloop_train", prompt);
        Assert.Contains("mloop_promote", prompt);
    }

    [Fact]
    public void Build_injects_project_path()
    {
        var prompt = MlopsPrompt.Build(Opts());
        Assert.Contains("C:/kamp/seq004", prompt);
    }

    [Fact]
    public void Build_instructs_to_ask_only_ambiguous_decisions()
    {
        var prompt = MlopsPrompt.Build(Opts());
        // 이미 아는 것은 묻지 말라는 핵심 원칙이 포함되어야 함
        Assert.Contains("스무고개", prompt);
    }
}
