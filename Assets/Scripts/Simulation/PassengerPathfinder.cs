// Small grid pathfinder for passenger agents. Passengers only walk on passenger-safe terminal floors.
using System.Collections.Generic;
using UnityEngine;
using SkyHubTycoon.Build;
using SkyHubTycoon.Data;
using SkyHubTycoon.Grid;

namespace SkyHubTycoon.Simulation
{
    public class PassengerPathfinder
    {
        private readonly GridManager grid;

        public PassengerPathfinder(GridManager grid)
        {
            this.grid = grid;
        }

        public bool TryFindPath(Vector3 fromWorld, BuildableInstance target, out List<Vector3> worldPath)
        {
            worldPath = new List<Vector3>();
            if (grid == null || target == null) return false;

            Vector2Int start = grid.WorldToGrid(fromWorld);
            List<Vector2Int> goals = GetPassengerAccessCells(target);
            if (goals.Count == 0) return false;

            HashSet<Vector2Int> goalSet = new HashSet<Vector2Int>(goals);
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                if (goalSet.Contains(current))
                {
                    BuildWorldPath(start, current, cameFrom, worldPath);
                    return true;
                }

                foreach (Vector2Int neighbor in grid.GetCardinalNeighbors(current))
                {
                    if (visited.Contains(neighbor)) continue;
                    if (!IsPassengerWalkable(neighbor, goalSet)) continue;
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }

            return false;
        }

        private void BuildWorldPath(Vector2Int start, Vector2Int end, Dictionary<Vector2Int, Vector2Int> cameFrom, List<Vector3> worldPath)
        {
            List<Vector2Int> cells = new List<Vector2Int>();
            Vector2Int current = end;
            cells.Add(current);

            while (current != start && cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                cells.Add(current);
            }

            cells.Reverse();
            for (int i = 0; i < cells.Count; i++)
            {
                worldPath.Add(grid.GridToWorld(cells[i]) + Vector3.up * 0.18f);
            }
        }

        private List<Vector2Int> GetPassengerAccessCells(BuildableInstance target)
        {
            List<Vector2Int> result = new List<Vector2Int>();
            for (int i = 0; i < target.OccupiedCells.Length; i++)
            {
                foreach (Vector2Int neighbor in grid.GetCardinalNeighbors(target.OccupiedCells[i]))
                {
                    if (IsPassengerWalkable(neighbor, null) && !result.Contains(neighbor)) result.Add(neighbor);
                }
            }
            return result;
        }

        private bool IsPassengerWalkable(Vector2Int position, HashSet<Vector2Int> goalSet)
        {
            GridCell cell = grid.GetCell(position);
            if (cell == null || !cell.HasFloor || !IsPassengerSafeFloor(cell.Floor.Definition)) return false;
            if (goalSet != null && goalSet.Contains(position)) return true;
            return !cell.HasBuildable;
        }

        private bool IsPassengerSafeFloor(FloorDefinition floor)
        {
            if (floor == null || !floor.Allows(AgentPathType.Passenger)) return false;
            if (floor.zoneType == ZoneType.Airfield || floor.zoneType == ZoneType.Staff || floor.zoneType == ZoneType.Baggage) return false;
            return true;
        }
    }
}
