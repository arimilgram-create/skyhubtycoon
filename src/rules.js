export const GRID_SIZE = 24;

export const FLOOR_TYPES = {
  public: { label: 'Public floor', zone: 'Entrance Zone', color: '#8fc8ff', walkable: ['passenger', 'staff'], cost: 45 },
  secure: { label: 'Secure floor', zone: 'Security Zone', color: '#bba7ff', walkable: ['passenger', 'staff'], cost: 70 },
  waiting: { label: 'Waiting carpet', zone: 'Waiting Zone', color: '#84d6c3', walkable: ['passenger', 'staff'], cost: 65 },
  gate: { label: 'Gate floor', zone: 'Gate Zone', color: '#70aef7', walkable: ['passenger', 'staff'], cost: 75 },
  baggage: { label: 'Baggage floor', zone: 'Baggage Zone', color: '#f2bd6b', walkable: ['staff', 'baggage'], cost: 60 },
  shop: { label: 'Shop floor', zone: 'Shops/Food Zone', color: '#ffd36d', walkable: ['passenger', 'staff'], cost: 80 },
  bathroom: { label: 'Bathroom floor', zone: 'Bathroom Zone', color: '#9ee7ee', walkable: ['passenger', 'staff'], cost: 55 },
  staff: { label: 'Staff floor', zone: 'Staff Zone', color: '#b7c4d6', walkable: ['staff'], cost: 45 },
  vip: { label: 'VIP floor', zone: 'VIP Zone', color: '#e5c16d', walkable: ['passenger', 'staff'], cost: 180 },
  customs: { label: 'Customs floor', zone: 'International Zone', color: '#d598d9', walkable: ['passenger', 'staff'], cost: 130 },
  airfield: { label: 'Airfield pavement', zone: 'Airfield Zone', color: '#5f7285', walkable: ['vehicle'], cost: 35 }
};

export const BRUSHES = [
  { id: '1x1', label: '1×1', w: 1, h: 1 },
  { id: '1x2', label: '1×2', w: 1, h: 2 },
  { id: '2x1', label: '2×1', w: 2, h: 1 },
  { id: '2x2', label: '2×2', w: 2, h: 2 },
  { id: '3x3', label: '3×3', w: 3, h: 3 },
  { id: '4x4', label: '4×4', w: 4, h: 4 },
  { id: '5x5', label: '5×5', w: 5, h: 5 },
  { id: '10x10', label: '10×10', w: 10, h: 10 }
];

