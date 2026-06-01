(function () {
  'use strict';

  class PlayerCamera {
    constructor() {
      this.x = 0;
      this.y = 0;
      this.zoom = 1;
      this.rotation = 0;
      this.speed = 520;
    }

    update(input, dt) {
      const axis = input.axis();
      this.x += axis.x * this.speed * dt / this.zoom;
      this.y += axis.y * this.speed * dt / this.zoom;
      const wheel = input.consumeWheel();
      if (wheel !== 0) this.zoom = Math.max(0.65, Math.min(1.6, this.zoom - wheel * 0.08));
    }

    rotate() {
      this.rotation = (this.rotation + 1) % 4;
    }
  }

  window.SkyHubPlayer = { PlayerCamera };
}());
