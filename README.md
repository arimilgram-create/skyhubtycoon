# SkyHub Tycoon

SkyHub Tycoon is a static GitHub Pages browser game. It runs directly from the root `index.html` file with plain HTML, CSS, and JavaScript. No Unity, backend server, build step, database, or Node.js runtime is required for the published game.

## Controls

- **Left click**: place the selected floor or airport object.
- **WASD / Arrow keys**: pan around the airport.
- **Mouse wheel**: zoom in and out.
- **Space**: schedule the next flight.
- **E**: rotate the camera state.
- **Escape**: pause or resume.

## How to play

1. Click **Play** on the main menu.
2. Paint public floors, then place an entrance.
3. Add check-in, secure floor, security, seats, airfield pavement, runway, taxiway, and a small gate.
4. Press **Space** or use the flight action to schedule flights once passenger and airfield routes are online.
5. Earn money from flights and shops, improve satisfaction, unlock levels, and expand into baggage, restaurants, and international processing.

## File structure

```text
my-game/
  index.html          Main GitHub Pages entry file
  README.md           This guide
  .gitignore          Git ignore rules
  assets/
    images/           Placeholder folder for future image assets
    audio/            Placeholder folder for future audio assets
    fonts/            Placeholder folder for future font assets
  css/
    style.css         Responsive page, canvas, HUD, and menu styling
  js/
    main.js           Startup and error handling
    game.js           Main game state, update loop, rendering, and airport rules
    player.js         Camera/player movement controller
    input.js          Keyboard and mouse input
    ui.js             Menus, HUD, buttons, and messages
    levels.js         Level, tool, floor, and grid data
    save.js           localStorage save/high-score helpers
```

The asset folders currently contain `.gitkeep` placeholders. Add future files with relative paths such as `assets/images/player.png` or `assets/audio/click.wav`.

## Run locally

Open `index.html` directly in a browser, or use any simple static server if you prefer. The game uses relative paths only, so it works from the file system and from GitHub Pages.

## Upload to GitHub

1. Create a new GitHub repository.
2. Copy this project into the repository root.
3. Commit all files.
4. Push to the `main` branch:

```bash
git add .
git commit -m "Add SkyHub Tycoon browser game"
git branch -M main
git remote add origin https://github.com/USERNAME/REPO-NAME.git
git push -u origin main
```

## Enable GitHub Pages

1. Go to the repository on GitHub.
2. Open **Settings**.
3. Click **Pages**.
4. Set **Source** to **Deploy from a branch**.
5. Set **Branch** to `main`.
6. Set **Folder** to `/root`.
7. Click **Save**.

The game will be playable at:

```text
https://USERNAME.github.io/REPO-NAME/
```

## Troubleshooting

- If the page is blank, open the browser developer tools and check the Console.
- Make sure `index.html` is in the repository root.
- Make sure all paths are relative and do not start with `/`.
- Make sure GitHub Pages is set to branch `main` and folder `/root`.
- If controls do not work, click the game canvas once so the browser focuses the page.