export const OBJECTS = {
  entrance: {
    label: 'Entrance door', category: 'Walls and Doors', icon: '🚪', size: [2, 1], cost: 900,
    requires: ['public'], connects: ['passenger'], warning: 'Must connect outside pavement to indoor public terminal floor.',
    validate: ({ hasFloor, isOutdoorEdge, clearAhead }) => hasFloor('public') && isOutdoorEdge() && clearAhead(2)
  },
  checkin: {
    label: 'Check-in desk', category: 'Passenger Processing', icon: '🧳', size: [2, 1], cost: 500,
    requires: ['public'], connects: ['passenger', 'staff'], capacity: 20,
    warning: 'Must be indoors in the Check-In Zone with two clear queue tiles and a path to entrance.',
    validate: ({ hasFloor, hasAny, clearAhead }) => hasFloor('public') && hasAny('entrance') && clearAhead(2)
  },
  kiosk: {
    label: 'Self check-in kiosk', category: 'Passenger Processing', icon: '🏧', size: [1, 1], cost: 350,
    requires: ['public'], connects: ['passenger'], capacity: 12,
    warning: 'Must be near an entrance and on passenger-accessible floor.',
    validate: ({ hasFloor, within }) => hasFloor('public') && within('entrance', 6)
  },
  security: {
    label: 'Security checkpoint', category: 'Passenger Processing', icon: '🛂', size: [2, 3], cost: 1500,
    requires: ['secure'], connects: ['passenger', 'staff'], capacity: 35, power: 4,
    warning: 'Requires check-in first, security floor, queue space, staff access, and a route toward gates.',
    validate: ({ hasFloor, hasAny, clearAhead }) => hasFloor('secure') && hasAny('checkin', 'kiosk') && clearAhead(2)
  },
  seating: {
    label: 'Seating row', category: 'Comfort', icon: '💺', size: [2, 1], cost: 180,
    requires: ['waiting', 'gate'], connects: ['passenger'], capacity: 8,
    warning: 'Place after security on waiting or gate floors without blocking main paths.',
    validate: ({ hasFloor, hasAny }) => (hasFloor('waiting') || hasFloor('gate')) && hasAny('security')
  },
  smallGate: {
    label: 'Small boarding gate', category: 'Gates and Flights', icon: '🛫', size: [3, 2], cost: 5000,
    requires: ['gate'], connects: ['passenger', 'taxiway'], capacity: 48, power: 3,
    warning: 'Needs secure path, nearby seating, runway, taxiway, and an outside aircraft stand.',
    validate: ({ hasFloor, hasAny, within }) => hasFloor('gate') && hasAny('security') && hasAny('runway') && hasAny('taxiway') && within('seating', 10)
  },
  runway: {
    label: 'Small runway', category: 'Airfield', icon: '▰', size: [20, 3], cost: 20000,
    requires: ['airfield'], connects: ['taxiway'], warning: 'Runway must be straight, outdoors, at least 3×20, and clear of buildings.',
    validate: ({ hasFloorOnly }) => hasFloorOnly('airfield')
  },
  taxiway: {
    label: 'Taxiway', category: 'Airfield', icon: '═', size: [2, 2], cost: 600,
    requires: ['airfield'], connects: ['vehicle'], warning: 'Taxiways must be outdoors on airfield pavement and connect runway to aircraft stands.',
    validate: ({ hasFloorOnly }) => hasFloorOnly('airfield')
  },
  bagDrop: {
    label: 'Bag drop', category: 'Baggage', icon: '📦', size: [1, 1], cost: 800,
    requires: ['public', 'baggage'], connects: ['baggage', 'staff'], warning: 'Must sit next to check-in and connect to baggage conveyor.',
    validate: ({ hasFloor, adjacent }) => (hasFloor('public') || hasFloor('baggage')) && adjacent('checkin')
  },
  conveyor: {
    label: 'Conveyor belt', category: 'Baggage', icon: '➜', size: [1, 1], cost: 120,
    requires: ['baggage'], connects: ['baggage'], warning: 'Must be on baggage floor and connect edge-to-edge through wall ports.',
    validate: ({ hasFloor }) => hasFloor('baggage')
  },
  carousel: {
    label: 'Baggage carousel', category: 'Baggage', icon: '🔁', size: [3, 2], cost: 4000,
    requires: ['baggage'], connects: ['baggage', 'passenger'], warning: 'Requires gate, connected conveyor, and accessible baggage claim space.',
    validate: ({ hasFloor, hasAny, adjacent }) => hasFloor('baggage') && hasAny('smallGate') && adjacent('conveyor')
  },
  bathroom: {
    label: 'Bathroom suite', category: 'Comfort', icon: '🚻', size: [3, 2], cost: 1200,
    requires: ['bathroom'], connects: ['passenger', 'water'], water: 3,
    warning: 'Needs bathroom floor, passenger access, plumbing, and cannot open into food prep.',
    validate: ({ hasFloor, hasAny }) => hasFloor('bathroom') && hasAny('entrance')
  },
  coffee: {
    label: 'Coffee stand', category: 'Shops', icon: '☕', size: [2, 1], cost: 2000,
    requires: ['shop'], connects: ['passenger', 'water'], income: 65, water: 1, power: 1,
    warning: 'Must face a passenger path on shop floor with clear counter space.',
    validate: ({ hasFloor, clearAhead }) => hasFloor('shop') && clearAhead(1)
  },
  staffRoom: {
    label: 'Staff room', category: 'Staff', icon: '🧑‍✈️', size: [3, 2], cost: 1800,
    requires: ['staff'], connects: ['staff'], warning: 'Must be staff-only with lockers, break furniture, and staff path access.',
    validate: ({ hasFloor }) => hasFloor('staff')
  },
  generator: {
    label: 'Generator', category: 'Utilities', icon: '⚡', size: [2, 2], cost: 2500,
    requires: ['staff', 'airfield'], connects: ['power'], production: 18,
    warning: 'Place in utility/staff space or outdoors on airfield pavement.',
    validate: ({ hasFloor }) => hasFloor('staff') || hasFloor('airfield')
  },
  waterHub: {
    label: 'Plumbing hub', category: 'Utilities', icon: '💧', size: [2, 2], cost: 1900,
    requires: ['staff'], connects: ['water'], production: 12,
    warning: 'Place in staff-only utility rooms to supply bathrooms and restaurants.',
    validate: ({ hasFloor }) => hasFloor('staff')
  },
  passport: {
    label: 'Passport control', category: 'Passenger Processing', icon: '🛃', size: [2, 2], cost: 4500,
    requires: ['customs'], connects: ['passenger', 'staff'], warning: 'International processing requires customs floor and airport level 5.',
    validate: ({ hasFloor, level }) => hasFloor('customs') && level >= 5
  }
};

