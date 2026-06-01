using UnityEngine;
using SkyHubTycoon.Build;

namespace SkyHubTycoon.Grid
{
    public class GridCell
    {
        public Vector2Int Position { get; private set; }
        public FloorInstance Floor { get; set; }
        public BuildableInstance Buildable { get; set; }

        public bool HasFloor { get { return Floor != null; } }
        public bool HasBuildable { get { return Buildable != null; } }

        public GridCell(Vector2Int position)
        {
            Position = position;
        }
    }
}
