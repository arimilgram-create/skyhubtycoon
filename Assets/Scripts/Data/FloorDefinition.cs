using UnityEngine;

namespace SkyHubTycoon.Data
{
    [CreateAssetMenu(menuName = "SkyHub Tycoon/Floor Definition", fileName = "FloorDefinition")]
    public class FloorDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        public ZoneType zoneType;

        [Header("Presentation")]
        public Color color = Color.white;
        public Material material;
        public GameObject prefab;

        [Header("Economy")]
        public int cost = 45;

        [Header("Path Access")]
        public bool passengerWalkable;
        public bool staffWalkable;
        public bool baggageWalkable;
        public bool vehicleWalkable;

        public bool Allows(AgentPathType pathType)
        {
            switch (pathType)
            {
                case AgentPathType.Passenger: return passengerWalkable;
                case AgentPathType.Staff: return staffWalkable;
                case AgentPathType.Baggage: return baggageWalkable;
                case AgentPathType.Vehicle: return vehicleWalkable;
                default: return false;
            }
        }
    }
}
