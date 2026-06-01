#if UNITY_EDITOR
// Editor-only bootstrap that creates the upload-ready MainScene, starter assets, build settings, and WebGL configuration.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SkyHubTycoon.Build;
using SkyHubTycoon.CameraControls;
using SkyHubTycoon.Data;
using SkyHubTycoon.Grid;
using SkyHubTycoon.Simulation;
using SkyHubTycoon.UI;

namespace SkyHubTycoon.EditorTools
{
    public static class SkyHubProjectBootstrap
    {
        private const string DataRoot = "Assets/Data";
        private const string FloorRoot = "Assets/Floors";
        private const string MaterialRoot = "Assets/Materials";
        private const string PrefabRoot = "Assets/Prefabs";
        private const string SceneRoot = "Assets/Scenes";
        // Final generated scene path: Assets/Scenes/MainScene.unity
        private const string ScenePath = SceneRoot + "/MainScene.unity";

        [InitializeOnLoadMethod]
        private static void AutoCreateStarterProject()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (File.Exists(ScenePath)) return;
            EditorApplication.delayCall += delegate
            {
                if (!File.Exists(ScenePath)) CreateFullStarterProject();
            };
        }

        [MenuItem("Tools/SkyHub Tycoon/Create Full Starter Project")]
        public static void CreateFullStarterProject()
        {
            EnsureFolders();

            Material groundMaterial = CreateMaterial("Ground_UnlockedLand", new Color(0.36f, 0.62f, 0.42f));
            Material validPreview = CreateTransparentMaterial("Preview_Valid_Green", new Color(0.2f, 1f, 0.45f, 0.42f));
            Material invalidPreview = CreateTransparentMaterial("Preview_Invalid_Red", new Color(1f, 0.15f, 0.28f, 0.42f));
            Material warningPreview = CreateTransparentMaterial("Preview_Warning_Yellow", new Color(1f, 0.78f, 0.1f, 0.45f));

            GameObject floorPrefab = CreateFloorPrefab();
            GameObject previewPrefab = CreatePreviewPrefab(validPreview);

            Dictionary<string, FloorDefinition> floors = CreateFloorDefinitions(floorPrefab);
            BuildableDefinition[] buildables = CreateBuildableDefinitions(floors);
            FloorDefinition[] floorArray = new FloorDefinition[]
            {
                floors["public"], floors["secure"], floors["waiting"], floors["gate"], floors["baggage"], floors["shop"], floors["bathroom"], floors["staff"], floors["vip"], floors["customs"], floors["airfield"]
            };

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateGround(groundMaterial);
            CreateCamera();
            CreateLight();
            CreateGameSystems(floorArray, buildables, previewPrefab, validPreview, invalidPreview, warningPreview);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ConfigureWebGLBuildSettings();
            Debug.Log("SkyHub Tycoon WebGL-ready starter project generated at " + ScenePath + ". Open this scene and press Play, then build WebGL.");
        }


        private static void ConfigureWebGLBuildSettings()
        {
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[] { new EditorBuildSettingsScene(ScenePath, true) };
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.memorySize = 256;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = true;
        }

        private static void EnsureFolders()
        {
            CreateFolder("Assets", "Scenes");
            CreateFolder("Assets", "Scripts");
            CreateFolder("Assets", "Prefabs");
            CreateFolder("Assets", "Materials");
            CreateFolder("Assets", "Audio");
            CreateFolder("Assets", "UI");
            CreateFolder("Assets", "Data");
            CreateFolder("Assets", "Floors");
        }

