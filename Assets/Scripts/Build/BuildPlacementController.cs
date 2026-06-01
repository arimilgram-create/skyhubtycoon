// Handles browser-friendly mouse placement, floor brushes, previews, object instantiation, and bulldozing at runtime.
using System.Collections.Generic;
using UnityEngine;
using SkyHubTycoon.Data;
using SkyHubTycoon.Grid;
using SkyHubTycoon.Simulation;
using SkyHubTycoon.UI;

namespace SkyHubTycoon.Build
{
    public class BuildPlacementController : MonoBehaviour
    {
        [Header("References")]
        public Camera mainCamera;
        public GridManager grid;
        public AirportState airport;
        public BuildPreview preview;
        public AlertManager alertManager;
        public UIManager uiManager;
        public LayerMask groundMask = ~0;

        [Header("Catalog")]
        public FloorDefinition[] floorDefinitions;
        public BuildableDefinition[] buildableDefinitions;
        public BrushSize[] brushSizes;

        private PlacementValidator validator;
        private FloorDefinition selectedFloor;
        private BuildableDefinition selectedBuildable;
        private Vector2Int selectedBrush = new Vector2Int(2, 2);
        private bool bulldozeMode;
        private PlacementResult lastResult;

        public FloorDefinition SelectedFloor { get { return selectedFloor; } }
        public BuildableDefinition SelectedBuildable { get { return selectedBuildable; } }
        public Vector2Int SelectedBrush { get { return selectedBrush; } }
        public bool BulldozeMode { get { return bulldozeMode; } }

        private void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            validator = new PlacementValidator(grid, airport);
            if (selectedFloor == null && floorDefinitions != null && floorDefinitions.Length > 0) SelectFloor(floorDefinitions[0]);
        }

        private void Update()
        {
            if (grid == null || airport == null || mainCamera == null) return;

            // Start and pause menus intentionally block building input for WebGL browsers.
            if (uiManager != null && uiManager.IsInputBlocked)
            {
                if (preview != null) preview.Clear();
                return;
            }

            Vector2Int gridPosition;
            if (!TryGetMouseGridPosition(out gridPosition))
            {
                if (preview != null) preview.Clear();
                return;
            }

            if (bulldozeMode)
            {
                lastResult = PlacementResult.Valid("Click to bulldoze object or floor.", new Vector2Int[] { gridPosition }, false);
            }
            else if (selectedFloor != null)
            {
                lastResult = validator.ValidateFloor(selectedFloor, gridPosition, selectedBrush);
            }
            else if (selectedBuildable != null)
            {
                lastResult = validator.ValidateBuildable(selectedBuildable, gridPosition);
            }

            if (preview != null) preview.Show(lastResult);
            if (uiManager != null) uiManager.SetPlacementHint(lastResult.message);

            if (Input.GetMouseButtonDown(0))
            {
                if (bulldozeMode) BulldozeAt(gridPosition);
                else if (lastResult.valid && selectedFloor != null) PaintFloor(selectedFloor, gridPosition, selectedBrush, lastResult);
                else if (lastResult.valid && selectedBuildable != null) PlaceBuildable(selectedBuildable, gridPosition, lastResult);
                else PushAlert(lastResult.message);
            }
        }

        public void SelectFloor(FloorDefinition floor)
        {
            selectedFloor = floor;
            selectedBuildable = null;
            bulldozeMode = false;
            if (uiManager != null) uiManager.SetSelectedTool(floor != null ? floor.displayName : "No floor");
        }

        public void SelectBuildable(BuildableDefinition buildable)
        {
            selectedBuildable = buildable;
            selectedFloor = null;
            bulldozeMode = false;
            if (uiManager != null) uiManager.SetSelectedTool(buildable != null ? buildable.displayName : "No buildable");
        }

        public void SetBrush(Vector2Int brush)
        {
            selectedBrush = brush;
        }

        public void ToggleBulldoze()
        {
            bulldozeMode = !bulldozeMode;
            selectedBuildable = null;
            selectedFloor = null;
            if (uiManager != null) uiManager.SetSelectedTool(bulldozeMode ? "Bulldoze" : "No tool selected");
        }

