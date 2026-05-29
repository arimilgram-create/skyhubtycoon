import { BRUSHES, BUILD_CATEGORIES, FLOOR_TYPES, GRID_SIZE, LEVELS, OBJECTS, footprintCells, key, validatePlacement } from './rules.js';

const world = document.querySelector('#world');
const buildMenu = document.querySelector('#buildMenu');
const alerts = document.querySelector('#alerts');
const systems = document.querySelector('#systems');
const staffList = document.querySelector('#staffList');
const unlockTree = document.querySelector('#unlockTree');
const scheduleList = document.querySelector('#scheduleList');
const missionList = document.querySelector('#missionList');
const selectedToolName = document.querySelector('#selectedToolName');
const placementHint = document.querySelector('#placementHint');
const camera = document.querySelector('#camera');

const state = {
  money: 85000,
  satisfaction: 82,
  reputation: 1.3,
  level: 1,
  handledFlights: 0,
  passengers: 0,
  flights: [],
  staff: { janitors: 1, security: 0, checkin: 0, gate: 0, baggage: 0 },
  selectedTool: 'floor:public',
  brush: BRUSHES[3],
  floors: new Map(),
  objects: [],
  preview: null,
  rotation: 0,
  showGrid: true,
  cutaway: true,
  roofs: false,
  bulldoze: false
};

function init() {
  renderBuildMenu();
  renderWorld();
  renderHud();
  renderSidebars();
  bindControls();
  seedGuideGhosts();
}

function seedGuideGhosts() {
  pushAlert('info', 'Start by painting public terminal floors, then add entrance → check-in → security → waiting seats → runway/taxiway → gate.');
}

function renderBuildMenu() {
  buildMenu.innerHTML = '';
  const brushStrip = document.createElement('div');
  brushStrip.className = 'brush-strip';
  BRUSHES.forEach(brush => {
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = `brush ${state.brush.id === brush.id ? 'is-active' : ''}`;
    btn.textContent = brush.label;
    btn.addEventListener('click', () => { state.brush = brush; renderBuildMenu(); updateSelectedReadout(); });
    brushStrip.append(btn);
  });
  buildMenu.append(brushStrip);

  BUILD_CATEGORIES.forEach(category => {
    const section = document.createElement('section');
    section.className = 'build-category';
    section.innerHTML = `<h3>${category.label}</h3>`;
    const list = document.createElement('div');
    list.className = 'tool-list';
    category.items.forEach(item => {
      const isFloor = item.startsWith('floor:');
      const id = isFloor ? item.split(':')[1] : item;
      const def = isFloor ? FLOOR_TYPES[id] : OBJECTS[id];
      const btn = document.createElement('button');
      btn.type = 'button';
      btn.className = `tool ${state.selectedTool === item ? 'is-active' : ''}`;
      btn.dataset.tool = item;
      btn.innerHTML = `<span class="tool__swatch" style="--swatch:${isFloor ? def.color : '#ffffff'}">${isFloor ? '' : def.icon}</span><span>${def.label}</span><small>$${def.cost ?? 0}</small>`;
      btn.addEventListener('click', () => selectTool(item));
      list.append(btn);
    });
    section.append(list);
    buildMenu.append(section);
  });
}

function renderWorld() {
  world.style.setProperty('--grid-size', GRID_SIZE);
  world.innerHTML = '';
  for (let y = 0; y < GRID_SIZE; y += 1) {
    for (let x = 0; x < GRID_SIZE; x += 1) {
      const tile = document.createElement('button');
      tile.type = 'button';
      tile.className = 'tile';
      tile.dataset.x = x;
      tile.dataset.y = y;
      const floor = state.floors.get(key(x, y));
      if (floor) {
        tile.classList.add('has-floor', `floor-${floor}`);
        tile.style.setProperty('--tile-color', FLOOR_TYPES[floor].color);
        tile.title = `${FLOOR_TYPES[floor].label} · ${x},${y}`;
      } else {
        tile.title = `Unlocked land · ${x},${y}`;
      }
      tile.addEventListener('mouseenter', () => setPreview(x, y));
      tile.addEventListener('focus', () => setPreview(x, y));
      tile.addEventListener('click', () => placeAt(x, y));
      world.append(tile);
    }
  }
  state.objects.forEach(renderObject);
  if (state.preview) renderPreview();
}

