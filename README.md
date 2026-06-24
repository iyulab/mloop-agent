# mloop-agent

MLoop + mloop-mcp 위에서 동작하는 **MLOps AI 에이전트**. 데이터셋을 주면 스무고개 게임처럼
대화하며 task·label·전처리를 결정하여 `init → train → promote` 를 수행한다.
iyulab 데이터/ML 생태계의 Layer 4(mloop-mcp 위) 포지션이며, ironhive-agent 엔진을 dogfooding 한다.

소비앱은 MLoop / MLoop+MCP / MLoop+Agent 중 자유롭게 선택할 수 있다 — mloop-agent 는 그 스택의 최상위(opt-in) 레이어다.

## 구성

| 산출물 | 형태 | 용도 |
|--------|------|------|
| `MLoop.Agent` | NuGet SDK | 소비앱(.NET)이 임베드. `MloopAgent.CreateAsync` 로 사용 |
| `mloop-agent` | dotnet tool | 대화형 CLI (데모 · dogfooding 검증) |

mloop-agent 가 더하는 가치는 두 가지뿐 — **(a) MLOps 스무고개 시스템 프롬프트, (b) mloop-mcp 배선**.
에이전트 루프 · 도구 실행 · HITL · 컨텍스트 압축 · 권한은 전부 [ironhive-agent](https://github.com/iyulab/ironhive-agent)(`IronHive.Agent` NuGet) 재사용.

## 아키텍처

```
소비앱 ──┐
         ├─→ MLoop.Agent (코어 SDK)
 mloop-agent CLI ─┘        │
                           ├─→ IronHive.Agent (AgentLoop)        ← LLM 루프·HITL·컨텍스트
                           └─→ mloop-mcp (MCP stdio 플러그인)     ← MLoop CLI 19개 도구
                                      └─→ mloop CLI (subprocess)  ← 실제 MLOps 실행
```

코어는 **provider-agnostic** — 소비앱이 `IChatClient`(Microsoft.Extensions.AI 호환: Anthropic / OpenAI / GpuStack 등)를 주입한다. CLI 는 GpuStack(OpenAI 호환) env 로 provider 를 구성한다.

## 빠른 시작 (CLI)

전제: .NET 10, Node.js, mloop CLI, 빌드된 mloop-mcp(`npm install && npm run build`).

```bash
export GPUSTACK_ENDPOINT=...     # OpenAI 호환 엔드포인트 (/v1 은 자동 부착)
export GPUSTACK_API_KEY=...
export GPUSTACK_MODEL=...
mloop-agent <projectPath> --mcp <mcp/build/index.js> --mloop <mloop 실행파일>
```

자격증명은 `.env` 파일로도 줄 수 있다 — CLI 가 `--env <경로>`(없으면 cwd→상위 탐색)로 로드한다.
실제 엔드포인트/키는 공개 리포에 두지 말고 로컬 `.env`(gitignore)에 둔다.

```bash
mloop-agent <projectPath> --mcp <index.js> --mloop <mloop> --env ./.env
```

`--mcp` 는 `MLOOP_MCP_PATH`, `--mloop` 는 `MLOOP_PATH` 환경변수로도 지정 가능하다.
실행하면 대화 프롬프트가 뜨고, "datasets/train.csv 로 모델 만들어줘" 처럼 요청하면 에이전트가
`mloop_info` 로 데이터를 분석한 뒤 모호한 결정만 되물으며 모델을 완성한다.

## SDK 사용 (소비앱)

```csharp
using MLoop.Agent;
using MLoop.Agent.Mcp;

await using var tools = new McpMloopToolProvider(mcpEntryPath, mloopPath);
var agent = await MloopAgent.CreateAsync(new MloopAgentOptions
{
    ChatClient  = myChatClient,   // 어떤 Microsoft.Extensions.AI provider든
    ProjectPath = projectPath,
}, tools);

await foreach (var chunk in agent.RunStreamingAsync("이 데이터로 모델 만들어줘"))
    Console.Write(chunk.TextDelta);
```

`MloopAgent` 는 단일 `AgentLoop` 를 보유하므로 `RunAsync`/`RunStreamingAsync` 를 반복 호출하면
대화 이력이 유지된다(멀티턴 스무고개).

## 마일스톤 범위

- ✅ M1 — 스캐폴딩(SDK + CLI) · ironhive-agent + mloop-mcp 배선 · MLOps 스무고개 프롬프트.
- ✅ M2(부분) — `.env` 로딩(`--env`) + 엔드포인트 정규화로 **라이브 LLM E2E** 검증(실 도구 구동으로 `init→train` 확인) · 도구 로딩 관측성(ILogger) + 도구 0개 fail-fast. 단위 테스트(20).
- ⏳ M3 — 자율 재학습 상향(trigger 감시 → 재학습 → compare → promote) · 전체 생명주기 자동화.

## 개발

```bash
dotnet build MLoop.Agent.slnx
dotnet test  MLoop.Agent.slnx
```

.NET 10 / 중앙 패키지 관리(`Directory.Packages.props`). 라이선스: MIT.