        private void PaintFloor(FloorDefinition floor, Vector2Int origin, Vector2Int brushSize, PlacementResult result)
        {
            List<Vector2Int> footprint = grid.GetFootprint(origin, brushSize);
            for (int i = 0; i < footprint.Count; i++)
            {
                GridCell cell = grid.GetCell(footprint[i]);
                if (cell.Floor != null) Destroy(cell.Floor.gameObject);

                GameObject floorObject = floor.prefab != null
                    ? Instantiate(floor.prefab, grid.GridToWorld(footprint[i]), Quaternion.identity, grid.floorParent)
                    : GameObject.CreatePrimitive(PrimitiveType.Cube);

                floorObject.transform.position = grid.GridToWorld(footprint[i]) + Vector3.down * 0.025f;
                floorObject.transform.localScale = new Vector3(grid.cellSize * 0.96f, 0.05f, grid.cellSize * 0.96f);

                Renderer renderer = floorObject.GetComponentInChildren<Renderer>();
                if (renderer != null && floor.material != null) renderer.sharedMaterial = floor.material;

                FloorInstance instance = floorObject.GetComponent<FloorInstance>();
                if (instance == null) instance = floorObject.AddComponent<FloorInstance>();
                instance.Initialize(floor, footprint[i]);
                cell.Floor = instance;
            }

            airport.money -= floor.cost * footprint.Count;
            airport.RecalculateSystems();
        }

        private void PlaceBuildable(BuildableDefinition definition, Vector2Int origin, PlacementResult result)
        {
            Vector3 center = grid.FootprintCenter(origin, definition.size);
            GameObject buildableObject = definition.prefab != null
                ? Instantiate(definition.prefab, center, Quaternion.identity, grid.buildableParent)
                : GameObject.CreatePrimitive(PrimitiveType.Cube);

            buildableObject.transform.position = center + Vector3.up * 0.2f;
            buildableObject.transform.localScale = new Vector3(definition.size.x * grid.cellSize * 0.92f, 0.4f, definition.size.y * grid.cellSize * 0.92f);

            Renderer renderer = buildableObject.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                Material material;
                if (renderer.sharedMaterial != null)
                {
                    material = new Material(renderer.sharedMaterial);
                }
                else
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                    if (shader == null) shader = Shader.Find("Standard");
                    material = new Material(shader);
                }
                material.color = definition.tint;
                renderer.sharedMaterial = material;
            }

            BuildableInstance instance = buildableObject.GetComponent<BuildableInstance>();
            if (instance == null) instance = buildableObject.AddComponent<BuildableInstance>();
            instance.Initialize(definition, origin, result.cells);

            for (int i = 0; i < result.cells.Length; i++)
            {
                GridCell cell = grid.GetCell(result.cells[i]);
                cell.Buildable = instance;
            }

            airport.money -= definition.cost;
            airport.RegisterBuildable(instance);
        }

        private void BulldozeAt(Vector2Int position)
        {
            GridCell cell = grid.GetCell(position);
            if (cell == null) return;

            if (cell.Buildable != null)
            {
                BuildableInstance buildable = cell.Buildable;
                for (int i = 0; i < buildable.OccupiedCells.Length; i++)
                {
                    GridCell occupied = grid.GetCell(buildable.OccupiedCells[i]);
                    if (occupied != null && occupied.Buildable == buildable) occupied.Buildable = null;
                }
                airport.money += Mathf.RoundToInt(buildable.Definition.cost * 0.35f);
                airport.UnregisterBuildable(buildable);
                Destroy(buildable.gameObject);
                PushAlert("Bulldozed " + buildable.Definition.displayName + ".");
                return;
            }

            if (cell.Floor != null)
            {
                Destroy(cell.Floor.gameObject);
                cell.Floor = null;
                airport.money += 10;
                airport.RecalculateSystems();
            }
        }

        private bool TryGetMouseGridPosition(out Vector2Int gridPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f, groundMask))
            {
                gridPosition = grid.WorldToGrid(hit.point);
                return grid.InBounds(gridPosition);
            }

            gridPosition = Vector2Int.zero;
            return false;
        }

        private void PushAlert(string message)
        {
            if (alertManager != null) alertManager.Push(message);
            else Debug.LogWarning(message);
        }
    }
}
