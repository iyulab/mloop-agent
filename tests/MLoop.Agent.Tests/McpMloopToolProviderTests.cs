using IronHive.Agent.Mcp;
using MLoop.Agent.Mcp;
using Xunit;

public class McpMloopToolProviderTests
{
    [Fact]
    public void BuildConfig_uses_stdio_node_with_entry_path()
    {
        var config = McpMloopToolProvider.BuildConfig("/x/mcp/build/index.js", null);

        Assert.Equal(McpTransportType.Stdio, config.Transport);
        Assert.Equal("node", config.Command);
        Assert.NotNull(config.Arguments);
        Assert.Single(config.Arguments!);
        Assert.Equal("/x/mcp/build/index.js", config.Arguments![0]);
    }

    [Fact]
    public void BuildConfig_passes_mloop_path_via_environment()
    {
        var config = McpMloopToolProvider.BuildConfig("/x/index.js", "C:/lib/mloop.exe");

        Assert.NotNull(config.Environment);
        Assert.Equal("C:/lib/mloop.exe", config.Environment!["MLOOP_PATH"]);
    }

    [Fact]
    public void BuildConfig_omits_environment_when_no_mloop_path()
    {
        var config = McpMloopToolProvider.BuildConfig("/x/index.js", null);
        Assert.Null(config.Environment);
    }
}
