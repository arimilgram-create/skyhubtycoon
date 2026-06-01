using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SkyHubTycoon.Build;
using SkyHubTycoon.CameraControls;
using SkyHubTycoon.Data;
using SkyHubTycoon.Simulation;

namespace SkyHubTycoon.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("References")]
        public AirportState airport;
        public BuildPlacementController placementController;
        public FlightScheduler flightScheduler;
        public AlertManager alertManager;
        public IsometricCameraController cameraController;

        [Header("Runtime UI")]
        public Canvas canvas;
        public Text moneyText;
        public Text satisfactionText;
        public Text reputationText;
        public Text flightText;
        public Text selectedToolText;
        public Text placementHintText;
        public Text systemsText;
        public Text staffText;
        public Text unlocksText;
        public Transform buildMenuParent;
        public Transform brushMenuParent;

        private bool roofsVisible;
        private bool cutaway = true;
        private bool gridVisible = true;

        private void Start()
        {
            if (canvas == null) BuildRuntimeCanvas();
            if (airport != null) airport.Changed += Refresh;
            PopulateBuildMenu();
            Refresh();
        }

        private void OnDestroy()
        {
            if (airport != null) airport.Changed -= Refresh;
        }

        public void SetSelectedTool(string value)
        {
            if (selectedToolText != null) selectedToolText.text = "Tool: " + value;
        }

        public void SetPlacementHint(string value)
        {
            if (placementHintText != null) placementHintText.text = value;
        }

        public void Refresh()
        {
            if (airport == null) return;
            if (moneyText != null) moneyText.text = "Money: $" + airport.money.ToString("N0");
            if (satisfactionText != null) satisfactionText.text = "Satisfaction: " + Mathf.RoundToInt(airport.satisfaction) + "%";
            if (reputationText != null) reputationText.text = "Reputation: ★ " + airport.reputation.ToString("0.0");
            if (flightText != null) flightText.text = "Flights: " + airport.handledFlights + " / 3";
            if (systemsText != null) systemsText.text = BuildSystemsText();
            if (staffText != null) staffText.text = BuildStaffText();
            if (unlocksText != null) unlocksText.text = BuildUnlockText();
        }

        private string BuildSystemsText()
        {
            List<string> lines = new List<string>();
            lines.Add("Systems");
            lines.Add((airport.HasPassengerRoute() ? "✓" : "!") + " Passenger route");
            lines.Add((airport.HasAirfieldRoute() ? "✓" : "!") + " Airfield route");
            lines.Add((airport.HasBaggageRoute() ? "✓" : "!") + " Baggage route");
            lines.Add("Power: " + airport.SumPowerUse() + " / " + airport.SumPowerProduction());
            lines.Add("Water: " + airport.SumWaterUse() + " / " + airport.SumWaterProduction());
            if (airport.SystemProblems.Count > 0)
            {
                lines.Add("");
                for (int i = 0; i < airport.SystemProblems.Count; i++) lines.Add("• " + airport.SystemProblems[i]);
            }
            return string.Join("\n", lines.ToArray());
        }

        private string BuildStaffText()
        {
            return "Staff\nJanitors: " + airport.janitors
                + "\nSecurity: " + airport.securityOfficers
                + "\nCheck-in: " + airport.checkInAgents
                + "\nGate: " + airport.gateAgents
                + "\nBaggage: " + airport.baggageHandlers;
        }

        private string BuildUnlockText()
        {
            return "Unlocks\nLevel " + airport.level + "\n"
                + "1 Tiny: first 3 flights\n"
                + "2 Local: 100 passengers\n"
                + "3 Regional: 70% satisfaction\n"
                + "4 National: 1,000 passengers\n"
                + "5 International: 5-star airport";
        }

        private void PopulateBuildMenu()
        {
            if (placementController == null) return;

            if (brushMenuParent != null && placementController.brushSizes != null)
            {
                for (int i = 0; i < placementController.brushSizes.Length; i++)
                {
                    BrushSize brush = placementController.brushSizes[i];
                    CreateButton(brushMenuParent, brush.label, delegate { placementController.SetBrush(brush.size); });
                }
            }

            if (buildMenuParent != null)
            {
                if (placementController.floorDefinitions != null)
                {
                    for (int i = 0; i < placementController.floorDefinitions.Length; i++)
                    {
                        FloorDefinition floor = placementController.floorDefinitions[i];
                        CreateButton(buildMenuParent, floor.displayName, delegate { placementController.SelectFloor(floor); });
                    }
                }

                if (placementController.buildableDefinitions != null)
                {
                    for (int i = 0; i < placementController.buildableDefinitions.Length; i++)
                    {
                        BuildableDefinition buildable = placementController.buildableDefinitions[i];
                        CreateButton(buildMenuParent, buildable.displayName, delegate { placementController.SelectBuildable(buildable); });
                    }
                }

                CreateButton(buildMenuParent, "Bulldoze", delegate { placementController.ToggleBulldoze(); });
            }
        }

        private void BuildRuntimeCanvas()
        {
            GameObject canvasObject = new GameObject("SkyHub Runtime UI");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            RectTransform top = CreatePanel(canvas.transform, "Top HUD", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -35f), new Vector2(1120f, 58f));
            moneyText = CreateText(top, "Money", font, 18, TextAnchor.MiddleLeft);
            satisfactionText = CreateText(top, "Satisfaction", font, 18, TextAnchor.MiddleLeft);
            reputationText = CreateText(top, "Reputation", font, 18, TextAnchor.MiddleLeft);
            flightText = CreateText(top, "Flights", font, 18, TextAnchor.MiddleLeft);
            AddHorizontalLayout(top.gameObject);

            RectTransform left = CreatePanel(canvas.transform, "Build Menu", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(145f, 0f), new Vector2(280f, 760f));
            brushMenuParent = CreatePanel(left, "Brushes", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(250f, 95f));
            buildMenuParent = CreatePanel(left, "Tools", new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.48f), new Vector2(0f, -20f), new Vector2(250f, 610f));
            AddVerticalLayout(brushMenuParent.gameObject);
            AddVerticalLayout(buildMenuParent.gameObject);

            RectTransform right = CreatePanel(canvas.transform, "Info Panel", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-170f, 0f), new Vector2(330f, 760f));
            systemsText = CreateText(right, "Systems", font, 14, TextAnchor.UpperLeft);
            staffText = CreateText(right, "Staff", font, 14, TextAnchor.UpperLeft);
            unlocksText = CreateText(right, "Unlocks", font, 14, TextAnchor.UpperLeft);
            AddVerticalLayout(right.gameObject);

            RectTransform bottom = CreatePanel(canvas.transform, "Bottom Controls", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 64f), new Vector2(760f, 110f));
            selectedToolText = CreateText(bottom, "Tool", font, 16, TextAnchor.MiddleLeft);
            placementHintText = CreateText(bottom, "Hint", font, 14, TextAnchor.MiddleLeft);
            CreateButton(bottom, "Schedule next flight", delegate { if (flightScheduler != null) flightScheduler.ScheduleNextFlight(); });
            CreateButton(bottom, "Hire staff", delegate { if (airport != null) airport.HireRandomStaff(); });
            CreateButton(bottom, "Rotate 90°", delegate { if (cameraController != null) cameraController.Rotate90(1); });
            CreateButton(bottom, "Roofs", ToggleRoofs);
            CreateButton(bottom, "Cutaway", ToggleCutaway);
            CreateButton(bottom, "Grid", ToggleGrid);
            AddHorizontalLayout(bottom.gameObject);

            GameObject alertsObject = new GameObject("Alerts");
            alertsObject.transform.SetParent(canvas.transform, false);
            RectTransform alertsRect = alertsObject.AddComponent<RectTransform>();
            alertsRect.anchorMin = new Vector2(0.5f, 0f);
            alertsRect.anchorMax = new Vector2(0.5f, 0f);
            alertsRect.anchoredPosition = new Vector2(0f, 180f);
            alertsRect.sizeDelta = new Vector2(620f, 160f);
            AddVerticalLayout(alertsObject);

            if (alertManager == null) alertManager = canvasObject.AddComponent<AlertManager>();
            alertManager.alertParent = alertsRect;
            Text alertTemplate = CreateText(alertsRect, "", font, 13, TextAnchor.UpperLeft);
            alertTemplate.gameObject.SetActive(false);
            alertManager.alertPrefab = alertTemplate;
        }

        private RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.04f, 0.09f, 0.18f, 0.82f);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return rect;
        }

        private Text CreateText(Transform parent, string text, Font font, int size, TextAnchor anchor)
        {
            GameObject textObject = new GameObject(text + " Text");
            textObject.transform.SetParent(parent, false);
            Text uiText = textObject.AddComponent<Text>();
            uiText.text = text;
            uiText.font = font;
            uiText.fontSize = size;
            uiText.color = Color.white;
            uiText.alignment = anchor;
            uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
            uiText.verticalOverflow = VerticalWrapMode.Overflow;
            LayoutElement layout = textObject.AddComponent<LayoutElement>();
            layout.minHeight = 30f;
            layout.flexibleWidth = 1f;
            return uiText;
        }

        private void CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = new GameObject(label + " Button");
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.12f, 0.25f, 0.42f, 0.94f);
            Button button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(action);
            LayoutElement layout = buttonObject.AddComponent<LayoutElement>();
            layout.minHeight = 32f;
            layout.flexibleWidth = 1f;

            Text text = CreateText(buttonObject.transform, label, Resources.GetBuiltinResource<Font>("Arial.ttf"), 13, TextAnchor.MiddleCenter);
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void AddVerticalLayout(GameObject target)
        {
            VerticalLayoutGroup layout = target.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 5f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
        }

        private void AddHorizontalLayout(GameObject target)
        {
            HorizontalLayoutGroup layout = target.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
        }

        private void ToggleRoofs()
        {
            roofsVisible = !roofsVisible;
            Debug.Log("Roofs: " + (roofsVisible ? "On" : "Off"));
        }

        private void ToggleCutaway()
        {
            cutaway = !cutaway;
            Debug.Log("Cutaway: " + (cutaway ? "On" : "Off"));
        }

        private void ToggleGrid()
        {
            gridVisible = !gridVisible;
            Debug.Log("Grid: " + (gridVisible ? "On" : "Off"));
        }
    }
}