export const BUILD_CATEGORIES = [
  { id: 'floors', label: 'Floors', items: Object.keys(FLOOR_TYPES).map(id => `floor:${id}`) },
  { id: 'processing', label: 'Passenger Processing', items: ['entrance', 'checkin', 'kiosk', 'security', 'passport'] },
  { id: 'gates', label: 'Gates and Flights', items: ['smallGate'] },
  { id: 'airfield', label: 'Airfield', items: ['runway', 'taxiway'] },
  { id: 'baggage', label: 'Baggage', items: ['bagDrop', 'conveyor', 'carousel'] },
  { id: 'comfort', label: 'Comfort', items: ['seating', 'bathroom'] },
  { id: 'shops', label: 'Shops', items: ['coffee'] },
  { id: 'staff', label: 'Staff', items: ['staffRoom'] },
  { id: 'utilities', label: 'Utilities', items: ['generator', 'waterHub'] }
];

export const LEVELS = [
  { level: 1, name: 'Tiny Airport', goal: 'Handle first 3 flights', unlocks: ['Basic floors', 'Entrance', 'Check-in', 'Security', 'Small gate', 'Small runway'] },
  { level: 2, name: 'Local Airport', goal: 'Handle 100 passengers', unlocks: ['Baggage drops', 'Conveyors', 'Baggage claim', 'Bathrooms', 'Coffee stand'] },
  { level: 3, name: 'Regional Airport', goal: 'Reach 70% satisfaction', unlocks: ['Medium gates', 'Shops', 'Restaurants', 'Staff rooms', 'Power rooms'] },
  { level: 4, name: 'National Airport', goal: 'Handle 1,000 passengers', unlocks: ['Large gates', 'Lounges', 'Advanced baggage', 'Service roads', 'Maintenance hangars'] },
  { level: 5, name: 'International Airport', goal: 'Become a 5-star airport', unlocks: ['Passport control', 'Customs', 'Duty-free', 'Jumbo runway', 'Control tower'] }
];

export function footprintCells(x, y, w, h) {
  return Array.from({ length: h }, (_, dy) => Array.from({ length: w }, (_, dx) => ({ x: x + dx, y: y + dy }))).flat();
}

export function inBounds(x, y) {
  return x >= 0 && y >= 0 && x < GRID_SIZE && y < GRID_SIZE;
}

