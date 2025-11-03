# Crowbane Arena Project Overview

## Project Summary

Crowbane Arena is a VRising server plugin designed for PvP arena combat with VBlood progression simulation. It enables players to test builds with all abilities unlocked in controlled environments. The system provides a robust foundation for arena gameplay, featuring VBlood hook systems, zone management, player state snapshotting, and command-based controls, all built with extensibility and maintainability in mind.

### Core Purpose
- Deliver competitive PvP arena combat inside VRising without risking long-term progression.
- Provide a sandbox for testing builds with every VBlood power temporarily unlocked.
- Offer administrators fine-grained control over arena sessions, loadouts, and player flow.

### Architecture & Components

#### Core Systems
- **ArenaSystem**: Coordinates match flow, participant tracking, and hooks into `SnapshotManagerService` for lifecycle events.
- **ZoneManager**: Maintains arena center, entry, exit, and spawn radii; exposes setters used by commands and proximity checks.
- **SnapshotManagerService / ProgressionCaptureService**: Capture inventories, equipment, abilities, blood state, and restore them on exit.
- **ProgressManagerService**: Persists arena participation state, handles archive/restore workflow, and drives VBlood hook activation.
- **CommandRegistry**: Ensures all VCF command groups are surfaced and provides discovery logging during plugin load.

#### Key Features
- Harmony-powered VBlood hook system that fakes unlock checks while the player is inside the arena.
- Configurable arena zones with optional proximity-based entry/exit enforced by `ArenaProximitySystem`.
- Complete snapshot/restore pipeline so players leave with exactly the gear and unlocks they had before entry.
- Rich command surface for day-to-day arena operations plus portal/waygate utilities.
- Debug-time smoke tests (e.g., `ZoneManagerTests`) to verify configuration changes.

### Lifecycle Management

#### Initialization
`Plugin.Load()` orchestrates startup:
1. Initialize foundational services via `CrowbaneArenaCore.Initialize()` and `VRisingCore.Initialize()`.
2. Spin up ECS tracking (`ArenaPlayerTrackerSystem`) and hook `ArenaUpdateBehaviour` for periodic maintenance.
3. Discover and log VCF command groups (Arena, Portal, Waygate) to ensure assembly loading.
4. Load arena configuration, initialize `ArenaTerritory`, and start `PlayerTracker` for world-aware state.

#### Player Lifecycle

**Entering Arena** (e.g., `.arena join [arenaName]` or proximity trigger)
1. Validate player state (alive, not already flagged in arena, allowed to enter).
2. Capture a snapshot of inventory, equipment, abilities, blood, unlocks, and position.
3. Persist snapshot and activate the VBlood hook via `GameSystems.MarkPlayerEnteredArena`.
4. Clear inventory, apply arena loadout/blood, optionally rename, and teleport using `TeleportService`.
5. Flag arena timers/state in `ProgressManagerService` for later restoration.

**In-Arena**
- All VBlood abilities appear available thanks to Harmony patches around unlock checks.
- `ArenaPlayerTrackerSystem` monitors position; optional `ArenaProximitySystem` enforces radius bounds.
- `ArenaUpdateBehaviour` keeps maintenance tasks (e.g., zone checks) running on a fixed cadence.

**Exiting Arena** (e.g., `.arena leave` or radius exit)
1. Clear arena-specific effects and stop timers.
2. Restore inventory, equipment, abilities, blood, and name from the stored snapshot.
3. Deactivate the VBlood hook with `GameSystems.MarkPlayerExitedArena` and clean snapshot records.
4. Teleport back to the captured location and resume normal gameplay state.

### Command System
Commands are handled via VampireCommandFramework with the following structure:

