// Checks whether the first playable airport has a connected departing passenger flow and airfield links.
using System.Collections.Generic;
using UnityEngine;
using SkyHubTycoon.Build;
using SkyHubTycoon.Data;
using SkyHubTycoon.Grid;

namespace SkyHubTycoon.Simulation
{
    public class AirportFlowValidator
    {
        public struct FlowIssue
        {
            public string message;
            public bool hasFocus;
            public Vector3 focusPosition;

            public FlowIssue(string message, bool hasFocus, Vector3 focusPosition)
            {
                this.message = message;
                this.hasFocus = hasFocus;
                this.focusPosition = focusPosition;
            }
        }

        public struct FlowValidationResult
        {
            public bool valid;
            public List<FlowIssue> issues;
        }

        private readonly GridManager grid;
        private readonly AirportState airport;

        public AirportFlowValidator(GridManager grid, AirportState airport)
        {
            this.grid = grid;
            this.airport = airport;
        }

        public FlowValidationResult ValidateBasicDepartingFlight()
        {
            FlowValidationResult result = new FlowValidationResult();
            result.issues = new List<FlowIssue>();

            BuildableInstance entrance = First(BuildableType.Entrance);
            BuildableInstance checkIn = First(BuildableType.CheckIn);
            BuildableInstance security = First(BuildableType.Security);
            BuildableInstance waiting = First(BuildableType.Seating);
            BuildableInstance gate = First(BuildableType.SmallGate);
            BuildableInstance runway = First(BuildableType.Runway);
            BuildableInstance taxiway = First(BuildableType.Taxiway);

            if (entrance == null || !TouchesTerminalFloor(entrance))
            {
                AddIssue(result.issues, "No entrance connected to terminal.", entrance);
            }

            if (entrance == null || checkIn == null || !HasPathBetween(entrance, checkIn))
            {
                AddIssue(result.issues, "No path from entrance to check-in.", checkIn != null ? checkIn : entrance);
            }

            if (checkIn == null || security == null || !HasPathBetween(checkIn, security))
            {
                AddIssue(result.issues, "No path from check-in to security.", security != null ? security : checkIn);
            }

            if (security == null || waiting == null || gate == null || !HasPathBetween(security, waiting) || !HasPathBetween(waiting, gate))
            {
                AddIssue(result.issues, "No path from security to gate.", gate != null ? gate : security);
            }

            if (gate == null || !TouchesBuildable(gate, BuildableType.Taxiway))
            {
                AddIssue(result.issues, "Gate is missing taxiway connection.", gate);
            }

            if (runway == null || taxiway == null || !TouchesBuildable(runway, BuildableType.Taxiway))
            {
                AddIssue(result.issues, "Runway is missing taxiway connection.", runway != null ? runway : taxiway);
            }

            result.valid = result.issues.Count == 0;
            return result;
        }

        private void AddIssue(List<FlowIssue> issues, string message, BuildableInstance target)
        {
            if (target != null)
            {
                issues.Add(new FlowIssue(message, true, grid.FootprintCenter(target.Origin, target.Definition.size)));
                return;
            }

            issues.Add(new FlowIssue(message, false, Vector3.zero));
        }

        private BuildableInstance First(BuildableType type)
        {
            if (airport == null) return null;
            IReadOnlyList<BuildableInstance> buildables = airport.Buildables;
            for (int i = 0; i < buildables.Count; i++)
            {
                BuildableInstance buildable = buildables[i];
                if (buildable != null && buildable.Definition.type == type) return buildable;
            }
            return null;
        }

        private bool HasPathBetween(BuildableInstance start, BuildableInstance end)
        {
            if (grid == null || start == null || end == null) return false;

            HashSet<Vector2Int> endCells = new HashSet<Vector2Int>(end.OccupiedCells);
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            for (int i = 0; i < start.OccupiedCells.Length; i++)
            {
                Vector2Int cell = start.OccupiedCells[i];
                if (!grid.InBounds(cell)) continue;
                queue.Enqueue(cell);
                visited.Add(cell);
            }

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                if (endCells.Contains(current)) return true;

                foreach (Vector2Int neighbor in grid.GetCardinalNeighbors(current))
                {
                    if (visited.Contains(neighbor)) continue;
                    if (!IsPassengerWalkable(neighbor, endCells)) continue;
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            return false;
        }

        private bool IsPassengerWalkable(Vector2Int position, HashSet<Vector2Int> targetCells)
        {
            GridCell cell = grid.GetCell(position);
            if (cell == null || !cell.HasFloor) return false;
            if (targetCells.Contains(position)) return true;
            return !cell.HasBuildable;
        }

        private bool TouchesTerminalFloor(BuildableInstance buildable)
        {
            if (buildable == null) return false;
            for (int i = 0; i < buildable.OccupiedCells.Length; i++)
            {
                GridCell cell = grid.GetCell(buildable.OccupiedCells[i]);
                if (cell != null && cell.HasFloor) return true;

                foreach (Vector2Int neighbor in grid.GetCardinalNeighbors(buildable.OccupiedCells[i]))
                {
                    GridCell neighborCell = grid.GetCell(neighbor);
                    if (neighborCell != null && neighborCell.HasFloor) return true;
                }
            }
            return false;
        }

        private bool TouchesBuildable(BuildableInstance buildable, BuildableType type)
        {
            if (buildable == null) return false;
            for (int i = 0; i < buildable.OccupiedCells.Length; i++)
            {
                foreach (Vector2Int neighbor in grid.GetCardinalNeighbors(buildable.OccupiedCells[i]))
                {
                    GridCell cell = grid.GetCell(neighbor);
                    if (cell != null && cell.Buildable != null && cell.Buildable.Definition.type == type) return true;
                }
            }
            return false;
        }
    }
}
