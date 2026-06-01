// Owns the airport tile grid, converts mouse/world positions to grid cells, and tracks floor/buildable occupancy.
using System.Collections.Generic;
using UnityEngine;
using SkyHubTycoon.Build;
using SkyHubTycoon.Data;

namespace SkyHubTycoon.Grid
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid")]
        public int width = 24;
        public int height = 24;
        public float cellSize = 1f;
        public Transform floorParent;
        public Transform buildableParent;

        private GridCell[,] cells;

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            cells = new GridCell[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells[x, y] = new GridCell(new Vector2Int(x, y));
                }
            }
        }

        public bool InBounds(Vector2Int position)
        {
            return position.x >= 0 && position.y >= 0 && position.x < width && position.y < height;
        }

        public GridCell GetCell(Vector2Int position)
        {
            if (!InBounds(position)) return null;
            if (cells == null) Initialize();
            return cells[position.x, position.y];
        }

        public Vector3 GridToWorld(Vector2Int position)
        {
            return new Vector3((position.x + 0.5f) * cellSize, 0f, (position.y + 0.5f) * cellSize);
        }

        public Vector3 FootprintCenter(Vector2Int origin, Vector2Int size)
        {
            return new Vector3((origin.x + size.x * 0.5f) * cellSize, 0f, (origin.y + size.y * 0.5f) * cellSize);
        }

        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            return new Vector2Int(Mathf.FloorToInt(worldPosition.x / cellSize), Mathf.FloorToInt(worldPosition.z / cellSize));
        }

        public List<Vector2Int> GetFootprint(Vector2Int origin, Vector2Int size)
        {
            List<Vector2Int> footprint = new List<Vector2Int>();
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    footprint.Add(new Vector2Int(origin.x + x, origin.y + y));
                }
            }
            return footprint;
        }

        public IEnumerable<Vector2Int> GetCardinalNeighbors(Vector2Int position)
        {
            Vector2Int[] candidates = new Vector2Int[]
            {
                new Vector2Int(position.x + 1, position.y),
                new Vector2Int(position.x - 1, position.y),
                new Vector2Int(position.x, position.y + 1),
                new Vector2Int(position.x, position.y - 1)
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                if (InBounds(candidates[i])) yield return candidates[i];
            }
        }

        public bool HasAnyFloor()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (cells[x, y].HasFloor) return true;
                }
            }
            return false;
        }

        public bool IsFootprintConnectedToExistingFloor(List<Vector2Int> footprint)
        {
            if (!HasAnyFloor()) return true;

            for (int i = 0; i < footprint.Count; i++)
            {
                foreach (Vector2Int neighbor in GetCardinalNeighbors(footprint[i]))
                {
                    GridCell cell = GetCell(neighbor);
                    if (cell != null && cell.HasFloor) return true;
                }
            }
            return false;
        }

        public int CountBuildables(BuildableType type)
        {
            int count = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    BuildableInstance buildable = cells[x, y].Buildable;
                    if (buildable != null && buildable.Origin == new Vector2Int(x, y) && buildable.Definition.type == type)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}
