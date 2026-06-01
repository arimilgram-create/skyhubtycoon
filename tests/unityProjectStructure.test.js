import assert from 'node:assert/strict';
import { existsSync, readFileSync } from 'node:fs';

const requiredPaths = [
  'Assets',
  'Packages',
  'ProjectSettings',
  'Assets/Scenes',
  'Assets/Scripts',
  'Assets/Prefabs',
  'Assets/Materials',
  'Assets/Audio',
  'Assets/UI',
  'Assets/Scripts/Data/FloorDefinition.cs',
  'Assets/Scripts/Data/BuildableDefinition.cs',
  'Assets/Scripts/Grid/GridManager.cs',
  'Assets/Scripts/Build/BuildPlacementController.cs',
  'Assets/Scripts/Build/PlacementValidator.cs',
  'Assets/Scripts/Simulation/AirportState.cs',
  'Assets/Scripts/Simulation/FlightScheduler.cs',
  'Assets/Scripts/Simulation/MissionSystem.cs',
  'Assets/Scripts/UI/UIManager.cs',
  'Assets/Scripts/Camera/IsometricCameraController.cs',
  'Assets/Scripts/Editor/SkyHubProjectBootstrap.cs',
  'Packages/manifest.json',
  'ProjectSettings/ProjectVersion.txt',
  'ProjectSettings/EditorBuildSettings.asset',
  'BUILD_INSTRUCTIONS.md'
];

for (const path of requiredPaths) {
  assert.equal(existsSync(path), true, `${path} should exist for the WebGL-ready Unity project`);
}

const bootstrap = readFileSync('Assets/Scripts/Editor/SkyHubProjectBootstrap.cs', 'utf8');
assert.match(bootstrap, /MenuItem\("Tools\/SkyHub Tycoon\/Create Full Starter Project"\)/, 'bootstrap exposes a Unity menu item');
assert.match(bootstrap, /InitializeOnLoadMethod/, 'bootstrap auto-generates starter scene/assets on first Unity import');
assert.match(bootstrap, /Assets\/Scenes\/MainScene\.unity/, 'bootstrap creates MainScene.unity in Assets/Scenes');
assert.match(bootstrap, /BuildTarget\.WebGL/, 'bootstrap switches/configures WebGL as the target');
assert.match(bootstrap, /EditorBuildSettings\.scenes/, 'bootstrap writes MainScene into Build Settings');

const editorBuildSettings = readFileSync('ProjectSettings/EditorBuildSettings.asset', 'utf8');
assert.match(editorBuildSettings, /Assets\/Scenes\/MainScene\.unity/, 'MainScene is present in serialized Build Settings as Scene 0');

const ui = readFileSync('Assets/Scripts/UI/UIManager.cs', 'utf8');
for (const marker of ['BuildStartMenu', 'BuildPauseMenu', 'Escape', 'RestartScene', 'ShowMainMenu', 'ScaleWithScreenSize', '1920f, 1080f', 'missionSystem']) {
  assert.match(ui, new RegExp(marker), `UIManager should include ${marker}`);
}
assert.doesNotMatch(ui, /Application\.Quit/, 'WebGL UI should not call Application.Quit');

const bootstrapDefinitions = (bootstrap.match(/AddBuildable\(/g) || []).length;
assert.ok(bootstrapDefinitions >= 55, `expected expanded airport catalog, found ${bootstrapDefinitions} buildable definitions`);
assert.match(bootstrap, /InternationalGate/, 'catalog should include international gates');
assert.match(bootstrap, /MaintenanceHangar/, 'catalog should include maintenance hangars');
assert.match(bootstrap, /DutyFreeShop/, 'catalog should include duty-free shops');

const placement = readFileSync('Assets/Scripts/Build/PlacementValidator.cs', 'utf8');
for (const rule of ['ValidateGate', 'ValidateSecurity', 'ValidateCheckIn', 'ValidateRunway', 'ValidateFloor', 'ValidateMaintenanceHangar']) {
  assert.match(placement, new RegExp(rule), `${rule} should be implemented`);
}

console.log(`Unity WebGL project structure verified with ${requiredPaths.length} required paths.`);
