(function () {
  'use strict';

  class InputManager {
    constructor(canvas) {
      this.canvas = canvas;
      this.keys = new Set();
      this.mouse = { x: 0, y: 0, down: false, clicked: false, wheel: 0 };
      this.onPause = null;
      this.onInteract = null;
      this.onRotate = null;
      this.preventedKeys = new Set([' ', 'ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight']);
      this.bindEvents();
    }

    bindEvents() {
      window.addEventListener('keydown', (event) => {
        if (this.preventedKeys.has(event.key)) event.preventDefault();
        this.keys.add(event.key.toLowerCase());
        if (event.key === 'Escape' && this.onPause) this.onPause();
        if ((event.key === ' ' || event.key.toLowerCase() === 'spacebar') && this.onInteract) this.onInteract();
        if (event.key.toLowerCase() === 'e' && this.onRotate) this.onRotate();
      }, { passive: false });

      window.addEventListener('keyup', (event) => {
        if (this.preventedKeys.has(event.key)) event.preventDefault();
        this.keys.delete(event.key.toLowerCase());
      }, { passive: false });

      this.canvas.addEventListener('mousemove', (event) => this.updateMousePosition(event));
      this.canvas.addEventListener('mousedown', (event) => {
        this.updateMousePosition(event);
        this.mouse.down = true;
        this.mouse.clicked = true;
      });
      window.addEventListener('mouseup', () => { this.mouse.down = false; });
      this.canvas.addEventListener('wheel', (event) => {
        event.preventDefault();
        this.mouse.wheel += Math.sign(event.deltaY);
      }, { passive: false });
    }

    updateMousePosition(event) {
      const rect = this.canvas.getBoundingClientRect();
      this.mouse.x = (event.clientX - rect.left) * (this.canvas.width / rect.width);
      this.mouse.y = (event.clientY - rect.top) * (this.canvas.height / rect.height);
    }

    axis() {
      const left = this.keys.has('a') || this.keys.has('arrowleft');
      const right = this.keys.has('d') || this.keys.has('arrowright');
      const up = this.keys.has('w') || this.keys.has('arrowup');
      const down = this.keys.has('s') || this.keys.has('arrowdown');
      return { x: (right ? 1 : 0) - (left ? 1 : 0), y: (down ? 1 : 0) - (up ? 1 : 0) };
    }

    consumeClick() {
      const clicked = this.mouse.clicked;
      this.mouse.clicked = false;
      return clicked;
    }

    consumeWheel() {
      const wheel = this.mouse.wheel;
      this.mouse.wheel = 0;
      return wheel;
    }
  }

  window.SkyHubInput = { InputManager };
}());
