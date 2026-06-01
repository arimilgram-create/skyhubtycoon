# SkyHub Tycoon

SkyHub Tycoon is a clean, upload-ready **Unity 2022.3 LTS WebGL starter project** for a cozy 2/3 isometric, grid-based airport tycoon. The repository includes the folder structure Unity Hub expects and can generate a playable `MainScene.unity` with no manual asset setup.

## Required Unity version

Use **Unity 2022.3 LTS**. The project version file targets `2022.3.20f1`.

## Project structure

```text
Assets/
  Audio/        Lightweight audio placeholder folder for WebGL-safe compressed audio
  Data/         Generated buildable ScriptableObjects
  Floors/       Generated floor ScriptableObjects
  Materials/   Generated lightweight materials
  Prefabs/      Generated lightweight prefabs
  Scenes/       MainScene.unity is generated here
  Scripts/      Runtime/editor C# source
  UI/           UI placeholder folder
Packages/       Unity package manifest
ProjectSettings/ Unity version, build scene, editor settings, graphics settings
```

## Open the project in Unity Hub

1. Open **Unity Hub**.
2. Choose **Add project from disk**.
3. Select this repository folder: `skyhubtycoon`.
4. Open it with **Unity 2022.3 LTS**.
5. Wait for package import and script compilation.
6. The editor bootstrap automatically creates:
   - `Assets/Scenes/MainScene.unity`
   - starter ScriptableObjects in `Assets/Data` and `Assets/Floors`
   - starter prefabs in `Assets/Prefabs`
   - starter materials in `Assets/Materials`
   - WebGL build settings with `MainScene.unity` as Scene 0
7. If generation does not run automatically, choose **Tools > SkyHub Tycoon > Create Full Starter Project**.

## Test in Unity

1. Open `Assets/Scenes/MainScene.unity`.
2. Press **Play**.
3. Click the start-menu **Play** button.
4. Build a basic airport, then click **Schedule next flight** when passenger and airfield routes are online.

## WebGL-friendly controls

- **Left click**: place the selected floor/object.
- **WASD / Arrow keys**: pan the isometric camera.
- **Mouse wheel**: zoom.
- **Q / E**: rotate the camera in 90-degree steps.
- **Escape**: pause/resume.

The WebGL game has a start menu, controls text, pause menu, resume button, restart button, and main-menu button. It intentionally does **not** show a quit button because browser games should not call `Application.Quit`.

## Build for WebGL

1. Go to **File > Build Settings**.
2. Select **WebGL**.
3. Click **Switch Platform**.
4. Make sure `Assets/Scenes/MainScene.unity` is listed as scene index `0`.
5. Click **Build**.
6. Choose a folder named `WebGLBuild`.
7. Upload the **contents** of `WebGLBuild` to itch.io, GitHub Pages, Netlify, or your own web host.

For exact upload steps, see [`BUILD_INSTRUCTIONS.md`](BUILD_INSTRUCTIONS.md).

## What is included

- 24×24 whole-tile airport grid.
- ScriptableObject floor zones and buildable airport object definitions.
- Placement preview colors: green valid, red invalid, yellow inefficient.
- Placement validation for money, bounds, overlap, floor connectivity, zone restrictions, dependencies, queue space, adjacency, and gate seating efficiency.
- Start menu and pause menu built for browser play.
- Runtime Canvas UI with `Scale With Screen Size` and reference resolution `1920x1080`.
- Lightweight generated primitive prefabs/materials to keep WebGL builds small.
- Simple simulation for money, staff, satisfaction, reputation, power, water, passenger route, baggage route, airfield route, unlocks, and flight rewards.

## Repository checks

These checks do not require Unity; they validate the original JS rule tests and verify the Unity WebGL project structure/scripts exist:

```bash
npm test
```

## Browser prototype reference

The earlier browser prototype is still available for design reference:

```bash
npm start
```

Open <http://127.0.0.1:4173/>.
