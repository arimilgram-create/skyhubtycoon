import assert from 'node:assert/strict';
import { existsSync, readFileSync } from 'node:fs';

const requiredFiles = [
  'Assets/Scripts/Data/FloorDefinition.cs',
  'Assets/Scripts/Data/BuildableDefinition.cs',
  'Assets/Scripts/Grid/GridManager.cs',
  'Assets/Scripts/Build/BuildPlacementController.cs',
  'Assets/Scripts/Build/PlacementValidator.cs',
  'Assets/Scripts/Simulation/AirportState.cs',
  'Assets/Scripts/Simulation/FlightScheduler.cs',
  'Assets/Scripts/UI/UIManager.cs',
  'Assets/Scripts/Camera/IsometricCameraController.cs',
  'Assets/Scripts/Editor/SkyHubProjectBootstrap.cs',
  'Packages/manifest.json',
  'ProjectSettings/ProjectVersion.txt'
];

for (const file of requiredFiles) {
  assert.equal(existsSync(file), true, `${file} should exist for the Unity project`);
}

const bootstrap = readFileSync('Assets/Scripts/Editor/SkyHubProjectBootstrap.cs', 'utf8');
assert.match(bootstrap, /MenuItem\("Tools\/SkyHub Tycoon\/Create Full Starter Project"\)/, 'bootstrap exposes a Unity menu item');
assert.match(bootstrap, /InitializeOnLoadMethod/, 'bootstrap auto-generates starter scene/assets on first Unity import');
assert.match(bootstrap, /SkyHubTycoon\.unity/, 'bootstrap creates the starter Unity scene');

const validator = readFileSync('Assets/Scripts/Build/PlacementValidator.cs', 'utf8');
for (const rule of ['ValidateSmallGate', 'ValidateSecurity', 'ValidateCheckIn', 'ValidateRunway', 'ValidateFloor']) {
  assert.match(validator, new RegExp(rule), `${rule} should be implemented`);
}

console.log(`Unity project structure verified with ${requiredFiles.length} required files.`);
