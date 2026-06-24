using IronHive.Agent.Loop;
using MLoop.Agent.Cli;
using Xunit;

namespace MLoop.Agent.Tests;

/// <summary>
/// The CLI must surface tool-call and reasoning activity, not just text. A reasoning model
/// driving a long MLOps flow (init→train) emits ToolCallDelta/ThinkingDelta for minutes while
/// TextDelta stays empty — rendering only text leaves the operator staring at a frozen prompt.
/// </summary>
public class AgentStreamRendererTests
{
    private static AgentResponseChunk Text(string s) => new() { TextDelta = s };
    private static AgentResponseChunk Think(string s) => new() { ThinkingDelta = s };
    private static AgentResponseChunk Tool(string? name, string? args, bool complete) =>
        new() { ToolCallDelta = new ToolCallChunk { Id = "call-1", NameDelta = name, ArgumentsDelta = args, IsComplete = complete } };

    [Fact]
    public void Text_delta_passes_through_verbatim()
    {
        var r = new AgentStreamRenderer();
        Assert.Equal("hello", r.Render(Text("hello")));
    }

    [Fact]
    public void Empty_or_noop_chunk_renders_nothing()
    {
        var r = new AgentStreamRenderer();
        Assert.Null(r.Render(Text("")));
        Assert.Null(r.Render(new AgentResponseChunk()));
    }

    [Fact]
    public void Tool_call_deltas_accumulate_into_one_line_on_complete()
    {
        var r = new AgentStreamRenderer();
        Assert.Null(r.Render(Tool("mloop_", null, false)));
        Assert.Null(r.Render(Tool("init", "{\"task\":", false)));
        var line = r.Render(Tool(null, "\"regression\"}", true));

        Assert.NotNull(line);
        Assert.Contains("mloop_init", line);
        Assert.Contains("regression", line);
    }

    [Fact]
    public void Sequential_tool_calls_each_emit_their_own_line()
    {
        var r = new AgentStreamRenderer();
        var first = r.Render(Tool("mloop_info", "{}", true));
        var second = r.Render(Tool("mloop_train", "{}", true));

        Assert.Contains("mloop_info", first);
        Assert.Contains("mloop_train", second);
        Assert.DoesNotContain("mloop_info", second); // state reset between calls
    }

    [Fact]
    public void Reasoning_marker_emitted_once_per_thinking_run()
    {
        var r = new AgentStreamRenderer();
        var first = r.Render(Think("let me"));
        var second = r.Render(Think(" inspect the data"));

        Assert.NotNull(first);           // first thinking delta surfaces a marker
        Assert.Null(second);             // consecutive thinking deltas do not repeat it
    }

    [Fact]
    public void Thinking_marker_resets_after_visible_output()
    {
        var r = new AgentStreamRenderer();
        Assert.NotNull(r.Render(Think("a")));
        Assert.Equal("text", r.Render(Text("text")));
        Assert.NotNull(r.Render(Think("b"))); // new thinking run after text → marker again
    }

    [Fact]
    public void Long_arguments_are_truncated()
    {
        var r = new AgentStreamRenderer();
        var huge = new string('x', 1000);
        var line = r.Render(Tool("mloop_init", huge, true));

        Assert.NotNull(line);
        Assert.True(line!.Length < 500, $"expected truncation, got {line.Length} chars");
    }
}
