using MLoop.Agent.Cli;
using Xunit;

public class GpuStackChatClientFactoryTests
{
    [Fact]
    public void FromEnvironment_throws_clear_error_when_host_missing()
    {
        var prev = Environment.GetEnvironmentVariable("GPUSTACK_HOST");
        Environment.SetEnvironmentVariable("GPUSTACK_HOST", null);
        try
        {
            var ex = Assert.Throws<InvalidOperationException>(GpuStackChatClientFactory.FromEnvironment);
            Assert.Contains("GPUSTACK_HOST", ex.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GPUSTACK_HOST", prev);
        }
    }
}