function renderObject(obj) {
  const def = OBJECTS[obj.type];
  const el = document.createElement('div');
  el.className = `object object--${obj.type}`;
  el.style.gridColumn = `${obj.x + 1} / span ${obj.w}`;
  el.style.gridRow = `${obj.y + 1} / span ${obj.h}`;
  el.innerHTML = `<span class="object__icon">${def.icon}</span><strong>${def.label}</strong>`;
  el.title = `${def.label}: ${def.warning}`;
  world.append(el);
}

function renderPreview() {
  const preview = document.createElement('div');
  preview.className = `placement-preview ${state.preview.valid ? 'is-valid' : 'is-invalid'} ${state.preview.inefficient ? 'is-warning' : ''}`;
  const minX = Math.min(...state.preview.cells.map(c => c.x));
  const minY = Math.min(...state.preview.cells.map(c => c.y));
  const maxX = Math.max(...state.preview.cells.map(c => c.x));
  const maxY = Math.max(...state.preview.cells.map(c => c.y));
  preview.style.gridColumn = `${minX + 1} / span ${maxX - minX + 1}`;
  preview.style.gridRow = `${minY + 1} / span ${maxY - minY + 1}`;
  preview.innerHTML = `<span>${state.preview.valid ? state.preview.inefficient ? '⚠' : '✓' : '!'}</span>`;
  world.append(preview);
}

function selectTool(tool) {
  state.selectedTool = tool;
  state.bulldoze = false;
  document.querySelector('#bulldozeBtn').classList.remove('is-active');
  renderBuildMenu();
  updateSelectedReadout();
}

function updateSelectedReadout() {
  const tool = state.selectedTool;
  const isFloor = tool.startsWith('floor:');
  const def = isFloor ? FLOOR_TYPES[tool.split(':')[1]] : OBJECTS[tool];
  selectedToolName.textContent = def.label;
  placementHint.textContent = isFloor ? `Brush ${state.brush.label}. Floors must connect and respect zone rules.` : def.warning;
}

function setPreview(x, y) {
  if (state.bulldoze) {
    state.preview = { valid: true, message: 'Bulldoze object or floor.', cells: [{ x, y }] };
  } else {
    state.preview = validatePlacement(state, state.selectedTool, x, y);
    placementHint.textContent = state.preview.message;
  }
  renderWorld();
}

function placeAt(x, y) {
  if (state.bulldoze) return bulldozeAt(x, y);
  const validation = validatePlacement(state, state.selectedTool, x, y);
  if (!validation.valid) {
    pushAlert('danger', validation.message, x, y);
    state.preview = validation;
    renderWorld();
    return;
  }
  if (state.selectedTool.startsWith('floor:')) {
    const type = state.selectedTool.split(':')[1];
    validation.cells.forEach(cell => state.floors.set(key(cell.x, cell.y), type));
    state.money -= FLOOR_TYPES[type].cost * validation.cells.length;
  } else {
    const def = OBJECTS[state.selectedTool];
    const [w, h] = def.size;
    state.objects.push({ id: crypto.randomUUID(), type: state.selectedTool, x, y, w, h });
    state.money -= def.cost;
    if (state.selectedTool === 'security') state.staff.security += 1;
    if (state.selectedTool === 'checkin') state.staff.checkin += 1;
    if (state.selectedTool === 'smallGate') state.staff.gate += 1;
  }
  recalcAirport();
  state.preview = validation;
  renderWorld();
  renderHud();
  renderSidebars();
}

function bulldozeAt(x, y) {
  const objectIndex = state.objects.findIndex(obj => x >= obj.x && y >= obj.y && x < obj.x + obj.w && y < obj.y + obj.h);
  if (objectIndex >= 0) {
    const [removed] = state.objects.splice(objectIndex, 1);
    state.money += Math.round(OBJECTS[removed.type].cost * 0.35);
    pushAlert('info', `Bulldozed ${OBJECTS[removed.type].label}.`);
  } else if (state.floors.delete(key(x, y))) {
    state.money += 10;
  }
  recalcAirport();
  renderWorld();
  renderHud();
  renderSidebars();
}

