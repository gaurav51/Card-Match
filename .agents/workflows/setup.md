---
description: How to complete the Antigravity setup in Unity
---

# Antigravity Unity Setup Workflow

To finish setting up Antigravity for this Unity project:

1. **Wait for Package Compilation**: Unity should automatically detect the change I made to `Packages/manifest.json` and download the Antigravity integration package.
2. **Open Preferences**:
   - In Unity, go to **Edit > Preferences** (Windows) or **Unity > Settings** (macOS).
3. **Select External Tools**:
   - Click on the **External Tools** tab on the left.
4. **Set External Script Editor**:
   - In the **External Script Editor** dropdown, select **Antigravity**.
   - If it's not listed, click **Browse...** and locate your Antigravity installation.
5. **Regenerate Project Files**:
   - Click the **Regenerate project files** button in the same External Tools menu.
6. **Connect Console**:
   - Once set up, Unity's console and logs will be mirrored in the Antigravity IDE, allowing for better AI assistance.

// turbo
7. **Verify Installation**:
   - Check if `Assets/Open C# Project` now opens Antigravity.
