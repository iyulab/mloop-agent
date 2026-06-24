using MLoop.Agent.Cli;
using Xunit;

public class GpuStackChatClientFactoryTests
{
    [Theory]
    [InlineData("GPUSTACK_ENDPOINT")]
    [InlineData("GPUSTACK_API_KEY")]
    [InlineData("GPUSTACK_MODEL")]
    public void FromEnvironment_throws_clear_error_when_var_missing(string varName)
    {
        var saved = new Dictionary<string, string?>
        {
            ["GPUSTACK_ENDPOINT"] = Environment.GetEnvironmentVariable("GPUSTACK_ENDPOINT"),
            ["GPUSTACK_API_KEY"] = Environment.GetEnvironmentVariable("GPUSTACK_API_KEY"),
            ["GPUSTACK_MODEL"] = Environment.GetEnvironmentVariable("GPUSTACK_MODEL"),
        };
        Environment.SetEnvironmentVariable("GPUSTACK_ENDPOINT", "http://localhost");
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

    [Theory]
    // GpuStack's OpenAI-compatible API lives under /v1; the SDK appends operation paths to the
    // endpoint, so a bare host must be normalized to include /v1 or requests hit a 404.
    // Host is irrelevant to the logic — use a neutral placeholder (this repo is public OSS).
    [InlineData("http://gpustack.example:10150", "http://gpustack.example:10150/v1")]
    [InlineData("http://gpustack.example:10150/", "http://gpustack.example:10150/v1")]
    [InlineData("http://gpustack.example:10150/v1", "http://gpustack.example:10150/v1")]
    [InlineData("http://gpustack.example:10150/v1/", "http://gpustack.example:10150/v1")]
    public void NormalizeEndpoint_ensures_v1_suffix(string input, string expected)
        => Assert.Equal(expected, GpuStackChatClientFactory.NormalizeEndpoint(input));
}
