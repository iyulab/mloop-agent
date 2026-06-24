using Microsoft.Extensions.AI;
using MLoop.Agent;
using MLoop.Agent.Tests.Fakes;
using Xunit;

public class MloopAgentTests
{
    private static MloopAgentOptions Opts(IChatClient client) => new()
    {
        ChatClient = client,
        ProjectPath = "C:/kamp/seq004",
    };

    [Fact]
    public async Task RunAsync_returns_assistant_content()
    {
        var agent = await MloopAgent.CreateAsync(Opts(new CapturingChatClient()), new StubToolProvider());

        var response = await agent.RunAsync("이 데이터로 모델 만들어줘");

        Assert.Equal("OK", response.Content);
    }

    [Fact]
    public async Task CreateAsync_wires_system_prompt_and_tools_into_the_loop()
    {
        var capturing = new CapturingChatClient();
        var agent = await MloopAgent.CreateAsync(Opts(capturing), new StubToolProvider());

        await agent.RunAsync("시작");

        // 시스템 프롬프트가 첫 메시지로 주입됨
        var system = Assert.Single(capturing.LastMessages!, m => m.Role == ChatRole.System);
        Assert.Contains("mloop_train", system.Text);
        Assert.Contains("C:/kamp/seq004", system.Text);

        // MCP 도구가 루프에 전달됨
        Assert.NotNull(capturing.LastOptions?.Tools);
        Assert.Contains(capturing.LastOptions!.Tools!, t => t.Name == "mloop_info_stub");
    }

    [Fact]
    public async Task SystemPromptOverride_is_respected()
    {
        var capturing = new CapturingChatClient();
        var options = new MloopAgentOptions
        {
            ChatClient = capturing,
            ProjectPath = "C:/p",
            SystemPromptOverride = "CUSTOM-PROMPT-MARKER",
        };
        var agent = await MloopAgent.CreateAsync(options, new StubToolProvider());

        await agent.RunAsync("시작");

        var system = Assert.Single(capturing.LastMessages!, m => m.Role == ChatRole.System);
        Assert.Equal("CUSTOM-PROMPT-MARKER", system.Text);
    }
}
