using UnityEngine;

namespace SkyHubTycoon.Build
{
    public struct PlacementResult
    {
        public bool valid;
        public bool inefficient;
        public string message;
        public Vector2Int[] cells;

        public static PlacementResult Valid(string message, Vector2Int[] cells, bool inefficient)
        {
            PlacementResult result = new PlacementResult();
            result.valid = true;
            result.inefficient = inefficient;
            result.message = message;
            result.cells = cells;
            return result;
        }

        public static PlacementResult Invalid(string message, Vector2Int[] cells)
        {
            PlacementResult result = new PlacementResult();
            result.valid = false;
            result.inefficient = false;
            result.message = message;
            result.cells = cells;
            return result;
        }
    }
}