        private static void CreateFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }

        private static Material CreateMaterial(string name, Color color)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader);
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material CreateTransparentMaterial(string name, Color color)
        {
            Material material = CreateMaterial(name, color);
            material.color = color;
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = 3000;
            return material;
        }

        private static GameObject CreateFloorPrefab()
        {
            string path = PrefabRoot + "/FloorTile.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name = "FloorTile";
            tile.transform.localScale = new Vector3(0.96f, 0.05f, 0.96f);
            tile.AddComponent<FloorInstance>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tile, path);
            Object.DestroyImmediate(tile);
            return prefab;
        }

        private static GameObject CreatePreviewPrefab(Material material)
        {
            string path = PrefabRoot + "/PlacementPreviewTile.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name = "PlacementPreviewTile";
            Collider collider = tile.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
            tile.transform.localScale = new Vector3(0.95f, 0.03f, 0.95f);
            Renderer renderer = tile.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tile, path);
            Object.DestroyImmediate(tile);
            return prefab;
        }

        private static Dictionary<string, FloorDefinition> CreateFloorDefinitions(GameObject floorPrefab)
        {
            Dictionary<string, FloorDefinition> result = new Dictionary<string, FloorDefinition>();
            AddFloor(result, "public", "Public floor", ZoneType.Entrance, new Color(0.56f, 0.78f, 1f), 45, true, true, false, false, floorPrefab);
            AddFloor(result, "secure", "Secure floor", ZoneType.Security, new Color(0.73f, 0.65f, 1f), 70, true, true, false, false, floorPrefab);
            AddFloor(result, "waiting", "Waiting carpet", ZoneType.Waiting, new Color(0.52f, 0.84f, 0.76f), 65, true, true, false, false, floorPrefab);
            AddFloor(result, "gate", "Gate floor", ZoneType.Gate, new Color(0.44f, 0.68f, 0.97f), 75, true, true, false, false, floorPrefab);
            AddFloor(result, "baggage", "Baggage floor", ZoneType.Baggage, new Color(0.95f, 0.74f, 0.42f), 60, false, true, true, false, floorPrefab);
            AddFloor(result, "shop", "Shop floor", ZoneType.Shop, new Color(1f, 0.82f, 0.43f), 80, true, true, false, false, floorPrefab);
            AddFloor(result, "bathroom", "Bathroom floor", ZoneType.Bathroom, new Color(0.62f, 0.91f, 0.93f), 55, true, true, false, false, floorPrefab);
            AddFloor(result, "staff", "Staff floor", ZoneType.Staff, new Color(0.72f, 0.77f, 0.84f), 45, false, true, false, false, floorPrefab);
            AddFloor(result, "vip", "VIP floor", ZoneType.VIP, new Color(0.9f, 0.76f, 0.43f), 180, true, true, false, false, floorPrefab);
            AddFloor(result, "customs", "Customs floor", ZoneType.Customs, new Color(0.84f, 0.6f, 0.85f), 130, true, true, false, false, floorPrefab);
            AddFloor(result, "airfield", "Airfield pavement", ZoneType.Airfield, new Color(0.37f, 0.45f, 0.52f), 35, false, false, false, true, floorPrefab);
            return result;
        }

        private static void AddFloor(Dictionary<string, FloorDefinition> target, string id, string label, ZoneType zone, Color color, int cost, bool passenger, bool staff, bool baggage, bool vehicle, GameObject prefab)
        {
            string path = FloorRoot + "/" + id + ".asset";
            FloorDefinition floor = AssetDatabase.LoadAssetAtPath<FloorDefinition>(path);
            if (floor == null)
            {
                floor = ScriptableObject.CreateInstance<FloorDefinition>();
                AssetDatabase.CreateAsset(floor, path);
            }
            floor.id = id;
            floor.displayName = label;
            floor.zoneType = zone;
            floor.color = color;
            floor.cost = cost;
            floor.passengerWalkable = passenger;
            floor.staffWalkable = staff;
            floor.baggageWalkable = baggage;
            floor.vehicleWalkable = vehicle;
            floor.prefab = prefab;
            floor.material = CreateMaterial("Floor_" + id, color);
            EditorUtility.SetDirty(floor);
            target[id] = floor;
        }

        private static BuildableDefinition[] CreateBuildableDefinitions(Dictionary<string, FloorDefinition> floors)
        {
            List<BuildableDefinition> buildables = new List<BuildableDefinition>();
            buildables.Add(AddBuildable("entrance", "Entrance door", BuildableType.Entrance, BuildCategory.WallsAndDoors, new Vector2Int(2, 1), 900, new[] { floors["public"] }, "Must connect outside pavement to indoor public terminal floor.", new Color(0.4f, 0.9f, 1f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("checkin", "Check-in desk", BuildableType.CheckIn, BuildCategory.PassengerProcessing, new Vector2Int(2, 1), 500, new[] { floors["public"] }, "Must be indoors in the Check-In Zone with two clear queue tiles and a path to entrance.", new Color(0.9f, 0.72f, 0.34f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("kiosk", "Self check-in kiosk", BuildableType.Kiosk, BuildCategory.PassengerProcessing, Vector2Int.one, 350, new[] { floors["public"] }, "Must be near an entrance and on passenger-accessible floor.", new Color(0.34f, 0.9f, 0.95f), 1, 0, 0, 0));
            buildables.Add(AddBuildable("security", "Security checkpoint", BuildableType.Security, BuildCategory.PassengerProcessing, new Vector2Int(2, 3), 1500, new[] { floors["secure"] }, "Requires check-in first, security floor, queue space, staff access, and a route toward gates.", new Color(0.65f, 0.45f, 1f), 4, 0, 0, 0));
            buildables.Add(AddBuildable("seating", "Seating row", BuildableType.Seating, BuildCategory.Comfort, new Vector2Int(2, 1), 180, new[] { floors["waiting"], floors["gate"] }, "Place after security on waiting or gate floors without blocking main paths.", new Color(0.25f, 0.7f, 1f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("smallGate", "Small boarding gate", BuildableType.SmallGate, BuildCategory.GatesAndFlights, new Vector2Int(3, 2), 5000, new[] { floors["gate"] }, "Needs secure path, nearby seating, runway, taxiway, and an outside aircraft stand.", new Color(0.72f, 0.92f, 1f), 3, 0, 0, 0));
            buildables.Add(AddBuildable("runway", "Small runway", BuildableType.Runway, BuildCategory.Airfield, new Vector2Int(20, 3), 20000, new[] { floors["airfield"] }, "Runway must be straight, outdoors, at least 3×20, and clear of buildings.", new Color(0.1f, 0.13f, 0.18f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("taxiway", "Taxiway", BuildableType.Taxiway, BuildCategory.Airfield, new Vector2Int(2, 2), 600, new[] { floors["airfield"] }, "Taxiways must be outdoors on airfield pavement and connect runway to aircraft stands.", new Color(0.18f, 0.22f, 0.3f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("bagDrop", "Bag drop", BuildableType.BagDrop, BuildCategory.Baggage, Vector2Int.one, 800, new[] { floors["public"], floors["baggage"] }, "Must sit next to check-in and connect to baggage conveyor.", new Color(0.95f, 0.55f, 0.2f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("conveyor", "Conveyor belt", BuildableType.Conveyor, BuildCategory.Baggage, Vector2Int.one, 120, new[] { floors["baggage"] }, "Must be on baggage floor and connect edge-to-edge through wall ports.", new Color(0.2f, 0.2f, 0.22f), 1, 0, 0, 0));
            buildables.Add(AddBuildable("carousel", "Baggage carousel", BuildableType.Carousel, BuildCategory.Baggage, new Vector2Int(3, 2), 4000, new[] { floors["baggage"] }, "Requires gate, connected conveyor, and accessible baggage claim space.", new Color(0.75f, 0.35f, 0.15f), 2, 0, 0, 0));
            buildables.Add(AddBuildable("bathroom", "Bathroom suite", BuildableType.Bathroom, BuildCategory.Comfort, new Vector2Int(3, 2), 1200, new[] { floors["bathroom"] }, "Needs bathroom floor, passenger access, plumbing, and cannot open into food prep.", new Color(0.7f, 0.95f, 1f), 0, 3, 0, 0));
            buildables.Add(AddBuildable("coffee", "Coffee stand", BuildableType.Coffee, BuildCategory.Shops, new Vector2Int(2, 1), 2000, new[] { floors["shop"] }, "Must face a passenger path on shop floor with clear counter space.", new Color(0.78f, 0.45f, 0.2f), 1, 1, 0, 0));
            buildables.Add(AddBuildable("staffRoom", "Staff room", BuildableType.StaffRoom, BuildCategory.Staff, new Vector2Int(3, 2), 1800, new[] { floors["staff"] }, "Must be staff-only with lockers, break furniture, and staff path access.", new Color(0.72f, 0.77f, 0.84f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("generator", "Generator", BuildableType.Generator, BuildCategory.Utilities, new Vector2Int(2, 2), 2500, new[] { floors["staff"], floors["airfield"] }, "Place in utility/staff space or outdoors on airfield pavement.", new Color(1f, 0.9f, 0.2f), 0, 0, 18, 0));
            buildables.Add(AddBuildable("waterHub", "Plumbing hub", BuildableType.WaterHub, BuildCategory.Utilities, new Vector2Int(2, 2), 1900, new[] { floors["staff"] }, "Place in staff-only utility rooms to supply bathrooms and restaurants.", new Color(0.25f, 0.7f, 1f), 0, 0, 0, 12));
            buildables.Add(AddBuildable("passport", "Passport control", BuildableType.PassportControl, BuildCategory.PassengerProcessing, new Vector2Int(2, 2), 4500, new[] { floors["customs"] }, "International processing requires customs floor and airport level 5.", new Color(0.8f, 0.45f, 0.9f), 3, 0, 0, 0));
            return buildables.ToArray();
        }

        private static BuildableDefinition AddBuildable(string id, string label, BuildableType type, BuildCategory category, Vector2Int size, int cost, FloorDefinition[] allowedFloors, string warning, Color tint, int powerUse, int waterUse, int powerProduction, int waterProduction)
        {
            string path = DataRoot + "/" + id + ".asset";
            BuildableDefinition buildable = AssetDatabase.LoadAssetAtPath<BuildableDefinition>(path);
            if (buildable == null)
            {
                buildable = ScriptableObject.CreateInstance<BuildableDefinition>();
                AssetDatabase.CreateAsset(buildable, path);
            }

            buildable.id = id;
            buildable.displayName = label;
            buildable.type = type;
            buildable.category = category;
            buildable.size = size;
            buildable.cost = cost;
            buildable.allowedFloors = allowedFloors;
            buildable.invalidPlacementWarning = warning;
            buildable.validPlacementMessage = "Place " + label + ".";
            buildable.tint = tint;
            buildable.powerUse = powerUse;
            buildable.waterUse = waterUse;
            buildable.powerProduction = powerProduction;
            buildable.waterProduction = waterProduction;
            buildable.prefab = CreateBuildablePrefab(id, label, tint);
            EditorUtility.SetDirty(buildable);
            return buildable;
        }

        private static GameObject CreateBuildablePrefab(string id, string label, Color tint)
        {
            string path = PrefabRoot + "/" + id + ".prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = label;
            Renderer renderer = obj.GetComponent<Renderer>();
            renderer.sharedMaterial = CreateMaterial("Buildable_" + id, tint);
            obj.AddComponent<BuildableInstance>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static void CreateGround(Material groundMaterial)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Unlocked Build Land (24 x 24)";
            ground.transform.position = new Vector3(12f, -0.08f, 12f);
            ground.transform.localScale = new Vector3(24f, 0.08f, 24f);
            ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 15f;
            cameraObject.transform.position = new Vector3(12f, 18f, -12f);
            cameraObject.transform.rotation = Quaternion.Euler(55f, 45f, 0f);
            cameraObject.AddComponent<IsometricCameraController>();
        }

        private static void CreateLight()
        {
            GameObject lightObject = new GameObject("Sun Soft Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
        }

        private static void CreateGameSystems(FloorDefinition[] floors, BuildableDefinition[] buildables, GameObject previewPrefab, Material validPreview, Material invalidPreview, Material warningPreview)
        {
            GameObject root = new GameObject("SkyHub Game Systems");
            Transform floorParent = new GameObject("Placed Floors").transform;
            Transform buildableParent = new GameObject("Placed Buildables").transform;
            Transform previewParent = new GameObject("Placement Preview").transform;
            floorParent.SetParent(root.transform);
            buildableParent.SetParent(root.transform);
            previewParent.SetParent(root.transform);

            GridManager grid = root.AddComponent<GridManager>();
            grid.floorParent = floorParent;
            grid.buildableParent = buildableParent;

            AirportState airport = root.AddComponent<AirportState>();
            BuildPreview preview = previewParent.gameObject.AddComponent<BuildPreview>();
            preview.grid = grid;
            preview.previewTilePrefab = previewPrefab;
            preview.validMaterial = validPreview;
            preview.invalidMaterial = invalidPreview;
            preview.warningMaterial = warningPreview;

            AlertManager alerts = root.AddComponent<AlertManager>();
            FlightScheduler scheduler = root.AddComponent<FlightScheduler>();
            UIManager ui = root.AddComponent<UIManager>();
            BuildPlacementController placement = root.AddComponent<BuildPlacementController>();

            Camera camera = Camera.main;
            IsometricCameraController cameraController = camera != null ? camera.GetComponent<IsometricCameraController>() : null;

            placement.mainCamera = camera;
            placement.grid = grid;
            placement.airport = airport;
            placement.preview = preview;
            placement.alertManager = alerts;
            placement.uiManager = ui;
            placement.floorDefinitions = floors;
            placement.buildableDefinitions = buildables;
            placement.brushSizes = new BrushSize[]
            {
                new BrushSize { id = "1x1", label = "1×1", size = new Vector2Int(1, 1) },
                new BrushSize { id = "1x2", label = "1×2", size = new Vector2Int(1, 2) },
                new BrushSize { id = "2x1", label = "2×1", size = new Vector2Int(2, 1) },
                new BrushSize { id = "2x2", label = "2×2", size = new Vector2Int(2, 2) },
                new BrushSize { id = "3x3", label = "3×3", size = new Vector2Int(3, 3) },
                new BrushSize { id = "4x4", label = "4×4", size = new Vector2Int(4, 4) },
                new BrushSize { id = "5x5", label = "5×5", size = new Vector2Int(5, 5) },
                new BrushSize { id = "10x10", label = "10×10", size = new Vector2Int(10, 10) }
            };

            scheduler.airport = airport;
            scheduler.alertManager = alerts;

            ui.airport = airport;
            ui.placementController = placement;
            ui.flightScheduler = scheduler;
            ui.alertManager = alerts;
            ui.cameraController = cameraController;
        }
    }
}
#endif
