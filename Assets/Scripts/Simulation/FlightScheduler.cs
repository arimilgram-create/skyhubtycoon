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
            airport.money += reward + CalculateConcessionIncome();
            if (delayed) airport.satisfaction -= 6f;

            PushAlert((delayed ? "Delayed" : "Completed") + " flight: " + passengerCount + " passengers, +$" + reward + ".");
            airport.RecalculateSystems();
        }

        private int CalculateConcessionIncome()
        {
            return airport.Count(BuildableType.Coffee) * 160
                + airport.Count(BuildableType.SnackKiosk) * 120
                + airport.Count(BuildableType.Restaurant) * 420
                + airport.Count(BuildableType.GiftShop) * 260
                + airport.Count(BuildableType.DutyFreeShop) * 700
                + airport.Count(BuildableType.Bookstore) * 220
                + airport.Count(BuildableType.VendingMachine) * 75;
        }

        private void PushAlert(string message)
        {
            if (alertManager != null) alertManager.Push(message);
            else Debug.Log(message);
        }
    }
}