function recalcAirport() {
  const powerUsed = sumObjects('power');
  const powerMade = sumObjects('production');
  const waterUsed = sumObjects('water');
  const waterMade = state.objects.filter(o => o.type === 'waterHub').length * 12;
  const passengerRoute = hasPassengerRoute();
  const airRoute = state.objects.some(o => o.type === 'runway') && state.objects.some(o => o.type === 'taxiway') && state.objects.some(o => o.type === 'smallGate');

  state.satisfaction = Math.max(28, Math.min(98, 82 + count('seating') * 2 + count('coffee') * 3 + count('bathroom') * 3 - (powerUsed > powerMade ? 12 : 0) - (waterUsed > waterMade ? 8 : 0) - (!passengerRoute ? 10 : 0)));
  state.reputation = Math.min(5, 1.3 + state.handledFlights * 0.08 + state.satisfaction / 120);
  if (state.passengers >= 100 && state.level < 2) unlockLevel(2);
  if (state.satisfaction >= 70 && state.level < 3) unlockLevel(3);
  if (state.passengers >= 1000 && state.level < 4) unlockLevel(4);
  if (state.reputation >= 4.5 && state.level < 5) unlockLevel(5);

  const problems = [];
  if (!passengerRoute) problems.push('No complete passenger route from entrance to check-in, security, waiting, gate, and exit.');
  if (!airRoute) problems.push('Gate, runway, and taxiway must all exist before flights operate.');
  if (powerUsed > powerMade) problems.push('Power grid overloaded. Add generators or power rooms.');
  if (waterUsed > waterMade) problems.push('Water demand exceeds plumbing hub capacity.');
  state.systemProblems = problems;
}

function hasPassengerRoute() {
  return ['entrance', 'checkin', 'security', 'seating', 'smallGate'].every(type => state.objects.some(o => o.type === type));
}

function scheduleFlight() {
  recalcAirport();
  if (state.systemProblems.length) {
    pushAlert('danger', `Flight cannot operate. ${state.systemProblems[0]}`);
    renderSidebars();
    return;
  }
  const passengers = 34 + Math.floor(Math.random() * 18);
  const delayed = state.satisfaction < 65;
  const reward = delayed ? 1200 : 2400;
  state.flights.unshift({ id: state.flights.length + 1, code: `SH ${120 + state.flights.length}`, passengers, status: delayed ? 'Delayed' : 'On time', reward });
  state.handledFlights += delayed ? 0 : 1;
  state.passengers += passengers;
  state.money += reward + count('coffee') * 160;
  if (delayed) state.satisfaction -= 6;
  pushAlert(delayed ? 'warn' : 'success', `${delayed ? 'Delayed' : 'Completed'} flight SH ${119 + state.flights.length}: ${passengers} passengers, +$${reward}.`);
  recalcAirport();
  renderHud();
  renderSidebars();
}

function renderHud() {
  document.querySelector('#money').textContent = money(state.money);
  document.querySelector('#satisfaction').textContent = `${Math.round(state.satisfaction)}%`;
  document.querySelector('#reputation').textContent = `★ ${state.reputation.toFixed(1)}`;
  document.querySelector('#flightCount').textContent = `${state.handledFlights} / 3`;
}

function renderSidebars() {
  recalcAirport();
  alerts.innerHTML = '';
  const messages = state.systemProblems?.length ? state.systemProblems : ['Airport systems nominal. Place more zones to grow.'];
  messages.forEach(message => appendAlert(message.includes('nominal') ? 'success' : 'warn', message));
  systems.innerHTML = systemRow('Passenger route', hasPassengerRoute()) + systemRow('Baggage route', count('bagDrop') && count('conveyor') && count('carousel')) + systemRow('Airfield route', count('runway') && count('taxiway') && count('smallGate')) + systemRow('Power', sumObjects('power') <= sumObjects('production'), `${sumObjects('power')} / ${sumObjects('production')}`) + systemRow('Water', sumObjects('water') <= count('waterHub') * 12, `${sumObjects('water')} / ${count('waterHub') * 12}`);
  staffList.innerHTML = Object.entries(state.staff).map(([role, qty]) => `<div><span>${title(role)}</span><strong>${qty}</strong></div>`).join('');
  unlockTree.innerHTML = LEVELS.map(level => `<article class="unlock ${state.level >= level.level ? 'is-unlocked' : ''}"><strong>Level ${level.level}: ${level.name}</strong><span>${level.goal}</span><small>${level.unlocks.join(' · ')}</small></article>`).join('');
  missionList.innerHTML = ['Build your first working airport', 'Handle 50 passengers', 'Handle 10 flights without delay', 'Reach 80% satisfaction', 'Unlock international flights'].map((goal, i) => `<li class="${missionDone(i) ? 'done' : ''}">${goal}</li>`).join('');
  scheduleList.innerHTML = state.flights.length ? state.flights.slice(0, 4).map(f => `<div class="flight"><strong>${f.code}</strong><span>${f.status}</span><small>${f.passengers} pax · +$${f.reward}</small></div>`).join('') : '<p>No flights scheduled yet.</p>';
}

