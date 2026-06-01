# SkyHub Tycoon

SkyHub Tycoon is now a **Unity 2022.3 LTS starter project** for a cozy 2/3 isometric, grid-based airport tycoon. The repository still includes the earlier browser prototype for reference, but the root now contains the Unity project folders Unity Hub expects: `Assets/`, `Packages/`, and `ProjectSettings/`.

## Open it in Unity

1. Install **Unity Hub** and **Unity 2022.3 LTS**. The project version file targets `2022.3.20f1`.
2. In Unity Hub, choose **Add project from disk**.
3. Select this repository folder: `skyhubtycoon`.
4. Open the project.
5. On first import, the editor bootstrap creates the playable starter assets and scene automatically at:
   - `Assets/SkyHubTycoon/Scenes/SkyHubTycoon.unity`
6. If the scene was not generated for any reason, run:
   - **Tools → SkyHub Tycoon → Create Full Starter Project**
7. Open `Assets/SkyHubTycoon/Scenes/SkyHubTycoon.unity` and press **Play**.

## What is included in the Unity project

- A 24×24 grid manager with whole-tile floor and object placement.
- ScriptableObject data models for floor zones and buildable airport objects.
- A placement validator for bounds, money, overlap, floor connectivity, zone restrictions, object prerequisites, queue clearance, adjacency, and gate seating efficiency.
- A 2/3 isometric orthographic camera with pan, zoom, and 90-degree rotation.
- Runtime Unity UI for build tools, brushes, HUD, alerts, staff, systems, unlocks, flight scheduling, bulldoze, and view toggles.
- Starter generated prefabs/materials for floors, previews, and airport objects.
- A simple simulation for money, satisfaction, reputation, passengers, staff, power, water, passenger route, baggage route, airfield route, and flight rewards.

## How to play in Unity

1. Press **Play** in `SkyHubTycoon.unity`.
2. Use the left build menu to choose a floor or airport object.
3. Use brush buttons for floor sizes like `1×1`, `2×2`, `3×3`, and `10×10`.
4. Hover the grid for placement feedback:
   - Green = valid
   - Red = invalid
   - Yellow = allowed but inefficient
5. Build this first-airport flow:
   - Public floor
   - Entrance door on the edge
   - Check-in desk
   - Secure floor
   - Security checkpoint
   - Waiting or gate floor
   - Seating
   - Airfield pavement
   - Small runway
   - Taxiway
   - Gate floor
   - Small boarding gate
6. Click **Schedule next flight** once passenger and airfield systems are online.

## Unity source layout

```text
Assets/Scripts/Data          ScriptableObject definitions and enums
Assets/Scripts/Grid          Grid cells and world/grid conversion
Assets/Scripts/Build         Placement controller, preview, validator, instances
Assets/Scripts/Simulation    Airport state and flight scheduling
Assets/Scripts/UI            Runtime Canvas UI and alerts
Assets/Scripts/Camera        Isometric camera controller
Assets/Scripts/Editor        One-click/auto project bootstrap for scene/assets
```

## Browser prototype

The original browser prototype is still available for design reference:

```bash
npm start
```

Open <http://127.0.0.1:4173/>.

## Repository checks

These checks do not require Unity; they validate the original rule tests and verify that the Unity project structure/scripts exist:

```bash
npm test
```
