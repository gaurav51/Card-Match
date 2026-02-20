# Antigravity Rules for Unity

## Project Overview
- **Project Name:** Card-Match
- **Engine:** Unity
- **Language:** C#

## Coding Standards
- Use PascalCase for Class names and Method names.
- Use camelCase for private fields (with optional `_` prefix).
- Ensure all Serialized fields are documented or have clear names.
- Follow Unity-specific optimization patterns (avoid `GetComponent` in `Update`, use `TryGetComponent`, etc.).

## Antigravity IDE Integration
- **Package:** `com.unity.ide.antigravity` (or relevant integration package).
- **External Editor:** Ensure Antigravity is selected in `Edit > Preferences > External Tools`.
- **Interactions:** The agent can modify scene objects, create prefabs, and write C# scripts.

## Card Match Specifics
- The game is a "Card Match" game. 
- Ensure card animations are smooth.
- Logic should be decoupled from UI where possible.