function missionDone(i) {
  return [hasPassengerRoute(), state.passengers >= 50, state.handledFlights >= 10, state.satisfaction >= 80, state.level >= 5][i];
}

function bindControls() {
  document.querySelector('#scheduleBtn').addEventListener('click', scheduleFlight);
  document.querySelector('#hireBtn').addEventListener('click', () => {
    if (state.money < 700) return pushAlert('danger', 'Not enough money to hire staff.');
    state.money -= 700;
    const roles = Object.keys(state.staff);
    const role = roles[Math.floor(Math.random() * roles.length)];
    state.staff[role] += 1;
    pushAlert('success', `Hired ${title(role)} staff.`);
    renderHud(); renderSidebars();
  });
  document.querySelector('#rotateBtn').addEventListener('click', () => {
    state.rotation = (state.rotation + 90) % 360;
    camera.style.setProperty('--rotation', `${state.rotation}deg`);
  });
  document.querySelector('#roofBtn').addEventListener('click', event => {
    state.roofs = !state.roofs;
    document.body.classList.toggle('show-roofs', state.roofs);
    event.target.textContent = `Roofs: ${state.roofs ? 'On' : 'Off'}`;
  });
  document.querySelector('#wallBtn').addEventListener('click', event => {
    state.cutaway = !state.cutaway;
    document.body.classList.toggle('cutaway-off', !state.cutaway);
    event.target.textContent = `Cutaway: ${state.cutaway ? 'On' : 'Off'}`;
  });
  document.querySelector('#gridBtn').addEventListener('click', event => {
    state.showGrid = !state.showGrid;
    document.body.classList.toggle('grid-off', !state.showGrid);
    event.target.textContent = `Grid: ${state.showGrid ? 'On' : 'Off'}`;
  });
  document.querySelector('#bulldozeBtn').addEventListener('click', event => {
    state.bulldoze = !state.bulldoze;
    event.target.classList.toggle('is-active', state.bulldoze);
    placementHint.textContent = state.bulldoze ? 'Click any object or floor tile to bulldoze it.' : 'Choose a build item.';
  });
  updateSelectedReadout();
}

function pushAlert(type, message) {
  appendAlert(type, message);
}
function appendAlert(type, message) {
  const alert = document.createElement('button');
  alert.type = 'button';
  alert.className = `alert alert--${type}`;
  alert.textContent = message;
  alert.addEventListener('click', () => document.querySelector('#viewport').focus());
  alerts.prepend(alert);
}
function systemRow(label, ok, detail = '') { return `<div class="system ${ok ? 'ok' : 'bad'}"><span>${label}</span><strong>${ok ? 'Online' : 'Needs work'}</strong><small>${detail}</small></div>`; }
function sumObjects(field) { return state.objects.reduce((sum, obj) => sum + (OBJECTS[obj.type][field] || 0), 0); }
function count(type) { return state.objects.filter(obj => obj.type === type).length; }
function unlockLevel(level) { state.level = level; pushAlert('success', `Unlocked Level ${level}: ${LEVELS[level - 1].name}!`); }
function money(value) { return `$${Math.round(value).toLocaleString()}`; }
function title(value) { return value.replace(/([A-Z])/g, ' $1').replace(/^./, c => c.toUpperCase()); }

init();
