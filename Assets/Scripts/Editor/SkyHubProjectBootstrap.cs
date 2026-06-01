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
                floors["public"], floors["secure"], floors["waiting"], floors["gate"], floors["baggage"], floors["shop"], floors["bathroom"], floors["staff"], floors["vip"], floors["customs"], floors["maintenance"], floors["airfield"]
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
            AddFloor(result, "maintenance", "Maintenance floor", ZoneType.Maintenance, new Color(0.5f, 0.56f, 0.62f), 75, false, true, false, false, floorPrefab);
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
            buildables.Add(AddBuildable("passport", "Passport control", BuildableType.PassportControl, BuildCategory.PassengerProcessing, new Vector2Int(2, 2), 4500, new[] { floors["customs"] }, "International processing requires customs floor and airport level 5.", new Color(0.8f, 0.45f, 0.9f), 3, 0, 0, 0, 5));
            buildables.Add(AddBuildable("publicDoor", "Public door", BuildableType.PublicDoor, BuildCategory.WallsAndDoors, Vector2Int.one, 220, new[] { floors["public"], floors["waiting"], floors["gate"] }, "Must connect two public floor areas.", new Color(0.7f, 0.92f, 1f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("staffDoor", "Staff-only door", BuildableType.StaffDoor, BuildCategory.WallsAndDoors, Vector2Int.one, 260, new[] { floors["staff"], floors["baggage"], floors["maintenance"] }, "Must be placed on staff-accessible floor.", new Color(0.7f, 0.8f, 0.9f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("emergencyExit", "Emergency exit", BuildableType.EmergencyExit, BuildCategory.WallsAndDoors, Vector2Int.one, 400, new[] { floors["public"], floors["gate"], floors["waiting"] }, "Emergency exits must lead outside.", new Color(0.95f, 0.25f, 0.25f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("largeCheckin", "Large check-in desk", BuildableType.LargeCheckIn, BuildCategory.PassengerProcessing, new Vector2Int(2, 2), 1200, new[] { floors["public"] }, "Must be indoors in Check-In flow with queue space and entrance access.", new Color(1f, 0.78f, 0.38f), 1, 0, 0, 0, 2));
            buildables.Add(AddBuildable("metalDetector", "Metal detector", BuildableType.MetalDetector, BuildCategory.PassengerProcessing, new Vector2Int(1, 2), 900, new[] { floors["secure"] }, "Must be placed in the security zone after check-in.", new Color(0.55f, 0.5f, 0.95f), 2, 0, 0, 0));
            buildables.Add(AddBuildable("customsDesk", "Customs desk", BuildableType.CustomsDesk, BuildCategory.PassengerProcessing, new Vector2Int(2, 1), 3500, new[] { floors["customs"] }, "International arrivals require customs floor and passport control.", new Color(0.85f, 0.5f, 0.9f), 2, 0, 0, 0, 5));
            buildables.Add(AddBuildable("bench", "Bench seating", BuildableType.Bench, BuildCategory.Comfort, new Vector2Int(2, 1), 260, new[] { floors["waiting"], floors["gate"] }, "Place after security on waiting or gate floors.", new Color(0.2f, 0.6f, 1f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("luxurySeating", "Luxury seating", BuildableType.LuxurySeating, BuildCategory.Comfort, new Vector2Int(2, 2), 950, new[] { floors["vip"], floors["gate"] }, "Luxury seating requires VIP or gate floor.", new Color(0.95f, 0.78f, 0.35f), 0, 0, 0, 0, 4));
            buildables.Add(AddBuildable("mediumGate", "Medium boarding gate", BuildableType.MediumGate, BuildCategory.GatesAndFlights, new Vector2Int(4, 3), 12000, new[] { floors["gate"] }, "Needs secure path, nearby seating, runway, taxiway, and aircraft stand.", new Color(0.58f, 0.86f, 1f), 5, 0, 0, 0, 3));
            buildables.Add(AddBuildable("largeGate", "Large boarding gate", BuildableType.LargeGate, BuildCategory.GatesAndFlights, new Vector2Int(5, 4), 26000, new[] { floors["gate"] }, "Large gates need strong secure flow, seating, taxiway, and runway access.", new Color(0.45f, 0.72f, 1f), 8, 0, 0, 0, 4));
            buildables.Add(AddBuildable("internationalGate", "International gate", BuildableType.InternationalGate, BuildCategory.GatesAndFlights, new Vector2Int(5, 4), 36000, new[] { floors["gate"], floors["customs"] }, "International gates require passport control, customs, reputation, and a large runway.", new Color(0.55f, 0.45f, 1f), 10, 0, 0, 0, 5));
            buildables.Add(AddBuildable("boardingDesk", "Boarding desk", BuildableType.BoardingDesk, BuildCategory.GatesAndFlights, new Vector2Int(2, 1), 1100, new[] { floors["gate"] }, "Boarding desks must be inside a gate zone.", new Color(0.75f, 0.88f, 1f), 1, 0, 0, 0));
            buildables.Add(AddBuildable("jetBridge", "Jet bridge", BuildableType.JetBridge, BuildCategory.GatesAndFlights, new Vector2Int(2, 3), 8500, new[] { floors["gate"], floors["airfield"] }, "Jet bridges must bridge gate floor and aircraft stand.", new Color(0.85f, 0.9f, 0.96f), 2, 0, 0, 0, 4));
            buildables.Add(AddBuildable("aircraftStand", "Aircraft stand", BuildableType.AircraftStand, BuildCategory.Airfield, new Vector2Int(4, 6), 6500, new[] { floors["airfield"] }, "Aircraft stands must connect to taxiway and face a gate.", new Color(0.25f, 0.3f, 0.36f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("mediumRunway", "Medium runway", BuildableType.MediumRunway, BuildCategory.Airfield, new Vector2Int(22, 4), 45000, new[] { floors["airfield"] }, "Medium runway must be straight and outdoors on airfield pavement.", new Color(0.08f, 0.1f, 0.14f), 0, 0, 0, 0, 3));
            buildables.Add(AddBuildable("largeRunway", "Large runway", BuildableType.LargeRunway, BuildCategory.Airfield, new Vector2Int(24, 5), 90000, new[] { floors["airfield"] }, "Large runway must be straight and outdoors on airfield pavement.", new Color(0.06f, 0.08f, 0.12f), 0, 0, 0, 0, 4));
            buildables.Add(AddBuildable("internationalRunway", "International runway", BuildableType.InternationalRunway, BuildCategory.Airfield, new Vector2Int(24, 6), 150000, new[] { floors["airfield"] }, "International runway supports jumbo flights and requires high airport level.", new Color(0.04f, 0.06f, 0.1f), 0, 0, 0, 0, 5));
            buildables.Add(AddBuildable("serviceRoad", "Service road", BuildableType.ServiceRoad, BuildCategory.Airfield, new Vector2Int(2, 2), 500, new[] { floors["airfield"] }, "Service roads must be outdoors on airfield pavement.", new Color(0.28f, 0.28f, 0.28f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("fuelStation", "Fuel station", BuildableType.FuelStation, BuildCategory.Airfield, new Vector2Int(2, 2), 8000, new[] { floors["airfield"] }, "Fuel stations need service road access and must stay away from passenger entrances.", new Color(1f, 0.55f, 0.15f), 3, 0, 0, 0, 4));
            buildables.Add(AddBuildable("maintenanceHangar", "Maintenance hangar", BuildableType.MaintenanceHangar, BuildCategory.Airfield, new Vector2Int(5, 4), 22000, new[] { floors["airfield"] }, "Maintenance hangars require service road, maintenance room, and power.", new Color(0.5f, 0.55f, 0.6f), 6, 0, 0, 0, 4));
            buildables.Add(AddBuildable("controlTower", "Control tower", BuildableType.ControlTower, BuildCategory.Airfield, new Vector2Int(2, 2), 30000, new[] { floors["airfield"], floors["staff"] }, "Control tower requires airport level 5 and power.", new Color(0.7f, 0.85f, 1f), 8, 0, 0, 0, 5));
            buildables.Add(AddBuildable("conveyorCorner", "Conveyor corner", BuildableType.ConveyorCorner, BuildCategory.Baggage, Vector2Int.one, 160, new[] { floors["baggage"] }, "Conveyor corners must be on baggage floor.", new Color(0.18f, 0.18f, 0.2f), 1, 0, 0, 0));
            buildables.Add(AddBuildable("conveyorWallPort", "Conveyor wall port", BuildableType.ConveyorWallPort, BuildCategory.Baggage, Vector2Int.one, 240, new[] { floors["baggage"] }, "Wall ports let baggage belts cross walls.", new Color(0.25f, 0.25f, 0.28f), 1, 0, 0, 0));
            buildables.Add(AddBuildable("sortingMachine", "Sorting machine", BuildableType.SortingMachine, BuildCategory.Baggage, new Vector2Int(3, 2), 6500, new[] { floors["baggage"] }, "Sorting machines require connected conveyors and baggage staff.", new Color(0.85f, 0.58f, 0.2f), 4, 0, 0, 0, 3));
            buildables.Add(AddBuildable("cartLoadingZone", "Cart loading zone", BuildableType.CartLoadingZone, BuildCategory.Baggage, new Vector2Int(2, 2), 1800, new[] { floors["baggage"], floors["airfield"] }, "Cart loading zones must connect baggage belts to aircraft stands.", new Color(0.95f, 0.65f, 0.25f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("trashBin", "Trash bin", BuildableType.TrashBin, BuildCategory.Comfort, Vector2Int.one, 80, new[] { floors["public"], floors["waiting"], floors["gate"], floors["shop"] }, "Trash bins must be passenger-accessible.", new Color(0.2f, 0.45f, 0.25f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("vendingMachine", "Vending machine", BuildableType.VendingMachine, BuildCategory.Comfort, Vector2Int.one, 650, new[] { floors["public"], floors["waiting"], floors["gate"], floors["shop"] }, "Vending machines must face a passenger path.", new Color(0.9f, 0.15f, 0.2f), 1, 0, 0, 0));
            buildables.Add(AddBuildable("infoScreen", "Info screen", BuildableType.InfoScreen, BuildCategory.Comfort, Vector2Int.one, 500, new[] { floors["public"], floors["waiting"], floors["gate"] }, "Info screens need passenger visibility and power.", new Color(0.15f, 0.35f, 1f), 1, 0, 0, 0));
            buildables.Add(AddBuildable("plant", "Plant", BuildableType.Plant, BuildCategory.Decorations, Vector2Int.one, 120, new[] { floors["public"], floors["waiting"], floors["gate"], floors["shop"], floors["vip"] }, "Decorations must not block critical paths.", new Color(0.2f, 0.8f, 0.3f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("snackKiosk", "Snack kiosk", BuildableType.SnackKiosk, BuildCategory.Shops, new Vector2Int(2, 1), 1600, new[] { floors["shop"], floors["waiting"] }, "Snack kiosks must face a passenger path.", new Color(1f, 0.62f, 0.22f), 1, 0, 0, 0, 2));
            buildables.Add(AddBuildable("restaurant", "Restaurant", BuildableType.Restaurant, BuildCategory.Shops, new Vector2Int(4, 3), 9000, new[] { floors["shop"] }, "Restaurants need shop floor, water, trash bins, and passenger access.", new Color(0.9f, 0.35f, 0.18f), 3, 4, 0, 0, 3));
            buildables.Add(AddBuildable("giftShop", "Gift shop", BuildableType.GiftShop, BuildCategory.Shops, new Vector2Int(3, 2), 4500, new[] { floors["shop"] }, "Gift shops must face passenger paths.", new Color(0.9f, 0.65f, 0.2f), 2, 0, 0, 0, 3));
            buildables.Add(AddBuildable("dutyFree", "Duty-free shop", BuildableType.DutyFreeShop, BuildCategory.Shops, new Vector2Int(4, 3), 14000, new[] { floors["shop"], floors["gate"], floors["customs"] }, "Duty-free shops require secure international passenger flow.", new Color(0.75f, 0.5f, 1f), 3, 0, 0, 0, 5));
            buildables.Add(AddBuildable("bookstore", "Bookstore", BuildableType.Bookstore, BuildCategory.Shops, new Vector2Int(3, 2), 5200, new[] { floors["shop"], floors["waiting"] }, "Bookstores need passenger access.", new Color(0.55f, 0.35f, 0.18f), 1, 0, 0, 0, 3));
            buildables.Add(AddBuildable("locker", "Locker", BuildableType.Locker, BuildCategory.Staff, Vector2Int.one, 250, new[] { floors["staff"] }, "Lockers belong in staff rooms.", new Color(0.62f, 0.68f, 0.76f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("breakTable", "Break table", BuildableType.BreakTable, BuildCategory.Staff, new Vector2Int(2, 1), 450, new[] { floors["staff"] }, "Break furniture belongs in staff-only zones.", new Color(0.64f, 0.5f, 0.35f), 0, 0, 0, 0));
            buildables.Add(AddBuildable("cleaningCloset", "Cleaning closet", BuildableType.CleaningCloset, BuildCategory.Staff, new Vector2Int(1, 2), 800, new[] { floors["staff"] }, "Cleaning closets must connect to staff paths.", new Color(0.45f, 0.75f, 0.85f), 0, 1, 0, 0));
            buildables.Add(AddBuildable("securityOffice", "Security office", BuildableType.SecurityOffice, BuildCategory.Staff, new Vector2Int(3, 2), 3200, new[] { floors["staff"], floors["secure"] }, "Security office supports advanced security operations.", new Color(0.45f, 0.45f, 0.85f), 2, 0, 0, 0, 3));
            buildables.Add(AddBuildable("maintenanceRoom", "Maintenance room", BuildableType.MaintenanceRoom, BuildCategory.Staff, new Vector2Int(3, 2), 2800, new[] { floors["maintenance"], floors["staff"] }, "Maintenance rooms must be staff-accessible.", new Color(0.48f, 0.52f, 0.56f), 1, 0, 0, 0, 4));
            buildables.Add(AddBuildable("powerRoom", "Power room", BuildableType.PowerRoom, BuildCategory.Utilities, new Vector2Int(3, 2), 7000, new[] { floors["staff"], floors["maintenance"] }, "Power rooms belong in staff/maintenance utility space.", new Color(1f, 0.86f, 0.25f), 0, 0, 45, 0, 3));
            buildables.Add(AddBuildable("solarPanel", "Solar panel", BuildableType.SolarPanel, BuildCategory.Utilities, new Vector2Int(2, 2), 3500, new[] { floors["airfield"] }, "Solar panels must be outdoors.", new Color(0.12f, 0.25f, 0.55f), 0, 0, 9, 0, 3));
            buildables.Add(AddBuildable("light", "Airport light", BuildableType.Light, BuildCategory.Utilities, Vector2Int.one, 180, new[] { floors["public"], floors["secure"], floors["waiting"], floors["gate"], floors["airfield"] }, "Lights need floor or runway-adjacent placement.", new Color(1f, 1f, 0.65f), 1, 0, 0, 0));
            return buildables.ToArray();
        }

        private static BuildableDefinition AddBuildable(string id, string label, BuildableType type, BuildCategory category, Vector2Int size, int cost, FloorDefinition[] allowedFloors, string warning, Color tint, int powerUse, int waterUse, int powerProduction, int waterProduction, int requiredLevel = 1, int incomePerFlight = 0, int satisfactionBonus = 0, float requiredReputation = 0f)
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
            buildable.requiredAirportLevel = requiredLevel;
            buildable.requiredReputation = requiredReputation;
            buildable.incomePerFlight = incomePerFlight;
            buildable.satisfactionBonus = satisfactionBonus;
            buildable.requiresPassengerRoute = type == BuildableType.DutyFreeShop;
            buildable.requiresAirfieldRoute = type == BuildableType.FuelStation;
            buildable.requiresBaggageRoute = false;
            buildable.requiresStaffRoom = type == BuildableType.MaintenanceHangar || type == BuildableType.SecurityOffice;
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
            MissionSystem missions = root.AddComponent<MissionSystem>();
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
            missions.airport = airport;

            ui.flightScheduler = scheduler;
            ui.missionSystem = missions;
            ui.alertManager = alerts;
            ui.cameraController = cameraController;
        }
    }
}
#endif
