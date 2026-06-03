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

            int passengerCount = Random.Range(34, 52);
            bool delayed = airport.satisfaction < 65f;
            int reward = delayed ? 1200 : 2400;

            if (!delayed) airport.handledFlights++;
            airport.passengers += passengerCount;
            airport.money += reward + airport.Count(BuildableType.Coffee) * 160;
            if (delayed) airport.satisfaction -= 6f;

            PushAlert((delayed ? "Delayed" : "Completed") + " flight: " + passengerCount + " passengers, +$" + reward + ".");
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
