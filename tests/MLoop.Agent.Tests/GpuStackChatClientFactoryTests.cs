using MLoop.Agent.Cli;
using Xunit;

public class GpuStackChatClientFactoryTests
{
    [Theory]
    [InlineData("GPUSTACK_HOST")]
    [InlineData("GPUSTACK_API_KEY")]
    [InlineData("GPUSTACK_MODEL")]
    public void FromEnvironment_throws_clear_error_when_var_missing(string varName)
    {
        var saved = new Dictionary<string, string?>
        {
            ["GPUSTACK_HOST"] = Environment.GetEnvironmentVariable("GPUSTACK_HOST"),
            ["GPUSTACK_API_KEY"] = Environment.GetEnvironmentVariable("GPUSTACK_API_KEY"),
            ["GPUSTACK_MODEL"] = Environment.GetEnvironmentVariable("GPUSTACK_MODEL"),
        };
        Environment.SetEnvironmentVariable("GPUSTACK_HOST", "http://localhost");
        Environment.SetEnvironmentVariable("GPUSTACK_API_KEY", "key");
        Environment.SetEnvironmentVariable("GPUSTACK_MODEL", "model");
        Environment.SetEnvironmentVariable(varName, null);
        try
        {
            var ex = Assert.Throws<InvalidOperationException>(GpuStackChatClientFactory.FromEnvironment);
            Assert.Contains(varName, ex.Message);
        }
        finally
        {
            foreach (var (k, v) in saved)
                Environment.SetEnvironmentVariable(k, v);
        }
    }
}
