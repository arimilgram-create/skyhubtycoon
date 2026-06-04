using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SkyHubTycoon.CameraControls;

namespace SkyHubTycoon.UI
{
    public class AlertManager : MonoBehaviour
    {
        public Transform alertParent;
        public Text alertPrefab;
        public IsometricCameraController cameraController;
        public int maxAlerts = 8;

        private readonly Queue<Text> activeAlerts = new Queue<Text>();

        public void Push(string message)
        {
            Push(message, Vector3.zero, false);
        }

        public void Push(string message, Vector3 focusPosition)
        {
            Push(message, focusPosition, true);
        }

        private void Push(string message, Vector3 focusPosition, bool hasFocus)
        {
            Debug.Log("SkyHub Alert: " + message);
            if (alertParent == null || alertPrefab == null) return;

            Text alert = Instantiate(alertPrefab, alertParent);
            alert.gameObject.SetActive(true);
            alert.text = hasFocus ? "• " + message + "  (click to view)" : "• " + message;

            if (hasFocus)
            {
                Button button = alert.gameObject.GetComponent<Button>();
                if (button == null) button = alert.gameObject.AddComponent<Button>();
                button.targetGraphic = alert;
                button.onClick.AddListener(delegate { FocusCamera(focusPosition); });
            }

            activeAlerts.Enqueue(alert);

            while (activeAlerts.Count > maxAlerts)
            {
                Text oldAlert = activeAlerts.Dequeue();
                if (oldAlert != null) Destroy(oldAlert.gameObject);
            }
        }

        private void FocusCamera(Vector3 focusPosition)
        {
            if (cameraController != null) cameraController.FocusOnWorldPosition(focusPosition);
        }
    }
}
