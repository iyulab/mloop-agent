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
    public void Build_includes_fe_loop_procedure_directives()
    {
        // 자율 FE 루프 절차: analyze(눈) → prep_plan/features_select(손) → validate(자기검증)
        var prompt = MlopsPrompt.Build(Opts());
        Assert.Contains("mloop_analyze", prompt);
        Assert.Contains("mloop_prep_plan", prompt);
        Assert.Contains("mloop_features_select", prompt);
        Assert.Contains("mloop_validate", prompt);
    }

    [Fact]
    public void Build_injects_project_path()
    {
        var prompt = MlopsPrompt.Build(Opts());
        Assert.Contains("C:/kamp/seq004", prompt);
    }

    [Fact]
    public void Build_instructs_autonomous_fe_no_questions()
    {
        // Phase 3 핵심 계약: FE 결정은 사용자에게 질문하지 않고 규칙으로 자율 결정 (F-10 해소)
        var prompt = MlopsPrompt.Build(Opts());
        Assert.Contains("자율", prompt);
        Assert.Contains("질문하지 말", prompt);
    }

    [Fact]
    public void Build_bakes_six_rule_decision_policy()
    {
        // 검증된 6규칙 결정정책(system-fe-loop)이 default에 baking됐는지 회귀 가드
        var prompt = MlopsPrompt.Build(Opts());
        Assert.Contains("다중공선성", prompt);          // Rule 2
        Assert.Contains("|r|≥0.95", prompt);            // Rule 2 임계값
        Assert.Contains("정규화 금지", prompt);          // Rule 3 (트리 robust)
        Assert.Contains("likely-index", prompt);         // Rule 4 (도구↔프롬프트 짝, F-16)
        Assert.Contains("method=structural", prompt);    // Rule 5 (importance 독해, F-08)
        Assert.Contains("무차별 억제", prompt);          // Rule 6
    }

    [Fact]
    public void Build_enforces_hitl_training_boundary()
    {
        // A안: 자율 FE는 mloop.yaml까지만, train/promote는 사용자 승인 게이트 (자동 train 폭주 방지)
        var prompt = MlopsPrompt.Build(Opts());
        Assert.Contains("mloop_train", prompt);
        Assert.Contains("스스로 실행하지 마세요", prompt);
    }
}
