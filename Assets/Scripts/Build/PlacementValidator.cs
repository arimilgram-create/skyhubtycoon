// Validates prototype placement: unlocked land, overlap, floor/indoor rules, airfield connections, and passenger path blockers.
using System.Collections.Generic;
using UnityEngine;
using SkyHubTycoon.Data;
using SkyHubTycoon.Grid;
using SkyHubTycoon.Simulation;

namespace SkyHubTycoon.Build
{
    public class PlacementValidator
    {
        private readonly GridManager grid;
        private readonly AirportState airport;

        public PlacementValidator(GridManager grid, AirportState airport)
        {
            this.grid = grid;
            this.airport = airport;
        }

        public PlacementResult ValidateFloor(FloorDefinition floor, Vector2Int origin, Vector2Int brushSize)
        {
            List<Vector2Int> footprint = grid.GetFootprint(origin, brushSize);
            Vector2Int[] cells = footprint.ToArray();

            if (airport.money < floor.cost * footprint.Count) return PlacementResult.Invalid("Not enough money for this floor brush.", cells);
            for (int i = 0; i < footprint.Count; i++)
            {
                if (!grid.InBounds(footprint[i])) return PlacementResult.Invalid("Nothing can be placed outside unlocked land.", cells);
                GridCell cell = grid.GetCell(footprint[i]);
                if (cell.HasFloor || cell.HasBuildable) return PlacementResult.Invalid("Floors can only be placed on empty unlocked land.", cells);
            }

            return PlacementResult.Valid("Paint " + floor.displayName + ".", cells, false);
        }

        public PlacementResult ValidateBuildable(BuildableDefinition definition, Vector2Int origin)
        {
            List<Vector2Int> footprint = grid.GetFootprint(origin, definition.size);
            Vector2Int[] cells = footprint.ToArray();

            if (airport.money < definition.cost) return PlacementResult.Invalid("Not enough money for " + definition.displayName + ".", cells);
            if (!FootprintIsInBounds(footprint)) return PlacementResult.Invalid("Nothing can be placed outside unlocked land.", cells);
            if (FootprintOverlapsObject(footprint)) return PlacementResult.Invalid("Cannot overlap another object.", cells);

            switch (definition.type)
            {
                case BuildableType.Entrance: return ValidateEntrance(definition, origin, cells);
                case BuildableType.CheckIn: return ValidateCheckIn(definition, origin, cells);
                case BuildableType.Security: return ValidateSecurity(definition, origin, cells);
                case BuildableType.Seating: return ValidateSeating(definition, origin, cells);
                case BuildableType.SmallGate: return ValidateSmallGate(definition, origin, cells);
                case BuildableType.Runway: return ValidateRunway(definition, origin, cells);
                case BuildableType.Taxiway: return ValidateTaxiway(definition, origin, cells);
                default: return ValidateIndoorTerminalObject(definition, origin, cells);
            }
        }

        private PlacementResult ValidateEntrance(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!FootprintIsOnAllowedFloor(cells, definition)) return PlacementResult.Invalid("Must be placed on terminal floor.", cells);
            if (!FootprintTouchesTerminalEdge(cells)) return PlacementResult.Invalid("Must connect to terminal.", cells);
            if (WouldBlockPassengerPaths(cells)) return PlacementResult.Invalid("Cannot block all passenger paths.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateCheckIn(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!FootprintIsOnAllowedFloor(cells, definition)) return PlacementResult.Invalid("Must be placed on terminal floor.", cells);
            if (!FootprintIsIndoors(cells)) return PlacementResult.Invalid("Must be placed indoors.", cells);
            if (WouldBlockPassengerPaths(cells)) return PlacementResult.Invalid("Cannot block all passenger paths.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateSecurity(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!airport.HasBuildable(BuildableType.CheckIn)) return PlacementResult.Invalid("Requires check-in desk first.", cells);
            if (!FootprintIsOnAllowedFloor(cells, definition)) return PlacementResult.Invalid("Must be placed on terminal floor.", cells);
            if (!FootprintIsIndoors(cells)) return PlacementResult.Invalid("Must be placed indoors.", cells);
            if (WouldBlockPassengerPaths(cells)) return PlacementResult.Invalid("Cannot block all passenger paths.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateSeating(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!FootprintIsOnAllowedFloor(cells, definition)) return PlacementResult.Invalid("Must be placed on terminal floor.", cells);
            if (!FootprintIsIndoors(cells)) return PlacementResult.Invalid("Must be placed indoors.", cells);
            if (WouldBlockPassengerPaths(cells)) return PlacementResult.Invalid("Cannot block main paths.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateSmallGate(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!airport.HasBuildable(BuildableType.Security)) return PlacementResult.Invalid("Requires security checkpoint first.", cells);
            if (!FootprintIsOnAllowedFloor(cells, definition)) return PlacementResult.Invalid("Must be placed on terminal floor.", cells);
            if (!FootprintTouchesTerminalEdge(cells)) return PlacementResult.Invalid("Gate must face outside.", cells);
            if (!FootprintHasAdjacentBuildable(cells, BuildableType.Taxiway)) return PlacementResult.Invalid("Gate must connect to taxiway.", cells);
            if (WouldBlockPassengerPaths(cells)) return PlacementResult.Invalid("Cannot block all passenger paths.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateRunway(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!RunwayIsStraight(definition)) return PlacementResult.Invalid("Runway must be straight.", cells);
            if (!FootprintIsOpenGround(cells)) return PlacementResult.Invalid("Runway must be outdoors.", cells);
            if (FootprintTouchesAnyFloor(cells)) return PlacementResult.Invalid("Runway must be outdoors.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateTaxiway(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!FootprintIsOpenGround(cells)) return PlacementResult.Invalid("Taxiway must be outdoors.", cells);
            if (!FootprintHasAdjacentBuildable(cells, BuildableType.Runway)) return PlacementResult.Invalid("Taxiway must connect to runway.", cells);
            if (!FootprintTouchesTerminalEdge(cells) && !FootprintHasAdjacentBuildable(cells, BuildableType.SmallGate)) return PlacementResult.Invalid("Must connect to terminal.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateIndoorTerminalObject(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!FootprintIsOnAllowedFloor(cells, definition)) return PlacementResult.Invalid("Must be placed on terminal floor.", cells);
            if (!FootprintIsIndoors(cells)) return PlacementResult.Invalid("Must be placed indoors.", cells);
            if (WouldBlockPassengerPaths(cells)) return PlacementResult.Invalid("Cannot block all passenger paths.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private bool FootprintIsInBounds(List<Vector2Int> footprint)
        {
            for (int i = 0; i < footprint.Count; i++)
            {
                if (!grid.InBounds(footprint[i])) return false;
            }
            return true;
        }

        private bool FootprintOverlapsObject(List<Vector2Int> footprint)
        {
            for (int i = 0; i < footprint.Count; i++)
            {
                GridCell cell = grid.GetCell(footprint[i]);
                if (cell != null && cell.HasBuildable) return true;
            }
            return false;
        }

        private bool FootprintIsOnAllowedFloor(Vector2Int[] cells, BuildableDefinition definition)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (!CellHasAllowedFloor(grid.GetCell(cells[i]), definition)) return false;
            }
            return true;
        }

        private bool FootprintIsOpenGround(Vector2Int[] cells)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                GridCell cell = grid.GetCell(cells[i]);
                if (cell == null || cell.HasFloor || cell.HasBuildable) return false;
            }
            return true;
        }

        private bool FootprintIsIndoors(Vector2Int[] cells)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                foreach (Vector2Int neighbor in grid.GetCardinalNeighbors(cells[i]))
                {
                    GridCell neighborCell = grid.GetCell(neighbor);
                    if (neighborCell == null || !neighborCell.HasFloor) return false;
                }

                if (cells[i].x == 0 || cells[i].y == 0 || cells[i].x == grid.Width - 1 || cells[i].y == grid.Height - 1) return false;
            }
            return true;
        }

        private bool FootprintTouchesTerminalEdge(Vector2Int[] cells)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i].x == 0 || cells[i].y == 0 || cells[i].x == grid.Width - 1 || cells[i].y == grid.Height - 1) return true;

