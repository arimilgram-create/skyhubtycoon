using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SkyHubTycoon.UI
{
    public class AlertManager : MonoBehaviour
    {
        public Transform alertParent;
        public Text alertPrefab;
        public int maxAlerts = 8;

        private readonly Queue<Text> activeAlerts = new Queue<Text>();

        public void Push(string message)
        {
            Debug.Log("SkyHub Alert: " + message);
            if (alertParent == null || alertPrefab == null) return;

            Text alert = Instantiate(alertPrefab, alertParent);
            alert.gameObject.SetActive(true);
            alert.text = "• " + message;
            activeAlerts.Enqueue(alert);

            while (activeAlerts.Count > maxAlerts)
            {
                Text oldAlert = activeAlerts.Dequeue();
                if (oldAlert != null) Destroy(oldAlert.gameObject);
            }
        }
    }
}
