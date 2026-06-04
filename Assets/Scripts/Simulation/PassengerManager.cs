// Spawns simple passenger agents and applies lightweight satisfaction changes for route, queue, seating, and delay problems.
using System.Collections.Generic;
using UnityEngine;
using SkyHubTycoon.Build;
using SkyHubTycoon.Data;
using SkyHubTycoon.Grid;
using SkyHubTycoon.UI;

namespace SkyHubTycoon.Simulation
{
    public class PassengerManager : MonoBehaviour
    {
        public GridManager grid;
        public AirportState airport;
        public AlertManager alertManager;
        public GameObject passengerPrefab;
        public Transform passengerParent;
        public int queueWarningThreshold = 5;
        public int passengersPerFlight = 12;

        private PassengerPathfinder pathfinder;
        private int activePassengers;
        private int boardedPassengers;
        private int targetBoardedPassengers;
        private int checkInLine;
        private int securityLine;
        private bool flightInProgress;
        private bool warnedCheckInLine;
        private bool warnedSecurityLine;
        private bool warnedNoSeats;

        private void Awake()
        {
            if (passengerParent == null)
            {
                GameObject parent = new GameObject("Passengers");
                parent.transform.SetParent(transform);
                passengerParent = parent.transform;
            }
        }

        public bool IsFlightInProgress { get { return flightInProgress; } }

        public bool BeginBoardingFlight(int requestedPassengers, bool delayed)
        {
            if (grid == null || airport == null) return false;
            if (flightInProgress)
            {
                PushAlert("Flight already boarding. Wait for current passengers to board.");
                return false;
            }

            BuildableInstance entrance = First(BuildableType.Entrance);
            BuildableInstance checkIn = First(BuildableType.CheckIn);
            BuildableInstance security = First(BuildableType.Security);
            BuildableInstance waitingSeat = First(BuildableType.Seating);
            BuildableInstance gate = First(BuildableType.SmallGate);

            if (entrance == null || checkIn == null || security == null || gate == null)
            {
                PushAlert("Passengers cannot find a route.");
                airport.AdjustPassengerSatisfaction(-4f);
                return false;
            }

            if (delayed)
            {
                PushAlert("Flight delayed. Passenger satisfaction decreased.");
                airport.AdjustPassengerSatisfaction(-6f);
            }

            if (waitingSeat == null || !SeatNearGate(waitingSeat, gate, 6))
            {
                warnedNoSeats = true;
                PushAlert("No seats near the gate. Passenger satisfaction decreased.");
                airport.AdjustPassengerSatisfaction(-4f);
            }
            else
            {
                warnedNoSeats = false;
            }

            if (pathfinder == null) pathfinder = new PassengerPathfinder(grid);
            flightInProgress = true;
            boardedPassengers = 0;
            activePassengers = 0;
            targetBoardedPassengers = Mathf.Max(1, requestedPassengers);
            checkInLine = 0;
            securityLine = 0;
            warnedCheckInLine = false;
            warnedSecurityLine = false;

            for (int i = 0; i < targetBoardedPassengers; i++)
            {
                SpawnPassenger(entrance, checkIn, security, waitingSeat, gate, i);
            }

            return true;
        }

        public bool TryGetPath(Vector3 fromWorld, BuildableInstance target, List<Vector3> path)
        {
            path.Clear();
            if (pathfinder == null) pathfinder = new PassengerPathfinder(grid);
            List<Vector3> foundPath;
            if (!pathfinder.TryFindPath(fromWorld, target, out foundPath)) return false;
            path.AddRange(foundPath);
            return true;
        }

        public Vector3 GetBuildableCenter(BuildableInstance buildable)
        {
            if (buildable == null || grid == null) return Vector3.zero;
            return grid.FootprintCenter(buildable.Origin, buildable.Definition.size);
        }

