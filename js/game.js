(function () {
  'use strict';

  const STATES = Object.freeze({ LOADING: 'loading', MENU: 'menu', PLAYING: 'playing', PAUSED: 'paused', GAME_OVER: 'gameOver' });

  class Game {
    constructor(canvas, input, ui, save) {
      this.canvas = canvas;
      this.ctx = canvas.getContext('2d');
      this.input = input;
      this.ui = ui;
      this.save = save;
      this.levelData = window.SkyHubLevels.firstLevel;
      this.camera = new window.SkyHubPlayer.PlayerCamera();
      this.state = STATES.LOADING;
      this.lastTime = 0;
      this.hoverCell = null;
      this.messages = [];
      this.resize();
      this.resetWorld();
    }

    init() {
      this.ui.bindButtons(this);
      this.input.onPause = () => this.togglePause();
      this.input.onInteract = () => this.scheduleFlight();
      this.input.onRotate = () => this.camera.rotate();
      window.addEventListener('resize', () => this.resize());
      this.setState(STATES.MENU);
      this.loop(0);
    }

    resize() {
      const shell = this.canvas.parentElement;
      const rect = shell.getBoundingClientRect();
      const scale = window.devicePixelRatio || 1;
      this.canvas.width = Math.max(800, Math.floor(rect.width * scale));
      this.canvas.height = Math.max(450, Math.floor(rect.height * scale));
    }

    resetWorld() {
      const { width, height, startingMoney, startingLevel, tools } = this.levelData;
      this.grid = Array.from({ length: height }, () => Array.from({ length: width }, () => ({ floor: null, object: null })));
      this.objects = [];
      this.money = startingMoney;
      this.satisfaction = 82;
      this.reputation = 1.3;
      this.level = startingLevel;
      this.passengers = 0;
      this.flights = 0;
      this.powerUsed = 0;
      this.powerMade = 0;
      this.waterUsed = 0;
      this.waterMade = 0;
      this.selectedToolId = tools[0].id;
      this.camera.x = 0;
      this.camera.y = 0;
      this.camera.zoom = 1;
      this.camera.rotation = 0;
      this.ui.buildTools(this.visibleTools(), this.selectedToolId, (id) => this.selectTool(id));
      this.recalculate();
    }

    setState(state) {
      this.state = state;
      this.ui.showState(state);
      this.ui.updateStats(this);
    }

    startNewGame() {
      this.resetWorld();
      this.ui.log('Welcome! Build entrance → check-in → security → seats → runway/taxiway → gate.', 'good');
      this.setState(STATES.PLAYING);
    }

    showMenu() {
      this.setState(STATES.MENU);
    }

    pause() {
      if (this.state === STATES.PLAYING) this.setState(STATES.PAUSED);
    }

    resume() {
      if (this.state === STATES.PAUSED) this.setState(STATES.PLAYING);
    }

    togglePause() {
      if (this.state === STATES.PLAYING) this.pause();
      else if (this.state === STATES.PAUSED) this.resume();
    }

    gameOver(reason) {
      const highScore = this.save.recordHighScore(this.passengers);
      this.ui.showGameOver(reason, highScore);
      this.setState(STATES.GAME_OVER);
    }

    loop(time) {
      const dt = Math.min(0.05, (time - this.lastTime) / 1000 || 0);
      this.lastTime = time;
      try {
        if (this.state === STATES.PLAYING) this.update(dt);
        this.render();
      } catch (error) {
        console.error('SkyHub Tycoon crashed during the game loop:', error);
        this.gameOver('A browser error stopped the airport simulation. Check the console.');
      }
      window.requestAnimationFrame((next) => this.loop(next));
    }

    update(dt) {
      this.camera.update(this.input, dt);
      this.hoverCell = this.screenToGrid(this.input.mouse.x, this.input.mouse.y);
      if (this.input.consumeClick() && this.hoverCell) this.tryBuild(this.hoverCell.x, this.hoverCell.y);
    }

    visibleTools() {
      return this.levelData.tools.filter((tool) => !tool.level || tool.level <= this.level);
    }

    selectTool(id) {
      this.selectedToolId = id;
      this.ui.buildTools(this.visibleTools(), this.selectedToolId, (toolId) => this.selectTool(toolId));
    }

    selectedTool() {
      return this.levelData.tools.find((tool) => tool.id === this.selectedToolId);
    }

    tryBuild(x, y) {
      const tool = this.selectedTool();
      const validation = this.validatePlacement(tool, x, y);
      if (!validation.valid) {
        this.ui.log(validation.message, 'bad');
        return;
      }

      if (tool.type === 'floor') this.placeFloor(tool, x, y, validation.cells);
      else this.placeObject(tool, x, y, validation.cells);
      this.recalculate();
      this.ui.updateStats(this);
      this.ui.buildTools(this.visibleTools(), this.selectedToolId, (toolId) => this.selectTool(toolId));
    }

    validatePlacement(tool, x, y) {
      const size = tool.type === 'floor' ? tool.brush : tool.size;
      const cells = this.footprint(x, y, size[0], size[1]);
      if (cells.some((cell) => !this.inBounds(cell.x, cell.y))) return { valid: false, message: 'Outside airport land.', cells };
      const cost = tool.type === 'floor' ? this.levelData.floors[tool.floor].cost * cells.length : tool.cost;
      if (this.money < cost) return { valid: false, message: 'Not enough money.', cells };
      if (tool.level && this.level < tool.level) return { valid: false, message: `Requires airport level ${tool.level}.`, cells };
      if (cells.some((cell) => this.grid[cell.y][cell.x].object)) return { valid: false, message: 'Cannot overlap another object.', cells };

      if (tool.type === 'floor') {
        if (this.hasAnyFloor() && !cells.some((cell) => this.neighbors(cell.x, cell.y).some((n) => this.grid[n.y][n.x].floor))) {
          return { valid: false, message: 'Floors must connect to the existing airport.', cells };
        }
        return { valid: true, message: 'Floor placed.', cells };
      }

      if (cells.some((cell) => !tool.floors.includes(this.grid[cell.y][cell.x].floor))) return { valid: false, message: `${tool.name} must be placed in the correct zone.`, cells };
      if (!this.dependenciesMet(tool, x, y)) return { valid: false, message: this.dependencyMessage(tool), cells };
      return { valid: true, message: `${tool.name} placed.`, cells };
    }

    dependenciesMet(tool, x, y) {
      const has = (id) => this.objects.some((object) => object.tool.id === id);
      const hasAny = (...ids) => ids.some(has);
      const near = (...ids) => this.objects.some((object) => ids.includes(object.tool.id) && Math.abs(object.x - x) + Math.abs(object.y - y) <= 10);
      switch (tool.id) {
        case 'entrance': return x === 0 || y === 0 || x + tool.size[0] >= this.levelData.width || y + tool.size[1] >= this.levelData.height;
        case 'checkin': return has('entrance');
        case 'security': return hasAny('checkin');
        case 'seat': return has('security');
        case 'gateSmall': return has('security') && has('runwaySmall') && has('taxiway') && near('seat');
        case 'bagDrop': return has('checkin');
        case 'sorter': return has('bagDrop') && has('conveyor');
        case 'carousel': return has('sorter') && has('conveyor');
        case 'bathroom': return has('waterHub');
        case 'coffee': return has('entrance');
        case 'restaurant': return has('waterHub') && has('coffee');
        case 'passport': return this.level >= 5;
        default: return true;
      }
    }

    dependencyMessage(tool) {
      const messages = {
        entrance: 'Entrance must be on the airport edge.',
        checkin: 'Check-in requires an entrance first.',
        security: 'Security requires check-in first.',
        seat: 'Seats must be after security.',
        gateSmall: 'Gate needs security, nearby seats, runway, and taxiway.',
        bagDrop: 'Bag drop requires check-in.',
        sorter: 'Sorter requires bag drop and conveyor.',
        carousel: 'Carousel requires sorter and conveyor.',
        bathroom: 'Bathroom needs a water hub.',
        coffee: 'Coffee stand needs passenger access.',
        restaurant: 'Restaurant needs water and coffee service.',
        passport: 'Passport control unlocks at airport level 5.'
      };
      return messages[tool.id] || 'Placement dependency is missing.';
    }

    placeFloor(tool, x, y, cells) {
      cells.forEach((cell) => { this.grid[cell.y][cell.x].floor = tool.floor; });
      this.money -= this.levelData.floors[tool.floor].cost * cells.length;
      this.ui.log(`${this.levelData.floors[tool.floor].name} painted.`, 'good');
    }

    placeObject(tool, x, y, cells) {
      const object = { id: crypto.randomUUID ? crypto.randomUUID() : `${tool.id}-${Date.now()}`, tool, x, y, cells };
      cells.forEach((cell) => { this.grid[cell.y][cell.x].object = object; });
      this.objects.push(object);
      this.money -= tool.cost;
      this.ui.log(`${tool.name} placed.`, 'good');
    }

    scheduleFlight() {
      if (this.state !== STATES.PLAYING) return;
      this.recalculate();
      if (!this.hasPassengerRoute()) return this.ui.log('No valid passenger route from entrance to gate.', 'bad');
      if (!this.hasAirfieldRoute()) return this.ui.log('Plane cannot reach runway. Add runway, taxiway, and gate.', 'bad');
      if (this.powerUsed > this.powerMade) return this.ui.log('Power grid overloaded.', 'bad');
      if (this.waterUsed > this.waterMade) return this.ui.log('Water demand exceeds plumbing capacity.', 'bad');

      const pax = 34 + Math.floor(Math.random() * 28);
      const concessionIncome = this.objects.reduce((sum, object) => sum + (object.tool.income || 0), 0);
      const reward = 2200 + concessionIncome;
      this.passengers += pax;
      this.flights += 1;
      this.money += reward;
      this.ui.log(`Flight completed: ${pax} passengers, +$${reward.toLocaleString()}.`, 'good');
      this.recalculate();
      this.ui.updateStats(this);
      if (this.money < -5000) this.gameOver('Your airport ran out of money.');
    }

    recalculate() {
      this.powerUsed = this.objects.reduce((sum, object) => sum + (object.tool.power || 0), 0);
      this.powerMade = this.objects.reduce((sum, object) => sum + (object.tool.powerOut || 0), 0);
      this.waterUsed = this.objects.reduce((sum, object) => sum + (object.tool.water || 0), 0);
      this.waterMade = this.objects.reduce((sum, object) => sum + (object.tool.waterOut || 0), 0);
      const comfort = this.objects.reduce((sum, object) => sum + (object.tool.satisfaction || 0), 0);
      this.satisfaction = Math.max(25, Math.min(98, 72 + comfort - (this.powerUsed > this.powerMade ? 12 : 0) - (this.waterUsed > this.waterMade ? 8 : 0)));
      this.reputation = Math.min(5, 1 + this.flights * 0.1 + this.satisfaction / 100);
      if (this.passengers >= 100 && this.level < 2) this.level = 2;
      if (this.satisfaction >= 70 && this.level < 3) this.level = 3;
      if (this.passengers >= 1000 && this.level < 4) this.level = 4;
      if (this.reputation >= 4.2 && this.level < 5) this.level = 5;
    }

    hasPassengerRoute() {
      const has = (id) => this.objects.some((object) => object.tool.id === id);
      return has('entrance') && has('checkin') && has('security') && has('seat') && has('gateSmall');
    }

    hasAirfieldRoute() {
      const has = (id) => this.objects.some((object) => object.tool.id === id);
      return has('runwaySmall') && has('taxiway') && has('gateSmall');
    }

    hasAnyFloor() {
      return this.grid.some((row) => row.some((cell) => cell.floor));
    }

    footprint(x, y, w, h) {
      const cells = [];
      for (let yy = 0; yy < h; yy += 1) for (let xx = 0; xx < w; xx += 1) cells.push({ x: x + xx, y: y + yy });
      return cells;
    }

    inBounds(x, y) {
      return x >= 0 && y >= 0 && x < this.levelData.width && y < this.levelData.height;
    }

    neighbors(x, y) {
      return [{ x: x + 1, y }, { x: x - 1, y }, { x, y: y + 1 }, { x, y: y - 1 }].filter((cell) => this.inBounds(cell.x, cell.y));
    }

    screenToGrid(screenX, screenY) {
      const world = this.screenToWorld(screenX, screenY);
      const x = Math.floor(world.x / this.levelData.tileSize);
      const y = Math.floor(world.y / this.levelData.tileSize);
      if (!this.inBounds(x, y)) return null;
      return { x, y };
    }

    screenToWorld(screenX, screenY) {
      const cx = this.canvas.width / 2;
      const cy = this.canvas.height / 2;
      return {
        x: (screenX - cx) / this.camera.zoom + this.camera.x + this.levelData.width * this.levelData.tileSize / 2,
        y: (screenY - cy) / this.camera.zoom + this.camera.y + this.levelData.height * this.levelData.tileSize / 2
      };
    }

    worldToScreen(x, y) {
      const cx = this.canvas.width / 2;
      const cy = this.canvas.height / 2;
      return {
        x: (x - this.camera.x - this.levelData.width * this.levelData.tileSize / 2) * this.camera.zoom + cx,
        y: (y - this.camera.y - this.levelData.height * this.levelData.tileSize / 2) * this.camera.zoom + cy
      };
    }

    render() {
      const ctx = this.ctx;
      ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
      this.drawBackground(ctx);
      this.drawGrid(ctx);
      this.drawObjects(ctx);
      this.drawPreview(ctx);
      this.drawRouteStatus(ctx);
    }

    drawBackground(ctx) {
      const gradient = ctx.createLinearGradient(0, 0, 0, this.canvas.height);
      gradient.addColorStop(0, '#7bb88a');
      gradient.addColorStop(1, '#5d8b6f');
      ctx.fillStyle = gradient;
      ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);
    }

    drawGrid(ctx) {
      const tile = this.levelData.tileSize;
      for (let y = 0; y < this.levelData.height; y += 1) {
        for (let x = 0; x < this.levelData.width; x += 1) {
          const cell = this.grid[y][x];
          const point = this.worldToScreen(x * tile, y * tile);
          const size = tile * this.camera.zoom;
          ctx.fillStyle = cell.floor ? this.levelData.floors[cell.floor].color : 'rgba(72, 126, 78, 0.55)';
          ctx.fillRect(point.x, point.y, size - 1, size - 1);
          ctx.strokeStyle = 'rgba(255,255,255,0.12)';
          ctx.strokeRect(point.x, point.y, size, size);
        }
      }
    }

    drawObjects(ctx) {
      const tile = this.levelData.tileSize;
      this.objects.forEach((object) => {
        const point = this.worldToScreen(object.x * tile, object.y * tile);
        const w = object.tool.size[0] * tile * this.camera.zoom;
        const h = object.tool.size[1] * tile * this.camera.zoom;
        ctx.fillStyle = object.tool.color;
        ctx.fillRect(point.x + 2, point.y + 2, w - 4, h - 4);
        ctx.fillStyle = '#071326';
        ctx.font = `${Math.max(14, 20 * this.camera.zoom)}px sans-serif`;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(object.tool.icon || object.tool.name[0], point.x + w / 2, point.y + h / 2);
      });
    }

    drawPreview(ctx) {
      if (!this.hoverCell || this.state !== STATES.PLAYING) return;
      const tool = this.selectedTool();
      const validation = this.validatePlacement(tool, this.hoverCell.x, this.hoverCell.y);
      const tile = this.levelData.tileSize;
      validation.cells.forEach((cell) => {
        const point = this.worldToScreen(cell.x * tile, cell.y * tile);
        const size = tile * this.camera.zoom;
        ctx.fillStyle = validation.valid ? 'rgba(88, 227, 145, 0.42)' : 'rgba(255, 107, 138, 0.42)';
        ctx.fillRect(point.x, point.y, size, size);
        ctx.strokeStyle = validation.valid ? '#58e391' : '#ff6b8a';
        ctx.lineWidth = 2;
        ctx.strokeRect(point.x + 1, point.y + 1, size - 2, size - 2);
      });
    }

    drawRouteStatus(ctx) {
      ctx.save();
      ctx.fillStyle = 'rgba(7, 19, 38, 0.72)';
      ctx.fillRect(this.canvas.width - 330, 72, 310, 112);
      ctx.fillStyle = '#eef8ff';
      ctx.font = '16px sans-serif';
      ctx.textAlign = 'left';
      ctx.fillText(`Passenger route: ${this.hasPassengerRoute() ? 'Online' : 'Needs work'}`, this.canvas.width - 312, 104);
      ctx.fillText(`Airfield route: ${this.hasAirfieldRoute() ? 'Online' : 'Needs work'}`, this.canvas.width - 312, 132);
      ctx.fillText(`Power: ${this.powerUsed}/${this.powerMade}`, this.canvas.width - 312, 160);
      ctx.restore();
    }
  }

  window.SkyHubGame = { Game, STATES };
}());
