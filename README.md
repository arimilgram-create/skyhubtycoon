# SkyHub Tycoon

A browser-playable prototype for a cozy 2/3 isometric, grid-based airport tycoon game. The player paints terminal and airfield zones, places rule-driven airport objects, manages simple money/staff/utility systems, and schedules flights only when passenger and airfield flows are complete.

## Features

- 24×24 square grid where every floor tile and object snaps to whole-tile footprints.
- 2/3 isometric diorama presentation with colorful floors, raised 3D-style objects, smooth camera rotation controls, grid toggles, roof/cutaway toggles, and a responsive tycoon HUD.
- Build categories for floors, passenger processing, gates, airfield pieces, baggage, comfort, shops, staff, and utilities.
- Placement validation with green/red/yellow previews and human-readable warnings for missing dependencies, invalid zones, overlaps, disconnected floors, insufficient money, and inefficient gate seating.
- Airport systems for passenger route, baggage route, airfield route, staff, power, water, alerts, missions, unlock levels, satisfaction, reputation, and flight scheduling.
- Progression from a tiny airport toward international airport unlocks.

## Run

```bash
npm start
```

Open <http://127.0.0.1:4173/>.

## Test

```bash
npm test
```