        public void EnterCheckInLine()
        {
            checkInLine++;
            if (checkInLine > queueWarningThreshold && !warnedCheckInLine)
            {
                warnedCheckInLine = true;
                PushAlert("Check-in line is too long. Passenger satisfaction decreased.");
                airport.AdjustPassengerSatisfaction(-2f);
            }
        }

        public void LeaveCheckInLine()
        {
            checkInLine = Mathf.Max(0, checkInLine - 1);
        }

        public void EnterSecurityLine()
        {
            securityLine++;
            if (securityLine > queueWarningThreshold && !warnedSecurityLine)
            {
                warnedSecurityLine = true;
                PushAlert("Security line is too long. Passenger satisfaction decreased.");
                airport.AdjustPassengerSatisfaction(-2f);
            }
        }

        public void LeaveSecurityLine()
        {
            securityLine = Mathf.Max(0, securityLine - 1);
        }

        public void PassengerCouldNotFindRoute(PassengerAgent passenger)
        {
            PushAlert("Passenger cannot find a route. Passenger satisfaction decreased.");
            airport.AdjustPassengerSatisfaction(-3f);
            RemovePassenger(passenger, false);
        }

        public void PassengerBoarded(PassengerAgent passenger)
        {
            RemovePassenger(passenger, true);
        }

        private void SpawnPassenger(BuildableInstance entrance, BuildableInstance checkIn, BuildableInstance security, BuildableInstance waitingSeat, BuildableInstance gate, int index)
        {
            GameObject passengerObject = passengerPrefab != null
                ? Instantiate(passengerPrefab, passengerParent)
                : GameObject.CreatePrimitive(PrimitiveType.Capsule);
            passengerObject.name = "Passenger " + (index + 1);
            passengerObject.transform.SetParent(passengerParent, false);
            passengerObject.transform.localScale = new Vector3(0.28f, 0.52f, 0.28f);

            Renderer renderer = passengerObject.GetComponentInChildren<Renderer>();
            if (renderer != null) renderer.material.color = Color.Lerp(new Color(0.2f, 0.55f, 1f), new Color(1f, 0.7f, 0.25f), (index % 5) / 4f);

            PassengerAgent passenger = passengerObject.GetComponent<PassengerAgent>();
            if (passenger == null) passenger = passengerObject.AddComponent<PassengerAgent>();
            activePassengers++;
            passenger.Initialize(this, entrance, checkIn, security, waitingSeat, gate);
        }

        private void RemovePassenger(PassengerAgent passenger, bool boarded)
        {
            if (boarded) boardedPassengers++;
            activePassengers = Mathf.Max(0, activePassengers - 1);
            if (passenger != null) Destroy(passenger.gameObject);

            if (flightInProgress && activePassengers == 0)
            {
                CompleteFlight();
            }
        }

        private void CompleteFlight()
        {
            flightInProgress = false;
            airport.handledFlights++;
            airport.passengers += boardedPassengers;
            int reward = 900 + boardedPassengers * 80;
            airport.money += reward;
            PushAlert("Flight boarded: " + boardedPassengers + " / " + targetBoardedPassengers + " passengers, +$" + reward + ".");
            airport.RecalculateSystems();
        }

        private BuildableInstance First(BuildableType type)
        {
            IReadOnlyList<BuildableInstance> buildables = airport.Buildables;
            for (int i = 0; i < buildables.Count; i++)
            {
                BuildableInstance buildable = buildables[i];
                if (buildable != null && buildable.Definition.type == type) return buildable;
            }
            return null;
        }

        private bool SeatNearGate(BuildableInstance seat, BuildableInstance gate, int range)
        {
            if (seat == null || gate == null) return false;
            int distance = Mathf.Abs(seat.Origin.x - gate.Origin.x) + Mathf.Abs(seat.Origin.y - gate.Origin.y);
            return distance <= range;
        }

        private void PushAlert(string message)
        {
            if (alertManager != null) alertManager.Push(message);
            else Debug.Log(message);
        }
    }
}
