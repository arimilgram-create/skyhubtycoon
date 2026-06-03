# SkyHub Tycoon — Project Memory

This file is the long-term memory for SkyHub Tycoon. Update it whenever the game direction, systems, known issues, or implementation priorities change.

## 1. Current Game Vision

SkyHub Tycoon is a cozy, polished 3D airport tycoon viewed from a 2/3 isometric camera angle. The player starts with a tiny local airport on a square grid and expands it into a full international airport.

The core fantasy is to design, operate, and grow an airport that feels readable, charming, and alive. Players should be able to paint floors, place functional airport objects, connect passenger and aircraft routes, unlock new services, and improve the airport through smart layout decisions rather than frantic micromanagement.

### Desired player experience

- Build an airport on a square tile grid with clear placement feedback.
- Expand from a small terminal into a multi-service international hub.
- Watch systems come online as the airport gains entrances, check-in, security, waiting areas, gates, runways, taxiways, baggage, shops, bathrooms, staff rooms, and utilities.
- Keep the tone cozy, optimistic, and approachable rather than harsh or punishing.
- Make every new feature easy to understand from the isometric view.

## 2. Tech Stack

### Primary game target

- **Engine:** Unity 2022.3 LTS.
- **Primary platform:** WebGL/browser play.
- **Language:** C# for Unity runtime/editor systems.
- **Camera style:** 3D 2/3 isometric airport view.
- **Build format:** Lightweight WebGL build suitable for itch.io, GitHub Pages, Netlify, or similar hosts.

### Repository support tools

- **Node.js tests:** Used for repository-level validation that does not require Unity.
- **Browser prototype:** A lightweight JavaScript/HTML/CSS reference prototype remains available for quick design preview through `npm start`.
- **Unity bootstrap:** Editor scripts generate starter scene assets, primitive prefabs, materials, ScriptableObjects, and WebGL build settings.

## 3. Core Systems

These systems define the intended foundation of the project. Do not replace them casually; extend them in small, testable steps.

### Grid and placement

- Whole-tile square grid construction.
- Floor painting before most terminal objects can be placed.
- Footprint-based placement for buildable objects.
- Rotation support for oriented objects.
- Placement preview feedback:
  - Green: valid.
  - Red: invalid.
  - Yellow: allowed but inefficient or suboptimal.
- Validation should account for bounds, overlap, available money, floor requirements, zone restrictions, dependencies, queue/adjacency needs, and route connectivity.

### Airport zones and buildables

Planned and/or represented categories include:

- Public terminal floors.
- Secure-side floors.
- Staff/service floors.
- Airfield surfaces.
- Walls and entrances.
- Check-in desks.
- Security checkpoints.
- Waiting seating.
- Gates.
- Runways and taxiways.
- Shops and cafes.
- Bathrooms.
- Baggage systems.
- Staff rooms.
- Utility objects such as power and water support.

### Simulation and progression

- Money, satisfaction, reputation, passenger count, handled flights, and staff should remain readable to the player.
- Flights should only become successful when passenger-facing and airfield-facing routes are sufficiently online.
- Unlocks should gradually introduce complexity as the airport grows.
- Inefficient but functional layouts should be allowed where appropriate, with gentle warnings rather than hard failure.

### UI and controls

- The game needs clear build menus, status panels, alerts, staff readouts, unlock information, and flight scheduling.
- WebGL controls should avoid platform-inappropriate behavior such as quit buttons.
- The start menu, pause menu, restart, resume, and main-menu flows should remain browser-friendly.

## 4. Completed Features

Current repository state includes:

