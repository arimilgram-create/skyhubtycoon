import assert from 'node:assert/strict';
import { BRUSHES, FLOOR_TYPES, OBJECTS, key, validatePlacement } from '../src/rules.js';

function makeState() {
  return {
    money: 100000,
    level: 1,
    brush: BRUSHES[0],
    floors: new Map(),
    objects: []
  };
}

function paint(state, type, x, y, w, h) {
  for (let yy = y; yy < y + h; yy += 1) {
    for (let xx = x; xx < x + w; xx += 1) state.floors.set(key(xx, yy), type);
  }
}

function placeObject(state, type, x, y) {
  const [w, h] = OBJECTS[type].size;
  state.objects.push({ id: `${type}-${x}-${y}`, type, x, y, w, h });
}

{
  const state = makeState();
  state.brush = BRUSHES.find(b => b.id === '2x2');
  const result = validatePlacement(state, 'floor:public', 4, 4);
  assert.equal(result.valid, true, 'first floor foundation may be placed without a connection');
  assert.equal(result.cells.length, 4, 'brush creates the expected footprint');
}

{
  const state = makeState();
  paint(state, 'public', 4, 4, 2, 2);
  state.brush = BRUSHES.find(b => b.id === '1x1');
  const result = validatePlacement(state, 'floor:secure', 14, 14);
  assert.equal(result.valid, false, 'new floors after the first must connect to the terminal');
  assert.match(result.message, /connect/i);
}

{
  const state = makeState();
  paint(state, 'public', 4, 4, 5, 5);
  const result = validatePlacement(state, 'checkin', 5, 5);
  assert.equal(result.valid, false, 'check-in desk requires an entrance first');
  assert.match(result.message, /entrance/i);
}

{
  const state = makeState();
  paint(state, 'public', 1, 1, 8, 8);
  placeObject(state, 'entrance', 1, 1);
  const result = validatePlacement(state, 'checkin', 4, 4);
  assert.equal(result.valid, true, 'check-in desk is valid on public floor with entrance and queue tiles');
}

{
  const state = makeState();
  paint(state, 'gate', 10, 10, 5, 5);
  placeObject(state, 'entrance', 1, 1);
  placeObject(state, 'checkin', 3, 3);
  placeObject(state, 'security', 5, 5);
  placeObject(state, 'runway', 0, 15);
  placeObject(state, 'taxiway', 7, 15);
  const missingSeats = validatePlacement(state, 'smallGate', 11, 11);
  assert.equal(missingSeats.valid, false, 'gate cannot be placed before nearby seating exists');
  placeObject(state, 'seating', 10, 9);
  const withSeats = validatePlacement(state, 'smallGate', 11, 11);
  assert.equal(withSeats.valid, true, 'gate becomes valid after seating, runway, taxiway, and secure processing exist');
}

{
  const state = makeState();
  paint(state, 'public', 2, 2, 4, 4);
  const result = validatePlacement(state, 'runway', 2, 2);
  assert.equal(result.valid, false, 'runways cannot be placed indoors on public terminal floors');
  assert.match(result.message, /outdoors|Runway/i);
}

console.log(`Validated ${Object.keys(FLOOR_TYPES).length} floor types and ${Object.keys(OBJECTS).length} airport objects.`);
