using UnityEngine;

namespace SkyHubTycoon.Data
{
    [CreateAssetMenu(menuName = "SkyHub Tycoon/Buildable Definition", fileName = "BuildableDefinition")]
    public class BuildableDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        public BuildableType type;
        public BuildCategory category;

        [Header("Grid")]
        public Vector2Int size = Vector2Int.one;
        public FloorDefinition[] allowedFloors;

        [Header("Economy")]
        public int cost;
        public int maintenanceCost;
        public int capacity;
        public int upgradeLevel;

        [Header("Utilities")]
        public int powerUse;
        public int waterUse;
        public int powerProduction;
        public int waterProduction;

        [Header("Feedback")]
        [TextArea(2, 4)] public string invalidPlacementWarning;
        [TextArea(2, 4)] public string validPlacementMessage;

        [Header("Presentation")]
        public GameObject prefab;
        public Color tint = Color.white;
    }
}
