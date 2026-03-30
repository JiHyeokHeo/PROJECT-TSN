PROJECT-TSN root instructions

Purpose

* Keep this file short.
* Put only repo-wide rules here.
* Put role-specific rules in agent files.
* Use sub-agents when the task is specialized or spans multiple domains.

Repo summary

* Unity project root: PROJECT-TSN/
* Main game code: Assets/PROJECT-TSN/Scripts/Common/
* Main scene assets: Assets/PROJECT-TSN/Scenes/
* Render/input settings: Assets/PROJECT-TSN/Settings/

Important paths

* Assets/PROJECT-TSN/Scripts/Common/
* Assets/PROJECT-TSN/Scripts/Common/Contents/
* Assets/PROJECT-TSN/Scenes/
* Assets/PROJECT-TSN/Settings/
* Packages/manifest.json
* ProjectSettings/ProjectVersion.txt

Agent files

* .claude/agents/requirement-planner.md
* .claude/agents/game-design-architect.md
* .claude/agents/unity-content-developer.md
* .claude/agents/unity-db-ui-dev.md
* .claude/agents/unity-mcp-tool-dev.md
* .claude/agents/unity-server-dev.md
* .claude/agents/art-asset-manager.md

Core architecture

* BootStrapper / IdleBootstrapper = startup flow and scene initialization.
* GameDataModel = item definition cache and lookup.
* UserDataModel = player state and runtime user data.
* GameManager = core gameplay orchestration.
* SimulationSystem = simulation tick and progression loop.
* GardenScoreManager = garden score calculation.
* FileManager = save/load and file persistence.
* UIManager = UI lifecycle and screen coordination.

Skills usage

* Before writing code, check whether the task should be handled through an existing skill or sub-agent.
* Prefer existing skills, agents, and project patterns over creating new structures from scratch.
* Treat skills as the default path for implementation when the task matches a known domain.
* Reuse existing managers, data models, workflows, and file conventions unless there is a clear reason not to.
* If a task spans multiple domains, choose one primary skill or agent and keep the others limited to supporting work.
* Only implement without a skill when the task is too small, too obvious, or no existing skill clearly applies.

Global rules

* Use namespace TST for game code.
* Unity version: 6000.3.2f1.
* URP version: 17.3.0.
* Do not edit Unity-generated .csproj files.
* Do not hardcode secrets.
* Do not hardcode service ports if config already exists.
* Prefer extending existing systems over adding new singletons.
* Keep save data JSON-safe and backward-compatible.

Agent routing

* Use a sub-agent when the task is specialized, multi-step, or crosses architecture, gameplay, UI, data, server, MCP, or art boundaries.
* Choose the single best-fit agent first.
* Use multiple agents only when the task clearly spans multiple domains.
* If the task is ambiguous, decide from the main deliverable, not from incidental details.
* Keep repo-wide rules in this file.
* Keep role-specific rules in each agent file.
* Do not hand off trivial work to a sub-agent if the answer is obvious from local context.

Agent selection guide

* requirement-planner = requirements, task breakdown, acceptance criteria, implementation plans.
* game-design-architect = feature design, gameplay loops, balance, progression, system-level design.
* unity-content-developer = Unity gameplay features, scenes, prefabs, interactions, game-side implementation.
* unity-db-ui-dev = data models, persistence, UI flows, UI binding, backend-facing client data work.
* unity-mcp-tool-dev = Unity bridge tools, MCP tool definitions, tool payloads, editor integration points.
* unity-server-dev = MCP server, JSON-RPC, process management, stdio, protocol handling, server-side debugging.
* art-asset-manager = asset specs, naming, import rules, art pipeline, ComfyUI-related asset workflow.

Selection rule

* If one agent is clearly primary, use that agent.
* If the task crosses domains, use the primary agent for the main task and consult a second agent only for the dependent part.
* For gameplay feature requests, prefer game-design-architect first for design and unity-content-developer next for implementation.
* For UI tied to save data or data models, prefer unity-db-ui-dev.

Work order

* Read relevant files before changing architecture.
* For new features: data model -> logic -> UI.
* For debugging: state hypothesis, evidence, and fix.

