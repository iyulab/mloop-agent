using DotNetEnv;
using Microsoft.Extensions.Logging;
using MLoop.Agent;
using MLoop.Agent.Cli;
using MLoop.Agent.Mcp;

// 사용법: mloop-agent <projectPath> [--mcp <mcp/build/index.js>] [--mloop <mloop.exe>] [--env <.env>]
if (args.Length < 1)
{
    Console.Error.WriteLine("사용법: mloop-agent <projectPath> [--mcp <index.js>] [--mloop <mloop 실행파일>] [--env <.env>]");
    return 1;
}

// .env 적극 활용: 명시 경로(--env) 우선, 없으면 cwd→상위로 탐색.
// (실제 엔드포인트/키는 private 레포의 로컬 .env 에 두고 이 공개 CLI 는 값을 모른다.)
var envPath = GetOption(args, "--env");
if (!string.IsNullOrWhiteSpace(envPath))
    Env.Load(envPath);
else
    Env.TraversePath().Load();

var projectPath = args[0];
var mcpEntry = GetOption(args, "--mcp") ?? Environment.GetEnvironmentVariable("MLOOP_MCP_PATH");
var mloopPath = GetOption(args, "--mloop") ?? Environment.GetEnvironmentVariable("MLOOP_PATH");

if (string.IsNullOrWhiteSpace(mcpEntry))
{
    Console.Error.WriteLine("mloop-mcp 진입점이 필요합니다: --mcp <index.js> 또는 MLOOP_MCP_PATH 환경변수.");
    return 1;
}

using var loggerFactory = LoggerFactory.Create(b => b
    .SetMinimumLevel(LogLevel.Information)
    .AddConsole());

using var chatClient = GpuStackChatClientFactory.FromEnvironment();
await using var toolProvider = new McpMloopToolProvider(
    mcpEntry, mloopPath, loggerFactory.CreateLogger<McpMloopToolProvider>());

var agent = await MloopAgent.CreateAsync(
    new MloopAgentOptions
    {
        ChatClient = chatClient,
        ProjectPath = projectPath,
        MloopPath = mloopPath,
        ModelId = Environment.GetEnvironmentVariable("GPUSTACK_MODEL"),
    },
    toolProvider);

Console.WriteLine("mloop-agent — 데이터셋 경로를 알려주거나 만들고 싶은 모델을 설명하세요. (빈 줄/Ctrl+C 로 종료)");
while (true)
{
    Console.Write("\n> ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;

    await foreach (var chunk in agent.RunStreamingAsync(input))
    {
        if (chunk.TextDelta is { Length: > 0 } delta)
            Console.Write(delta);
    }
    Console.WriteLine();
}
return 0;

static string? GetOption(string[] args, string name)
{
    var i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}