export function validatePlacement(state, tool, x, y) {
  const isFloor = tool.startsWith('floor:');
  const brush = state.brush;
  const def = isFloor ? { size: [brush.w, brush.h], cost: FLOOR_TYPES[tool.split(':')[1]].cost } : OBJECTS[tool];
  const [w, h] = def.size;
  const cells = footprintCells(x, y, w, h);
  const invalid = cells.find(cell => !inBounds(cell.x, cell.y));
  if (invalid) return result(false, 'Outside unlocked land.', cells);

  if (state.money < def.cost * (isFloor ? cells.length : 1)) return result(false, 'Not enough money.', cells);

  if (isFloor) {
    const type = tool.split(':')[1];
    if (cells.some(cell => state.objects.some(obj => covers(obj, cell.x, cell.y)))) return result(false, 'Cannot overlap existing objects.', cells);
    if (type !== 'airfield' && cells.some(cell => state.floors.get(key(cell.x, cell.y)) === 'airfield')) return result(false, 'Cannot overlap runway, taxiway, or airfield pavement.', cells);
    const hasExistingFloor = state.floors.size > 0;
    const connected = !hasExistingFloor || cells.some(cell => neighbors(cell.x, cell.y).some(n => state.floors.has(key(n.x, n.y))));
    if (!connected) return result(false, 'Floor must connect to an existing airport floor, entrance, service road, or terminal foundation.', cells);
    return result(true, `Paint ${FLOOR_TYPES[type].label}.`, cells);
  }

  if (cells.some(cell => state.objects.some(obj => covers(obj, cell.x, cell.y)))) return result(false, 'Any object overlapping another object is not allowed.', cells);
  const context = makeContext(state, cells, x, y);
  if (!def.validate(context)) return result(false, def.warning, cells);
  const inefficient = def.label.includes('gate') && !context.within('seating', 6);
  return result(true, inefficient ? 'Allowed, but inefficient: seating should be within 6 tiles for best boarding.' : `Place ${def.label}.`, cells, inefficient);
}

function makeContext(state, cells, x, y) {
  const floorAt = cell => state.floors.get(key(cell.x, cell.y));
  return {
    level: state.level,
    hasFloor: type => cells.every(cell => floorAt(cell) === type),
    hasFloorOnly: type => cells.every(cell => floorAt(cell) === type),
    hasAny: (...types) => state.objects.some(obj => types.includes(obj.type)),
    adjacent: type => state.objects.some(obj => footprintCells(obj.x, obj.y, obj.w, obj.h).some(c => cells.some(cell => manhattan(c, cell) === 1)) && obj.type === type),
    within: (type, range) => state.objects.some(obj => obj.type === type && manhattan({ x, y }, obj) <= range),
    isOutdoorEdge: () => x === 0 || y === 0 || x + cellsWide(cells) >= GRID_SIZE || y + cellsHigh(cells) >= GRID_SIZE || neighbors(x, y).some(n => !state.floors.has(key(n.x, n.y))),
    clearAhead: distance => {
      const frontY = y - 1;
      for (let dy = 0; dy < distance; dy += 1) {
        for (let dx = 0; dx < cellsWide(cells); dx += 1) {
          const cx = x + dx;
          const cy = frontY - dy;
          if (!inBounds(cx, cy) || state.objects.some(obj => covers(obj, cx, cy))) return false;
        }
      }
      return true;
    }
  };
}

export function key(x, y) { return `${x},${y}`; }
export function covers(obj, x, y) { return x >= obj.x && y >= obj.y && x < obj.x + obj.w && y < obj.y + obj.h; }
export function neighbors(x, y) { return [{ x: x + 1, y }, { x: x - 1, y }, { x, y: y + 1 }, { x, y: y - 1 }].filter(n => inBounds(n.x, n.y)); }
function manhattan(a, b) { return Math.abs(a.x - b.x) + Math.abs(a.y - b.y); }
function cellsWide(cells) { return Math.max(...cells.map(c => c.x)) - Math.min(...cells.map(c => c.x)) + 1; }
function cellsHigh(cells) { return Math.max(...cells.map(c => c.y)) - Math.min(...cells.map(c => c.y)) + 1; }
function result(valid, message, cells, inefficient = false) { return { valid, message, cells, inefficient }; }