```csharp
[CommandGroup("arena", "a")]
internal class ArenaCommands
{
    [Command("join", "j", description: "Join an arena")]
    public static void JoinArena(ChatCommandContext ctx, string arenaName = "default") { ... }

    [Command("leave", "l", description: "Leave current arena")]
    public static void LeaveArena(ChatCommandContext ctx) { ... }

    [Command("status", "s", description: "Check arena status")]
    public static void ArenaStatus(ChatCommandContext ctx) { ... }
}
```

### Testing Framework
Includes comprehensive tests for zone management and configuration services:

```csharp
public static class ZoneManagerTests
{
    public static void RunBasicTests()
    {
        TestSetArenaZone();
        TestSetEntryPoint();
        TestSetExitPoint();
        TestSetSpawnPoint();
    }
}
```

### Configuration
- Zone Settings: Arena center, radius, entry/exit points, spawn points
- Gameplay Settings: Default loadouts, ability configurations, match parameters

### Dependencies
- BepInEx: Plugin framework
- VampireCommandFramework: Command system
- VRising SDK: Game integration
- Unity ECS: Entity management

### Build & Deployment
**Build Process**: `dotnet build`

**Installation**:
- Copy CrowbaneArena.dll to BepInEx/plugins/
- Configure settings in BepInEx/config/

**Troubleshooting**:
- Check BepInEx/LogOutput.log for errors
- Verify VCF compatibility
- Ensure proper game version

### Best Practices
- **State Management**: Always capture complete player state before arena entry
- **Performance**: Optimize ECS queries, cache data, use efficient structures
- **Extensibility**: Design for modularity, use interfaces, document APIs

### Future Enhancements
- Tournament support
- Spectator mode
- Custom scoring systems
- Team-based arenas
- Async operations and better memory management

## Repository Structure (Detailed)

### Entry & Bootstrapping
- **Plugin.cs**: BepInEx entry point. Initializes core systems, ECS, commands, and services.
- Registers ArenaUpdateBehaviour for per-frame checks.

