using UnityEngine;
using SkyHubTycoon.Data;

namespace SkyHubTycoon.Build
{
    public class BuildableInstance : MonoBehaviour
    {
        public BuildableDefinition Definition { get; private set; }
        public Vector2Int Origin { get; private set; }
        public Vector2Int[] OccupiedCells { get; private set; }

        public void Initialize(BuildableDefinition definition, Vector2Int origin, Vector2Int[] occupiedCells)
        {
            Definition = definition;
            Origin = origin;
            OccupiedCells = occupiedCells;
            name = definition.displayName + " " + origin.x + "," + origin.y;
        }
    }
}
