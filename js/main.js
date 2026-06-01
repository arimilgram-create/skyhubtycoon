(function () {
  'use strict';

  window.addEventListener('DOMContentLoaded', () => {
    try {
      const canvas = document.getElementById('gameCanvas');
      if (!canvas) throw new Error('Missing required canvas element #gameCanvas.');

      const ui = new window.SkyHubUI.UIController();
      const input = new window.SkyHubInput.InputManager(canvas);
      const game = new window.SkyHubGame.Game(canvas, input, ui, window.SkyHubSave);
      window.skyHubGame = game;
      game.init();
    } catch (error) {
      console.error('SkyHub Tycoon failed to start:', error);
      const loading = document.getElementById('loadingScreen');
      if (loading) {
        loading.classList.add('is-visible');
        loading.innerHTML = '<div class="menu-card"><h1>Startup Error</h1><p>Check the browser console for details.</p></div>';
      }
    }
  });
}());
