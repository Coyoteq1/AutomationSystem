# CrowbaneArena Mod

A V Rising mod for enhanced arena progression and V Blood management.

## Features

- God-Mode System: Full spellbook and hotbar access in arenas.
- Hook System: Intercepts V Blood checks for arena mode.
- Snapshot Service: Captures and restores progression.
- Config-driven weapons, armor sets, loadouts, and builds.

## Build

- Requires .NET 6 SDK.
- Open CrowbaneArena.csproj in your IDE or run `dotnet build` from this folder.
- The project references BepInEx.Core and HarmonyLib via NuGet. It will auto-reference V Rising assemblies if installed under `VRising_Server/VRisingServer_Data/Managed` relative to this project.

## Installation

- Copy the built CrowbaneArena.dll to your BepInEx plugins folder on the VRising server.
- Ensure `arena_config.json` and `appsettings.json` are deployed alongside if you plan to customize configs.

## Usage

- The plugin loads automatically via BepInEx. Harmony patches will log when arena entry/exit and V Blood checks are intercepted.
- Use in-game admin/arena commands when implemented; current build focuses on core services and logging.
