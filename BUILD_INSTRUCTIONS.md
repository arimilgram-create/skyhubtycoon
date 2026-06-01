# SkyHub Tycoon WebGL Build Instructions

Follow these exact steps to build the Unity project for a browser upload.

## 1. Open the project

1. Open **Unity Hub**.
2. Click **Add** or **Add project from disk**.
3. Select the root folder of this repository: `skyhubtycoon`.
4. Use **Unity 2022.3 LTS**. This project targets `2022.3.20f1`.
5. Open the project.
6. Wait for Unity to finish importing packages and compiling scripts.
7. The project automatically creates the main scene at `Assets/Scenes/MainScene.unity`.
8. If the scene is not created, run **Tools > SkyHub Tycoon > Create Full Starter Project**.

## 2. Test in the Unity Editor

1. Open `Assets/Scenes/MainScene.unity`.
2. Press **Play**.
3. On the start menu, click **Play**.
4. Controls:
   - **Left click**: place selected floor/object.
   - **WASD / Arrow keys**: pan the camera.
   - **Mouse wheel**: zoom.
   - **Q / E**: rotate the camera.
   - **Escape**: pause/resume.
5. Build a first airport flow:
   - Public floor
   - Entrance door on the edge
   - Check-in desk
   - Secure floor
   - Security checkpoint
   - Waiting or gate floor
   - Seating
   - Airfield pavement
   - Small runway
   - Taxiway
   - Gate floor
   - Small boarding gate
6. Click **Schedule next flight** once the passenger and airfield systems are online.

## 3. Build for WebGL

1. Go to **File > Build Settings**.
2. Select **WebGL**.
3. Click **Switch Platform**.
4. Confirm `Assets/Scenes/MainScene.unity` is included in **Scenes In Build** as scene index `0`.
5. Click **Build**.
6. Choose or create a folder named `WebGLBuild`.
7. Wait for Unity to finish building.

## 4. Upload to itch.io

1. Open your itch.io dashboard.
2. Create or edit a project.
3. Set **Kind of project** to **HTML**.
4. Zip the **contents** of the `WebGLBuild` folder, not the parent folder itself.
5. Upload that zip file to itch.io.
6. Check **This file will be played in the browser**.
7. Save and test the page.

## 5. Upload to GitHub Pages, Netlify, or your site

Upload the **contents** of `WebGLBuild` to your web host. The folder normally contains files like:

```text
index.html
Build/
TemplateData/
```

For GitHub Pages, put those files in the published branch/folder. For Netlify, drag the `WebGLBuild` folder into the deploy screen or connect a repository that publishes that folder.

## 6. Troubleshooting

- If you see a compression error on a web host, enable decompression fallback in Unity Player Settings or use a host configured for Brotli/Gzip. The bootstrap sets WebGL decompression fallback on by default.
- If UI looks too large or small, check the Canvas Scaler uses **Scale With Screen Size** and reference resolution **1920x1080**.
- If controls do not respond after clicking the page, click inside the WebGL canvas once to focus it.
