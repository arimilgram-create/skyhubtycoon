(function () {
  'use strict';

  const TILE = 32;
  const GRID_WIDTH = 30;
  const GRID_HEIGHT = 30;

  const floors = {
    public: { id: 'public', name: 'Public Floor', color: '#8fc8ff', cost: 20, walk: ['passenger', 'staff'] },
    secure: { id: 'secure', name: 'Secure Floor', color: '#bba7ff', cost: 35, walk: ['passenger', 'staff'] },
    waiting: { id: 'waiting', name: 'Waiting Carpet', color: '#84d6c3', cost: 30, walk: ['passenger', 'staff'] },
    gate: { id: 'gate', name: 'Gate Floor', color: '#70aef7', cost: 40, walk: ['passenger', 'staff'] },
    baggage: { id: 'baggage', name: 'Baggage Floor', color: '#f2bd6b', cost: 30, walk: ['staff', 'baggage'] },
    shop: { id: 'shop', name: 'Shop Floor', color: '#ffd36d', cost: 45, walk: ['passenger', 'staff'] },
    staff: { id: 'staff', name: 'Staff Floor', color: '#b7c4d6', cost: 25, walk: ['staff'] },
    bathroom: { id: 'bathroom', name: 'Bathroom Floor', color: '#9ee7ee', cost: 30, walk: ['passenger', 'staff'] },
    customs: { id: 'customs', name: 'Customs Floor', color: '#d598d9', cost: 80, walk: ['passenger', 'staff'] },
    airfield: { id: 'airfield', name: 'Airfield Pavement', color: '#5f7285', cost: 18, walk: ['vehicle'] }
  };

  const tools = [
    { id: 'floor_public', type: 'floor', floor: 'public', name: 'Public Floor', brush: [3, 3] },
    { id: 'floor_secure', type: 'floor', floor: 'secure', name: 'Secure Floor', brush: [3, 3] },
    { id: 'floor_waiting', type: 'floor', floor: 'waiting', name: 'Waiting Floor', brush: [3, 3] },
    { id: 'floor_gate', type: 'floor', floor: 'gate', name: 'Gate Floor', brush: [3, 3] },
    { id: 'floor_baggage', type: 'floor', floor: 'baggage', name: 'Baggage Floor', brush: [3, 3] },
    { id: 'floor_shop', type: 'floor', floor: 'shop', name: 'Shop Floor', brush: [2, 2] },
    { id: 'floor_staff', type: 'floor', floor: 'staff', name: 'Staff Floor', brush: [2, 2] },
    { id: 'floor_bathroom', type: 'floor', floor: 'bathroom', name: 'Bathroom Floor', brush: [2, 2] },
    { id: 'floor_customs', type: 'floor', floor: 'customs', name: 'Customs Floor', brush: [2, 2], level: 5 },
    { id: 'floor_airfield', type: 'floor', floor: 'airfield', name: 'Airfield Pavement', brush: [4, 4] },
    { id: 'entrance', type: 'object', name: 'Entrance', icon: '🚪', size: [2, 1], cost: 900, floors: ['public'], color: '#63e5ff' },
    { id: 'checkin', type: 'object', name: 'Check-in', icon: '🧳', size: [2, 1], cost: 500, floors: ['public'], color: '#f2b84b' },
    { id: 'security', type: 'object', name: 'Security', icon: '🛂', size: [2, 2], cost: 1500, floors: ['secure'], color: '#9e85ff', power: 4 },
    { id: 'seat', type: 'object', name: 'Seats', icon: '💺', size: [2, 1], cost: 180, floors: ['waiting', 'gate'], color: '#35a2ff', satisfaction: 2 },
    { id: 'gateSmall', type: 'object', name: 'Small Gate', icon: '🛫', size: [3, 2], cost: 5000, floors: ['gate'], color: '#bfeeff', power: 3 },
    { id: 'runwaySmall', type: 'object', name: 'Small Runway', icon: '▰', size: [20, 3], cost: 20000, floors: ['airfield'], color: '#202a38' },
    { id: 'taxiway', type: 'object', name: 'Taxiway', icon: '═', size: [2, 2], cost: 600, floors: ['airfield'], color: '#303b4d' },
    { id: 'bagDrop', type: 'object', name: 'Bag Drop', icon: '📦', size: [1, 1], cost: 800, floors: ['public', 'baggage'], color: '#ed8f36' },
    { id: 'conveyor', type: 'object', name: 'Conveyor', icon: '➜', size: [1, 1], cost: 120, floors: ['baggage'], color: '#22252b', power: 1 },
    { id: 'sorter', type: 'object', name: 'Sorter', icon: '⚙', size: [2, 2], cost: 3000, floors: ['baggage'], color: '#d89028', power: 4 },
    { id: 'carousel', type: 'object', name: 'Carousel', icon: '🔁', size: [3, 2], cost: 4000, floors: ['baggage'], color: '#b9712f', power: 2 },
    { id: 'bathroom', type: 'object', name: 'Bathroom', icon: '🚻', size: [3, 2], cost: 1200, floors: ['bathroom'], color: '#a8eff7', water: 3, satisfaction: 3 },
    { id: 'coffee', type: 'object', name: 'Coffee', icon: '☕', size: [2, 1], cost: 2000, floors: ['shop', 'waiting'], color: '#b46b35', power: 1, water: 1, income: 160, satisfaction: 3 },
    { id: 'restaurant', type: 'object', name: 'Restaurant', icon: '🍽', size: [3, 2], cost: 6000, floors: ['shop'], color: '#e76f43', power: 3, water: 3, income: 420, satisfaction: 5, level: 3 },
    { id: 'staffRoom', type: 'object', name: 'Staff Room', icon: '👷', size: [3, 2], cost: 1800, floors: ['staff'], color: '#b7c4d6' },
    { id: 'generator', type: 'object', name: 'Generator', icon: '⚡', size: [2, 2], cost: 2500, floors: ['staff', 'airfield'], color: '#ffe45b', powerOut: 20 },
    { id: 'waterHub', type: 'object', name: 'Water Hub', icon: '💧', size: [2, 2], cost: 1900, floors: ['staff'], color: '#3fa7ff', waterOut: 12 },
    { id: 'passport', type: 'object', name: 'Passport', icon: '🛃', size: [2, 2], cost: 4500, floors: ['customs'], color: '#d786e5', power: 3, level: 5 }
  ];

  window.SkyHubLevels = {
    tileSize: TILE,
    firstLevel: {
      id: 'starter-airport',
      name: 'Starter Airport',
      width: GRID_WIDTH,
      height: GRID_HEIGHT,
      startingMoney: 85000,
      startingLevel: 1,
      floors,
      tools,
      missions: [
        'Build a working passenger route',
        'Schedule 3 flights',
        'Handle 100 passengers',
        'Reach 80% satisfaction',
        'Unlock international processing'
      ]
    }
  };
}());
