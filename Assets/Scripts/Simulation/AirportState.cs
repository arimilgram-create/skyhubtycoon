// Stores lightweight tycoon simulation state for money, staff, routes, utilities, satisfaction, reputation, and unlock progress.
using System;
using System.Collections.Generic;
using UnityEngine;
using SkyHubTycoon.Build;
using SkyHubTycoon.Data;

namespace SkyHubTycoon.Simulation
{
    public class AirportState : MonoBehaviour
    {
        [Header("Economy")]
        public int money = 85000;
        public float satisfaction = 82f;
        public float reputation = 1.3f;
        public int level = 1;
        public int handledFlights;
        public int passengers;

        [Header("Staff")]
        public int janitors = 1;
        public int securityOfficers;
        public int checkInAgents;
        public int gateAgents;
        public int baggageHandlers;

        private readonly List<BuildableInstance> buildables = new List<BuildableInstance>();
        private readonly List<string> systemProblems = new List<string>();

        public event Action Changed;
        public IReadOnlyList<BuildableInstance> Buildables { get { return buildables; } }
        public IReadOnlyList<string> SystemProblems { get { return systemProblems; } }

        public void RegisterBuildable(BuildableInstance buildable)
        {
            if (!buildables.Contains(buildable)) buildables.Add(buildable);
            AddAutoStaffFor(buildable.Definition.type);
            RecalculateSystems();
        }

        public void UnregisterBuildable(BuildableInstance buildable)
        {
            buildables.Remove(buildable);
            RecalculateSystems();
        }

        public bool HasBuildable(BuildableType type)
        {
            for (int i = 0; i < buildables.Count; i++)
            {
                if (buildables[i] != null && buildables[i].Definition.type == type) return true;
            }
            return false;
        }

        public bool HasBuildableWithin(BuildableType type, Vector2Int origin, int range)
        {
            for (int i = 0; i < buildables.Count; i++)
            {
                BuildableInstance buildable = buildables[i];
                if (buildable == null || buildable.Definition.type != type) continue;
                int distance = Mathf.Abs(origin.x - buildable.Origin.x) + Mathf.Abs(origin.y - buildable.Origin.y);
                if (distance <= range) return true;
            }
            return false;
        }

        public bool HasPassengerRoute()
        {
            return HasBuildable(BuildableType.Entrance)
                && HasBuildable(BuildableType.CheckIn)
                && HasBuildable(BuildableType.Security)
                && HasBuildable(BuildableType.Seating)
                && HasBuildable(BuildableType.SmallGate);
        }

        public bool HasAirfieldRoute()
        {
            return HasBuildable(BuildableType.Runway)
                && HasBuildable(BuildableType.Taxiway)
                && HasBuildable(BuildableType.SmallGate);
        }

        public bool HasBaggageRoute()
        {
            return HasBuildable(BuildableType.BagDrop)
                && HasBuildable(BuildableType.Conveyor)
                && HasBuildable(BuildableType.Carousel);
        }

        public int SumPowerUse()
        {
            int value = 0;
            for (int i = 0; i < buildables.Count; i++) value += buildables[i].Definition.powerUse;
            return value;
        }

        public int SumPowerProduction()
        {
            int value = 0;
            for (int i = 0; i < buildables.Count; i++) value += buildables[i].Definition.powerProduction;
            return value;
        }

        public int SumWaterUse()
        {
            int value = 0;
            for (int i = 0; i < buildables.Count; i++) value += buildables[i].Definition.waterUse;
            return value;
        }

        public int SumWaterProduction()
        {
            int value = 0;
            for (int i = 0; i < buildables.Count; i++) value += buildables[i].Definition.waterProduction;
            return value;
        }

        public void RecalculateSystems()
        {
            systemProblems.Clear();

            bool passengerRoute = HasPassengerRoute();
            bool airfieldRoute = HasAirfieldRoute();
            int powerUse = SumPowerUse();
            int powerProduction = SumPowerProduction();
            int waterUse = SumWaterUse();
            int waterProduction = SumWaterProduction();

            if (!passengerRoute) systemProblems.Add("No complete passenger route from entrance to check-in, security, waiting, gate, and exit.");
            if (!airfieldRoute) systemProblems.Add("Gate, runway, and taxiway must all exist before flights operate.");
            if (powerUse > powerProduction) systemProblems.Add("Power grid overloaded. Add generators or power rooms.");
            if (waterUse > waterProduction) systemProblems.Add("Water demand exceeds plumbing hub capacity.");

            satisfaction = Mathf.Clamp(82f + Count(BuildableType.Seating) * 2f + Count(BuildableType.Coffee) * 3f + Count(BuildableType.Bathroom) * 3f - (powerUse > powerProduction ? 12f : 0f) - (waterUse > waterProduction ? 8f : 0f) - (!passengerRoute ? 10f : 0f), 28f, 98f);
            reputation = Mathf.Min(5f, 1.3f + handledFlights * 0.08f + satisfaction / 120f);

            if (passengers >= 100 && level < 2) level = 2;
            if (satisfaction >= 70f && level < 3) level = 3;
            if (passengers >= 1000 && level < 4) level = 4;
            if (reputation >= 4.5f && level < 5) level = 5;

            RaiseChanged();
        }

        public int Count(BuildableType type)
        {
            int count = 0;
            for (int i = 0; i < buildables.Count; i++)
            {
                if (buildables[i] != null && buildables[i].Definition.type == type) count++;
            }
            return count;
        }

        public void HireRandomStaff()
        {
            if (money < 700) return;
            money -= 700;
            int roll = UnityEngine.Random.Range(0, 5);
            if (roll == 0) janitors++;
            if (roll == 1) securityOfficers++;
            if (roll == 2) checkInAgents++;
            if (roll == 3) gateAgents++;
            if (roll == 4) baggageHandlers++;
            RaiseChanged();
        }

        public void RaiseChanged()
        {
            if (Changed != null) Changed.Invoke();
        }

        private void AddAutoStaffFor(BuildableType type)
        {
            if (type == BuildableType.Security) securityOfficers++;
            if (type == BuildableType.CheckIn) checkInAgents++;
            if (type == BuildableType.SmallGate) gateAgents++;
        }
    }
}
