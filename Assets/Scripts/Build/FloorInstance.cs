using UnityEngine;
using SkyHubTycoon.Data;

namespace SkyHubTycoon.Build
{
    public class FloorInstance : MonoBehaviour
    {
        public FloorDefinition Definition { get; private set; }
        public Vector2Int Position { get; private set; }

        public void Initialize(FloorDefinition definition, Vector2Int position)
        {
            Definition = definition;
            Position = position;
            name = definition.displayName + " " + position.x + "," + position.y;
        }
    }
}