- Unity 2022.3 LTS WebGL starter project folders: `Assets`, `Packages`, and `ProjectSettings`.
- Runtime/editor C# source organized by Data, Grid, Build, Simulation, UI, Camera, and Editor concerns.
- Unity editor bootstrap for creating the first playable prototype scene/assets.
- WebGL build settings pointing at `Assets/Scenes/MainScene.unity`.
- 24×24 square build grid with visible generated grid lines.
- Fixed 3D 2/3 isometric camera with smooth panning, smooth zooming, and 90-degree rotation.
- Runtime build menu with only the first prototype items: basic terminal floor, entrance, check-in desk, security checkpoint, waiting seat, small gate, small runway, and taxiway.
- Money counter and object count HUD.
- Grid-snapped placement with selected-item preview tiles.
- Green valid previews and red invalid previews.
- Placement validation for money, unlocked-land bounds, empty-land floor painting, object overlap prevention, terminal-floor requirements, indoor/exterior placement rules, check-in/security/gate dependencies, runway outdoor spacing, taxiway runway-to-terminal connection, seat path blocking, and passenger floor connectivity.
- Browser prototype reference using `index.html`, `src/game.js`, `src/rules.js`, and `src/styles.css`.
- Node-based validation tests for browser placement rules and Unity project structure.
- Existing README and WebGL build instructions.

## 5. Current Bugs

No confirmed runtime bugs are documented yet.

Known risks to investigate during future implementation:

- Existing generated Unity scenes/assets from earlier versions may need to be regenerated with **Tools > SkyHub Tycoon > Create Full Starter Project** to pick up the simplified first playable prototype catalog.
- Unity-generated scene/assets may need to be regenerated after script changes; keep bootstrap idempotent.
- Browser prototype and Unity project may diverge in rules; treat the browser version as a reference, not the source of truth for final game behavior.
- Placement, route, and economy rules can become brittle if new buildables are added without tests; every new buildable should define explicit valid/invalid placement behavior before being exposed in the menu.
- WebGL performance should be watched carefully as 3D object count, UI, and simulation complexity grow.

## 6. Next Planned Features

Work should proceed in small feature slices rather than attempting the full game at once.

1. Verify the first playable Unity prototype in Unity 2022.3 LTS and regenerate `Assets/Scenes/MainScene.unity` if needed.
2. Improve prototype feel before adding passengers:
   - Better primitive shapes/icons for each build item.
   - Hover labels or clearer selected-item readout.
   - Optional rotate support for rectangular objects/runways, with validation updated for rotated footprints.
   - Camera bounds so panning cannot drift too far from the grid.
   - More visible path/connection feedback for gate ↔ taxiway ↔ runway and entrance/check-in/security flow.
3. Add a minimal save/reset flow for placed prototype objects.
4. Add gentle tutorial objectives for the first route chain, still without spawning passengers.
5. After building feels clean, add placeholder passenger route validation for entrance → check-in → security → waiting → gate.
6. Only after route validation feels good, begin passenger agents and flight scheduling.

## 7. Important Rules and Design Decisions

### Project rules

- Do not attempt to build the complete airport tycoon in one pass.
- Preserve the current Unity WebGL starter structure unless there is a clear reason to change it.
- Keep generated assets lightweight and WebGL-safe.
- Keep repository-level tests runnable without Unity.
- When adding Unity systems, prefer small C# classes with clear responsibilities.
- Update this file whenever project direction, completed features, bugs, or next steps change.

### Gameplay rules

- Build on a square grid using whole-tile footprints.
- Floors establish valid zones for most indoor objects.
- Airfield objects should require airfield-compatible placement.
- The first playable route should be: entrance → check-in → security → waiting → gate, plus runway/taxiway for aircraft operations.
- Placement feedback must explain why a tile is invalid, using clear warnings such as “Must be placed indoors,” “Must connect to terminal,” “Runway must be outdoors,” “Cannot overlap another object,” “Requires security checkpoint first,” “Gate must connect to taxiway,” and “Must be placed on terminal floor.”
- The tone should remain cozy and constructive; avoid overly punitive failure states.

### Design decisions

- Unity is the primary production target; the browser prototype is retained for reference and fast preview.
- The camera should communicate a 3D 2/3 isometric feel, not a flat top-down map.
- WebGL compatibility matters from the beginning.
- Future systems should be data-driven where practical so new buildables can be added without rewriting core placement code.
