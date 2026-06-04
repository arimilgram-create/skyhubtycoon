# SkyHub Tycoon — Future Agent Guide

Read this file and `GAME_MEMORY.md` before making changes. The project is intentionally in a starter-foundation phase; do not rush into building the full game.

## Mission

Help grow SkyHub Tycoon into a polished cozy 3D 2/3 isometric airport tycoon while preserving the existing project structure and keeping the game easy to preview.

## First steps for every agent

1. Check repository status with `git status --short`.
2. Read `GAME_MEMORY.md` for current vision, completed work, bugs, and next planned features.
3. Read `README.md` and `BUILD_INSTRUCTIONS.md` before changing Unity/WebGL workflow.
4. Inspect relevant files before editing; avoid broad rewrites.
5. Run appropriate checks before committing.

## Current project shape

- Unity 2022.3 LTS WebGL project is the primary production project.
- `Assets/Scripts` contains Unity C# runtime/editor systems.
- `Assets/Scripts/Editor/SkyHubProjectBootstrap.cs` generates starter scene assets and WebGL build settings.
- The browser prototype in `index.html` and `src/` is a design reference and quick preview, not the final production target.
- Node tests in `tests/` validate placement rules and Unity project structure without requiring Unity.

## Development rules

- Do not build the entire game in one change.
- Keep changes small, readable, and aligned with `GAME_MEMORY.md`.
- Preserve WebGL friendliness: lightweight assets, simple materials, and browser-appropriate UI.
- Do not add a quit button or call `Application.Quit` in WebGL UI.
- Keep placement and simulation logic testable.
- Prefer data-driven buildable/floor definitions when adding airport objects.
- Keep the first playable loop focused: entrance → check-in → security → waiting → gate, with taxiway/runway for flights.
- If a new bug or design decision appears, update `GAME_MEMORY.md`.

## Testing expectations

At minimum, run:

```bash
npm test
```

When Unity-specific behavior changes, also open the project in Unity 2022.3 LTS, let the bootstrap run, open `Assets/Scenes/MainScene.unity`, and test Play Mode manually when possible.

## File and structure guidelines

- Keep C# source organized under the existing folders:
  - `Assets/Scripts/Data`
  - `Assets/Scripts/Grid`
  - `Assets/Scripts/Build`
  - `Assets/Scripts/Simulation`
  - `Assets/Scripts/UI`
  - `Assets/Scripts/Camera`
  - `Assets/Scripts/Editor`
- Keep generated or placeholder asset folders under `Assets/Audio`, `Assets/Data`, `Assets/Floors`, `Assets/Materials`, `Assets/Prefabs`, `Assets/Scenes`, and `Assets/UI`.
- Avoid renaming core directories unless tests and documentation are updated in the same change.

## Handoff checklist

Before finishing a change:

- Update `GAME_MEMORY.md` if the vision, completed features, bugs, next steps, or rules changed.
- Run `npm test` unless the change is purely documentation and explain if it cannot run.
- Leave the repository in a clean, committed state when instructed by the task.
- Summarize touched files and checks clearly.