### Command Surface (Commands/)
- **ArenaCommands.cs**: Core arena controls (join/leave/status/create/delete/start/stop/setspawn)
- **PortalCommands.cs / PortalExecuteCommands.cs / ArenaPortalCommands.cs**: Portal management
- **WaygateCommands.cs / MapIconCommands.cs**: Waygate and map utilities
- **ArenaJoinCommands.cs**: Specialized entry/exit flows
- **ArenaResearchCommands.cs, UnlockCommands.cs**: Progression configuration
- **Converters/**: Custom parameter converters for bosses, items, etc.

### Services Layer (Services/)
- **CommandRegistry.cs**: Command registration and logging
- **ArenaManagerService.cs, ZoneManagerService.cs**: Arena/zone orchestration
- **SnapshotManagerService.cs & ProgressionCaptureService.cs**: Player state lifecycle
- **ProgressManagerService.cs**: Unlock state and ability restoration
- **AutoEnterService.cs / ArenaProximitySystem**: Auto entry/exit
- **GameSystems.cs**: VBlood hook toggling
- **PlayerTracker.cs / SystemService.cs / VectorService.cs**: Entity lookups and utilities
- **InventoryManagementService.cs, EquipmentService.cs, AbilityService.cs**: Loadouts and setups
- **TeleportService.cs, ArenaZoneService.cs, ArenaUpdateService.cs**: Movement and updates
- **RequestServices/**: Sub-APIs for commands

### Core Logic & Data
- **ArenaSystem.cs / ArenaController.cs**: Game loop, entry/exit orchestration
- **ArenaTerritory.cs / ArenaBounds.cs**: Territory grid and boundary checks
- **ZoneManager.cs**: Arena center/radii/entry/exit/spawn management
- **ArenaEvent.cs, Build.cs, ArmorSet.cs, BloodType.cs, BossReference.cs, Models.cs**: Data models
- **Constants.cs, ConfigManager.cs, ConfigHandler.cs, ArenaConfigLoader.cs**: Settings and reload support
- **ArenaConverters.cs, PrefabConverter.cs**: Config to prefab mappings

### Patching/Hooks
- **ArenaDeathPatch.cs, BuildAnywherePatch.cs, VBloodHook components**: Harmony patches for death, unlocks, building

### Components & ECS (Components/, Systems/)
- Tag components (e.g., ShipTag)
- **ArenaPlayerTrackerSystem**: Per-frame player monitoring

### Tests (Tests/)
- **ZoneManagerTests.cs**: Zone functionality validation
- **ConfigServiceTests.cs, ArenaServiceTests.cs**: Configuration and arena smoke checks

### Utilities
- **Extensions.cs**: Extension methods and helpers

### Lifecycle (Aligned with Arena Lifecycle Implementation)

#### Initialization (Server Start)
- Plugin.Load() → Core initializations → ECS systems → Command discovery → Zone configs

#### Arena Entry
- Validate player → Snapshot capture → VBlood hook activation → Teleport → Apply loadout → Start tracking

#### In-Arena Maintenance
- Position monitoring → Boundary enforcement → Ability UI updates

#### Arena Exit
- Validate presence → Clear effects → Restore snapshot → Deactivate hook → Teleport out → Cleanup

### Command Surface Summary
| Command | Alias | Purpose |
|---------|-------|---------|
| .arena test | .arena t | Validate VCF wiring |
| .arena enter [build] | .arena ent | Manual entry with optional build |
| .arena exit | .arena ext | Leave arena |
| .arena status | .arena s | View arena state |
| .arena create name [max] | .arena c | Admin create arena |
| .arena delete name | .arena d | Remove arena |
| .arena start/stop [name] | .arena s/x | Admin match control |
| .arena setspawn [type] | .arena ss | Record spawn point |

Additional portal/waygate commands available.

### Testing & Diagnostics
- ZoneManagerTests.RunBasicTests() in DEBUG
- Config reload support
- Centralized logging via Plugin.Logger

### Build & Deployment
- `dotnet build` (with XML doc and nullable warnings)
- Output DLL to BepInEx/plugins/
- Ensure VRising assemblies available

### Notable Warnings/Considerations
- XML comment warnings (CS1591)
- Nullable warnings (CS86xx)
- BepInEx inheritance warning (benign)
- MSB3245 warnings for missing assemblies

## Comprehensive File Listing

Below is a complete list of all files in the Crowbane Arena project, organized by directory:

### Root Directory
- BloodType.cs
- BossReference.cs
- Build.cs
- BuildAnywherePatch.cs
- BuildAnywherePatch.cs.disabled
- CommandManager.cs
- Commands.cs
- Commands.cs.old
- ConfigHandler.cs
- ConfigManager.cs
- Constants.cs
- Consumable.cs
- Core.cs
- CrowbaneArena.csproj
- CrowbaneArena.dll.config
- DataHandler.cs
- desktop.ini
- Directory.Build.props
- ECSExtensions.cs
- ErrorHandler.cs
- FodyWeavers.xml
- FodyWeavers.xsd
- Folder.DotSettings.user
- GodModeSystem.cs
- HookDOTS.API.dll
- ItemSpawner.cs
- JSON_MIGRATION_SUMMARY.md
- Loadout.cs
- Models.cs
- Module1.cs
- Module2.cs
- NuGet.config
- Patch.cs
- PlayerManager.cs
- PlayerTracker.cs
- Plugin.cs
- Prefabsnames
- README.md

### .idea/
- (Directory for IDE settings)

### .qodo/
- (Directory for project tools)

### .run/
- (Directory for run configurations)

### .tmp.driveupload/
- (Temporary directory)

### .vscode/
- (Directory for VS Code settings)

### Abilities/
- (Directory for ability-related files)

### Attributes/
- (Directory for attribute-related files)

### bin/
- (Build output directory)

### Commands/
- ArenaCommands.cs
- ArenaJoinCommands.cs
- ArenaLoadoutCommands.cs.disabled
- ArenaPortalCommands.cs
- ArenaResearchCommands.cs
- MapIconCommands.cs
- PortalCommands.cs
- PortalExecuteCommands.cs
- UnlockCommands.cs
- WaygateCommands.cs

### Components/
- ShipTag.cs

### ComponentSaver/
- (Directory for component saving)

### config/
- arena_config.json

### Core/
- ISystemWork.cs
- SystemContext.cs
- VSystemBase.cs

### CrowbaneArena_Release/
- CrowbaneArena.dll
- README.md

### Data/
- VBloodGUIDs.cs
- Shared/
  - InfuseSpellMod.cs
  - SpellSchool.cs
  - SpellSchoolGUIDs.cs
  - StatMod.cs
  - WeaponInfo.cs

### Extensions/
- EntityExtensions.cs

### Helpers/
- EffectsHelper.cs

### libs/
- .version
- 0Harmony.dll
- AsmResolver.dll
- AsmResolver.DotNet.dll
- AsmResolver.PE.dll
- AsmResolver.PE.File.dll
- AssetRipper.CIL.dll
- AssetRipper.Primitives.dll
- Backtrace.Unity.dll
- BepInEx.Core.dll
- BepInEx.Preloader.Core.dll
- BepInEx.Unity.Common.dll
- BepInEx.Unity.IL2CPP.dll
- clretwrc (1).dll
- clretwrc.dll
- clrjit.dll
- com.rlabrecque.steamworks.net.dll
- com.stunlock.network.eos.dll
- com.stunlock.platform.dll
- com.stunlock.platform.pc.dll
- coreclr.dll
- Cpp2IL.Core.dll
- dbgshim.dll
- Disarm.dll
- dobby.dll
- Gee.External.Capstone.dll
- HookDOTS.API.dll
- hostpolicy (1).dll
- hostpolicy.dll
- Iced.dll
- Il2CppInterop.Common.dll
- Il2CppInterop.Generator.dll
- Il2CppInterop.HarmonySupport.dll
- Il2CppInterop.Runtime.dll
- Il2CppMono.Security.dll
- Il2CppSystem.Drawing.dll
- Il2CppSystem.Net.Http.dll
- Il2CppSystem.Runtime.CompilerServices.Unsafe.dll
- Il2CppSystem.Runtime.Serialization.dll
- Il2CppSystem.Xml.dll
- Il2CppSystem.Xml.Linq.dll
- KindredSchematics.dll
- LibCpp2IL.dll
- Lidgren.Network.dll
- MagicaCloth.dll
- Malee.ReorderableList.dll
- Microsoft (1).Bcl.AsyncInterfaces.dll
- Microsoft (1).Extensions.Options.dll
- Microsoft (1).Win32.Primitives.dll
- Microsoft.Bcl.AsyncInterfaces.dll
- Microsoft.CSharp.dll
- Microsoft.DiaSymReader.Native.amd64.dll
- Microsoft.Extensions (1).DependencyInjection.dll
- Microsoft.Extensions.DependencyInjection (1).Abstractions.dll
- Microsoft.Extensions.DependencyInjection.Abstractions.dll
- Microsoft.Extensions.DependencyInjection.dll
- Microsoft.Extensions.Logging.Abstractions.dll
- Microsoft.Extensions.Logging.dll
- Microsoft.Extensions.Options.dll
- Microsoft.Extensions.Primitives.dll
- Microsoft.NETCore.App.deps.json

### Models/
- ArenaLoadout.cs
- Player.cs
- PlayerData.cs
- PlayerSnapshot.cs

### obj/
- CrowbaneArena.csproj.nuget.dgspec.json
- CrowbaneArena.csproj.nuget.g.props
- CrowbaneArena.csproj.nuget.g.targets
- project.assets.json
- project.nuget.cache
- project.packagespec.json
- rider.project.model.nuget.info
- rider.project.restore.info
- Debug/net6.0/.NETCoreApp,Version=v6.0.AssemblyAttributes.cs
- Debug/net6.0/Crowbane (1).D03F144F.Up2Date
- Debug/net6.0/Crowbane.D03F144F.Up2Date
- Debug/net6.0/CrowbaneArena (1).AssemblyInfo.cs
- Debug/net6.0/CrowbaneArena (1).AssemblyInfoInputs.cache
- Debug/net6.0/CrowbaneArena (1).csproj.AssemblyReference.cache
- Debug/net6.0/CrowbaneArena (1).csproj.CoreCompileInputs.cache
- Debug/net6.0/CrowbaneArena (1).dll
- Debug/net6.0/CrowbaneArena (1).pdb
- Debug/net6.0/CrowbaneArena (1).sourcelink.json
- Debug/net6.0/CrowbaneArena (1).xml
- Debug/net6.0/CrowbaneArena (2).AssemblyInfo.cs
- Debug/net6.0/CrowbaneArena (2).AssemblyInfoInputs.cache
- Debug/net6.0/CrowbaneArena (2).csproj.AssemblyReference.cache
- Debug/net6.0/CrowbaneArena (2).csproj.CoreCompileInputs.cache
- Debug/net6.0/CrowbaneArena (2).sourcelink.json
- Debug/net6.0/CrowbaneArena.AssemblyInfo.cs
- Debug/net6.0/CrowbaneArena.GeneratedMSBuildEditorConfig (1).editorconfig
- Debug/net6.0/CrowbaneArena.GeneratedMSBuildEditorConfig (2).editorconfig
- Debug/net6.0/CrowbaneArena.GeneratedMSBuildEditorConfig.editorconfig
- Debug/net6.0/CrowbaneArena.GlobalUsings.g.cs
- Debug/net6.0/CrowbaneArena.pdb
- Debug/net6.0/CrowbaneArena.sourcelink.json
- Debug/net6.0/CrowbaneArena.xml
- Debug/net6.0/PluginInfo (1).cs

### Patches/
- BloodMendPatch.cs
- EquipmentPatches.cs
- MapIconSpawnSystemPatch.cs
- PvPHookPatch.cs
- RegisterSpawnedChunkObjectsSystemPatch.cs
- RegisterSpawnedChunkObjectsSystemSpawnPatch.cs
- TeleportationRequestSystemPatch.cs
- VBloodHookPatch.cs

### Scripts/
- DebugTools.cs

### server/
- logs/usage_tracking.log

### Services/
- ArenaCheatService.cs
- ArenaPortalManager.cs
- ArenaZoneService.cs.disabled
- AutoEnterService.cs
- CastleSpawnService.cs
- CaveAutoEnterService.cs
- CommandRegistry.cs
- ConfigService.cs
- DefaultSpawner.cs
- EquipmentService.cs
- GameSystems.cs
- InventoryManagementService.cs
- ISpawner.cs
- ItemSpawnService.cs
- LocalizationService.cs
- LoggingService.cs
- MapIconService.cs
- PatchService.cs
- PlayerService.cs
- PlayerService.cs.new
- PortalService.cs
- PrefabRemapService.cs
- ProgressionCaptureService.cs
- ProgressManagerService.cs
- QueueService.cs
- SessionService.cs
- SnapshotManagerService.cs
- SnapshotService.cs
- SpawnService.cs
- SystemService.cs
- TeleportService.cs
- VBloodService.cs

### Systems/
- (Directory for ECS systems)

### Tests/
- (Directory for test files)

### UI/
- (Directory for UI-related files)

### Utilities/
- (Directory for utility files)

### Utils/
- (Directory for additional utilities)

This documentation provides a comprehensive overview of the Crowbane Arena project, combining detailed explanations of its architecture, lifecycle, and components with a complete file inventory for easy reference and navigation.
