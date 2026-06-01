(function () {
  'use strict';

  class UIController {
    constructor() {
      this.loadingScreen = document.getElementById('loadingScreen');
      this.mainMenu = document.getElementById('mainMenu');
      this.pauseMenu = document.getElementById('pauseMenu');
      this.gameOverScreen = document.getElementById('gameOverScreen');
      this.hud = document.getElementById('hud');
      this.buildMenu = document.getElementById('buildMenu');
      this.messageLog = document.getElementById('messageLog');
      this.stats = {
        money: document.getElementById('moneyText'),
        satisfaction: document.getElementById('satisfactionText'),
        reputation: document.getElementById('reputationText'),
        flights: document.getElementById('flightsText'),
        level: document.getElementById('levelText'),
        highScore: document.getElementById('highScoreText'),
        gameOverReason: document.getElementById('gameOverReason')
      };
    }

    bindButtons(game) {
      document.getElementById('playButton').addEventListener('click', () => game.startNewGame());
      document.getElementById('continueButton').addEventListener('click', () => game.startNewGame());
      document.getElementById('resumeButton').addEventListener('click', () => game.resume());
      document.getElementById('restartButton').addEventListener('click', () => game.startNewGame());
      document.getElementById('mainMenuButton').addEventListener('click', () => game.showMenu());
      document.getElementById('gameOverRestartButton').addEventListener('click', () => game.startNewGame());
      document.getElementById('gameOverMenuButton').addEventListener('click', () => game.showMenu());
    }

    buildTools(tools, activeToolId, onSelect) {
      this.buildMenu.innerHTML = '';
      tools.forEach((tool) => {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = `tool-button${tool.id === activeToolId ? ' is-active' : ''}`;
        button.title = `${tool.name} - $${tool.cost || 0}`;
        button.innerHTML = `<span class="tool-button__swatch" style="background:${tool.color || '#8fc8ff'}"></span><span>${tool.icon || '■'} ${tool.name}</span>`;
        button.addEventListener('click', () => onSelect(tool.id));
        this.buildMenu.appendChild(button);
      });
    }

    showState(state) {
      this.loadingScreen.classList.toggle('is-visible', state === 'loading');
      this.mainMenu.classList.toggle('is-visible', state === 'menu');
      this.pauseMenu.classList.toggle('is-visible', state === 'paused');
      this.gameOverScreen.classList.toggle('is-visible', state === 'gameOver');
      this.hud.classList.toggle('is-hidden', state !== 'playing' && state !== 'paused');
    }

    updateStats(game) {
      this.stats.money.textContent = `$${Math.round(game.money).toLocaleString()}`;
      this.stats.satisfaction.textContent = `${Math.round(game.satisfaction)}%`;
      this.stats.reputation.textContent = `★ ${game.reputation.toFixed(1)}`;
      this.stats.flights.textContent = String(game.flights);
      this.stats.level.textContent = String(game.level);
    }

    showGameOver(reason, highScore) {
      this.stats.gameOverReason.textContent = reason;
      this.stats.highScore.textContent = String(highScore);
    }

    log(message, type = '') {
      const line = document.createElement('p');
      line.className = type;
      line.textContent = message;
      this.messageLog.prepend(line);
      while (this.messageLog.children.length > 8) this.messageLog.lastChild.remove();
    }
  }

  window.SkyHubUI = { UIController };
}());
