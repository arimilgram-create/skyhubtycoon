(function () {
  'use strict';

  const KEY = 'skyhubTycoonSave';
  const DEFAULT_SAVE = {
    highScore: 0,
    unlockedItems: ['starter-airport'],
    settings: { sound: true, reducedMotion: false }
  };

  function load() {
    try {
      const raw = window.localStorage.getItem(KEY);
      if (!raw) return { ...DEFAULT_SAVE, settings: { ...DEFAULT_SAVE.settings } };
      return { ...DEFAULT_SAVE, ...JSON.parse(raw) };
    } catch (error) {
      console.warn('Save data could not be loaded. Starting with defaults.', error);
      return { ...DEFAULT_SAVE, settings: { ...DEFAULT_SAVE.settings } };
    }
  }

  function save(data) {
    try {
      window.localStorage.setItem(KEY, JSON.stringify({ ...DEFAULT_SAVE, ...data }));
    } catch (error) {
      console.warn('Save data could not be written.', error);
    }
  }

  function recordHighScore(passengers) {
    const data = load();
    if (passengers > data.highScore) {
      data.highScore = passengers;
      save(data);
    }
    return data.highScore;
  }

  window.SkyHubSave = { load, save, recordHighScore };
}());