                foreach (Vector2Int neighbor in grid.GetCardinalNeighbors(cells[i]))
                {
                    GridCell neighborCell = grid.GetCell(neighbor);
                    if (neighborCell != null && !neighborCell.HasFloor) return true;
                }
            }
            return false;
        }

        private bool FootprintTouchesAnyFloor(Vector2Int[] cells)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                foreach (Vector2Int neighbor in grid.GetCardinalNeighbors(cells[i]))
                {
                    GridCell neighborCell = grid.GetCell(neighbor);
                    if (neighborCell != null && neighborCell.HasFloor) return true;
                }
            }
            return false;
        }

        private bool FootprintHasAdjacentBuildable(Vector2Int[] cells, BuildableType type)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                foreach (Vector2Int neighbor in grid.GetCardinalNeighbors(cells[i]))
                {
                    GridCell neighborCell = grid.GetCell(neighbor);
                    if (neighborCell != null && neighborCell.Buildable != null && neighborCell.Buildable.Definition.type == type) return true;
                }
            }
            return false;
        }

        private bool WouldBlockPassengerPaths(Vector2Int[] proposedBlockers)
        {
            HashSet<Vector2Int> blocked = new HashSet<Vector2Int>();
            for (int i = 0; i < proposedBlockers.Length; i++) blocked.Add(proposedBlockers[i]);

            List<Vector2Int> openFloorCells = new List<Vector2Int>();
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    GridCell cell = grid.GetCell(position);
                    if (cell == null || !cell.HasFloor) continue;
                    if (blocked.Contains(position)) continue;
                    if (cell.HasBuildable) continue;
                    openFloorCells.Add(position);
                }
            }

            if (openFloorCells.Count < 2) return true;

            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(openFloorCells[0]);
            visited.Add(openFloorCells[0]);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                foreach (Vector2Int neighbor in grid.GetCardinalNeighbors(current))
                {
                    if (visited.Contains(neighbor) || blocked.Contains(neighbor)) continue;
                    GridCell neighborCell = grid.GetCell(neighbor);
                    if (neighborCell == null || !neighborCell.HasFloor || neighborCell.HasBuildable) continue;
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            return visited.Count != openFloorCells.Count;
        }

        private bool RunwayIsStraight(BuildableDefinition definition)
        {
            return definition.size.x >= definition.size.y * 3 || definition.size.y >= definition.size.x * 3;
        }

        private bool CellHasAllowedFloor(GridCell cell, BuildableDefinition definition)
        {
            if (!cell.HasFloor || definition.allowedFloors == null || definition.allowedFloors.Length == 0) return false;
            for (int i = 0; i < definition.allowedFloors.Length; i++)
            {
                if (cell.Floor.Definition == definition.allowedFloors[i]) return true;
            }
            return false;
        }

        private string DefaultValidMessage(BuildableDefinition definition)
        {
            if (!string.IsNullOrWhiteSpace(definition.validPlacementMessage)) return definition.validPlacementMessage;
            return "Place " + definition.displayName + ".";
        }
    }
}
