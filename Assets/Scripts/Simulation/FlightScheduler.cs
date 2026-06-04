using UnityEngine;
using SkyHubTycoon.Data;
using SkyHubTycoon.UI;
using SkyHubTycoon.Grid;

namespace SkyHubTycoon.Simulation
{
    public class FlightScheduler : MonoBehaviour
    {
        public AirportState airport;
        public GridManager grid;
        public PassengerManager passengerManager;
        public AlertManager alertManager;

        public void ScheduleNextFlight()
        {
            if (airport == null) return;
            airport.RecalculateSystems();

            AirportFlowValidator.FlowValidationResult flow = ValidateFlow();
            if (!flow.valid)
            {
                for (int i = 0; i < flow.issues.Count; i++)
                {
                    AirportFlowValidator.FlowIssue issue = flow.issues[i];
                    if (issue.hasFocus) PushAlert(issue.message, issue.focusPosition);
                    else PushAlert(issue.message);
                }
                return;
            }

            int passengerCount = passengerManager != null ? passengerManager.passengersPerFlight : Random.Range(10, 14);
            bool delayed = airport.satisfaction < 65f;

            if (passengerManager != null)
            {
                if (passengerManager.BeginBoardingFlight(passengerCount, delayed)) PushAlert("Boarding started: " + passengerCount + " passengers walking to the gate.");
                return;
            }

            if (delayed) airport.AdjustPassengerSatisfaction(-6f);
            airport.handledFlights++;
            airport.passengers += passengerCount;
            airport.money += 900 + passengerCount * 80;
            PushAlert((delayed ? "Delayed" : "Completed") + " fallback flight: " + passengerCount + " passengers.");
            airport.RecalculateSystems();
        }

        private AirportFlowValidator.FlowValidationResult ValidateFlow()
        {
            AirportFlowValidator validator = new AirportFlowValidator(grid, airport);
            return validator.ValidateBasicDepartingFlight();
        }

        private void PushAlert(string message)
        {
            if (alertManager != null) alertManager.Push(message);
            else Debug.Log(message);
        }

        private void PushAlert(string message, Vector3 focusPosition)
        {
            if (alertManager != null) alertManager.Push(message, focusPosition);
            else Debug.Log(message);
        }
    }
}
