// Tracks campaign-style goals from the original airport tycoon prompt without using any WebGL-unsafe APIs.
using System.Collections.Generic;
using UnityEngine;
using SkyHubTycoon.Data;

namespace SkyHubTycoon.Simulation
{
    public class MissionSystem : MonoBehaviour
    {
        public AirportState airport;

        public string BuildMissionReport()
        {
            if (airport == null) return "Missions\nNo airport state connected.";

            List<string> lines = new List<string>();
            lines.Add("Missions");
            Add(lines, airport.HasPassengerRoute() && airport.HasAirfieldRoute(), "Build your first working airport");
            Add(lines, airport.passengers >= 50, "Handle 50 passengers");
            Add(lines, airport.handledFlights >= 10, "Handle 10 flights without delay");
            Add(lines, airport.satisfaction >= 80f, "Reach 80% satisfaction");
            Add(lines, airport.Count(BuildableType.SmallGate) + airport.Count(BuildableType.MediumGate) + airport.Count(BuildableType.LargeGate) + airport.Count(BuildableType.InternationalGate) >= 3, "Build 3 gates");
            Add(lines, airport.HasBaggageRoute(), "Add complete baggage claim");
            Add(lines, airport.level >= 5, "Unlock international flights");
            Add(lines, airport.money >= 1000000, "Earn $1,000,000");
            Add(lines, airport.reputation >= 5f, "Reach 5-star airport rating");
            return string.Join("\n", lines.ToArray());
        }

        private void Add(List<string> lines, bool complete, string label)
        {
            lines.Add((complete ? "✓ " : "□ ") + label);
        }
    }
}
