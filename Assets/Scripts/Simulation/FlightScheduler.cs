using UnityEngine;
using SkyHubTycoon.Data;
using SkyHubTycoon.UI;

namespace SkyHubTycoon.Simulation
{
    public class FlightScheduler : MonoBehaviour
    {
        public AirportState airport;
        public AlertManager alertManager;

        public void ScheduleNextFlight()
        {
            if (airport == null) return;
            airport.RecalculateSystems();

            if (airport.SystemProblems.Count > 0)
            {
                PushAlert("Flight cannot operate. " + airport.SystemProblems[0]);
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

        private void PushAlert(string message)
        {
            if (alertManager != null) alertManager.Push(message);
            else Debug.Log(message);
        }
    }
}
