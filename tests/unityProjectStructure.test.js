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
  'Assets/Scripts/UI/UIManager.cs',
  'Assets/Scripts/Camera/IsometricCameraController.cs',
  'Assets/Scripts/Editor/SkyHubProjectBootstrap.cs',
  'Packages/manifest.json',
  'ProjectSettings/ProjectVersion.txt',
  'ProjectSettings/EditorBuildSettings.asset',
  'BUILD_INSTRUCTIONS.md',
  'GAME_MEMORY.md',
  'AGENT.md',
  'AGENTS.md'
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

for (const marker of [
  'Basic terminal floor',
  'Entrance',
  'Check-in desk',
  'Security checkpoint',
  'Waiting seat',
  'Small gate',
  'Small runway',
  'Taxiway',
  'Grid Line X',
  'Grid Line Z'
]) {
  assert.match(bootstrap, new RegExp(marker), `bootstrap should create ${marker}`);
}
assert.doesNotMatch(bootstrap, /Self check-in kiosk/, 'first playable prototype should not expose post-prototype build items');

const editorBuildSettings = readFileSync('ProjectSettings/EditorBuildSettings.asset', 'utf8');
assert.match(editorBuildSettings, /Assets\/Scenes\/MainScene\.unity/, 'MainScene is present in serialized Build Settings as Scene 0');

const ui = readFileSync('Assets/Scripts/UI/UIManager.cs', 'utf8');
for (const marker of ['BuildStartMenu', 'BuildPauseMenu', 'Escape', 'RestartScene', 'ShowMainMenu', 'ScaleWithScreenSize', '1920f, 1080f']) {
  assert.match(ui, new RegExp(marker), `UIManager should include ${marker}`);
}
assert.doesNotMatch(ui, /Application\.Quit/, 'WebGL UI should not call Application.Quit');

const camera = readFileSync('Assets/Scripts/Camera/IsometricCameraController.cs', 'utf8');
for (const marker of ['panSmoothing', 'zoomSmoothing', 'rotationSmoothing', 'Lerp', 'Rotate90']) {
  assert.match(camera, new RegExp(marker), `camera controller should include ${marker}`);
}

const placement = readFileSync('Assets/Scripts/Build/PlacementValidator.cs', 'utf8');
for (const rule of ['ValidateEntrance', 'ValidateSmallGate', 'ValidateSecurity', 'ValidateCheckIn', 'ValidateRunway', 'ValidateTaxiway', 'ValidateSeating', 'ValidateFloor']) {
  assert.match(placement, new RegExp(rule), `${rule} should be implemented`);
}

for (const marker of [
  'Floors can only be placed on empty unlocked land.',
  'Cannot overlap another object.',
  'Must be placed indoors.',
  'Must connect to terminal.',
  'Runway must be outdoors.',
  'Requires security checkpoint first.',
  'Gate must connect to taxiway.',
  'Must be placed on terminal floor.',
  'Cannot block all passenger paths.',
  'Cannot block main paths.'
]) {
  assert.match(placement, new RegExp(marker.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')), `placement validator should include warning: ${marker}`);
}


const gameMemory = readFileSync('GAME_MEMORY.md', 'utf8');
for (const marker of [
  'Current Game Vision',
  'Tech Stack',
  'Core Systems',
  'Completed Features',
  'Current Bugs',
  'Next Planned Features',
  'Important Rules and Design Decisions'
]) {
  assert.match(gameMemory, new RegExp(marker), `GAME_MEMORY.md should document ${marker}`);
}

const agentGuide = readFileSync('AGENT.md', 'utf8');
for (const marker of ['GAME_MEMORY.md', 'npm test', 'Unity 2022.3 LTS', 'WebGL']) {
  assert.match(agentGuide, new RegExp(marker), `AGENT.md should mention ${marker}`);
}

console.log(`Unity WebGL project structure verified with ${requiredPaths.length} required paths.`);
