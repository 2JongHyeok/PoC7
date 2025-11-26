# Repository Guidelines

## Project Structure & Module Organization
- Core Unity content lives in `Assets`; keep gameplay scripts and prefabs near their scenes in `Assets/Scenes`. The input map is centralized in `Assets/InputSystem_Actions.inputactions`.
- Third-party tooling (Odin Inspector) is vendored under `Assets/Plugins/Sirenix`; avoid editing vendor files directly.
- Project-wide configuration is in `ProjectSettings` (Unity 6000.0.61f1) and package manifests in `Packages`; change these through the Unity Editor to reduce merge noise.
- Transient folders (`Logs`, `Temp`, `UserSettings`) are local artifacts and should stay out of commits.

## Build, Test, and Development Commands
- Open the project in Unity 6000.0.61f1: `Unity -projectPath .` or via the Hub. Keep the editor version aligned with `ProjectSettings/ProjectVersion.txt`.
- Visual Studio / Rider users can inspect assemblies with `dotnet build PoC7.sln`, but rely on the Unity Editor for actual player builds.
- To produce a player build from CLI, use a build script entry point (e.g., `BuildPipeline.BuildPlayer`) via batchmode: `Unity -batchmode -nographics -quit -projectPath . -executeMethod BuildScripts.BuildAll`.

## Coding Style & Naming Conventions
- C# in Unity style: 4-space indents, PascalCase for classes/methods, camelCase for locals/parameters, and `_camelCase` for private serialized fields when possible. Keep Unity message names exact (`Awake`, `Start`, `Update`).
- Prefer ScriptableObjects for tunable data and keep them in `Assets/Settings` to separate data from scenes.
- Use Odin attributes only when they improve inspector clarity; do not add runtime dependencies on editor-only assemblies.

## Testing Guidelines
- Use Unity Test Runner; place edit mode tests in `Assets/Tests/EditMode` and play mode tests in `Assets/Tests/PlayMode`. Name files `{Feature}Tests.cs` and methods `Test_{Behavior}`.
- Aim for coverage of input mappings, scene bootstrapping, and serialization of ScriptableObjects. Run via the Test Runner window or CLI: `Unity -batchmode -nographics -quit -projectPath . -runTests -testPlatform editmode`.

## Commit & Pull Request Guidelines
- Commits should be small and focused; while history is minimal, adopt Conventional Commit prefixes (`feat:`, `fix:`, `chore:`) for clarity.
- Include context in messages (scene or system touched, e.g., `feat: add pause menu scene flow`). Avoid committing generated folders.
- PRs should summarize behavior changes, list test coverage (or explicitly note gaps), and include screenshots/gifs for UI-facing tweaks. Link issues/tasks when available and note any Unity version changes.

## Agent-Specific Instructions
- 이 저장소와 관련한 모든 커뮤니케이션(코멘트, 커밋 메시지, PR 설명 등)은 한국어로 작성합니다.
