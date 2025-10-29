# CrowbaneArena Mod - Project Structure

## Directory Organization

### Core Components
- **Root Files**: Main plugin entry points and core systems
  - `Plugin.cs` - BepInEx plugin initialization and Harmony patching
  - `Core.cs` - Core system initialization and service coordination
  - `VRising.cs` - V Rising game integration layer
  - `ArenaConfigLoader.cs` - Configuration management system

### Services Layer (`/Services/`)
Core business logic and system management:
- **Arena Management**: `ArenaService.cs`, `AutoEnterService.cs`, `TeleportService.cs`
- **Player Systems**: `PlayerService.cs`, `SessionService.cs`, `QueueService.cs`
- **Progression**: `SnapshotManagerService.cs`, `ProgressionCaptureService.cs`, `VBloodService.cs`
- **Equipment**: `EquipmentService.cs`, `InventoryManagementService.cs`, `WeaponVariantService.cs`
- **Infrastructure**: `SystemService.cs`, `LoggingService.cs`, `PatchService.cs`

### Data Models (`/Models/`, `/Data/`)
- **Configuration Models**: Arena settings, loadout definitions, player data structures
- **Game Data**: `VBloodGUIDs.cs`, `Loadouts.cs`, `Prefabs.cs`
- **Domain Models**: `ArenaLoadout.cs`, `PlayerData.cs`

### Patches (`/Patches/`)
Harmony runtime modifications:
- `ArenaPatches.cs` - Arena entry/exit behavior modifications
- `EquipmentPatches.cs` - Equipment and inventory system patches

### Configuration (`/config/`)
Runtime configuration files:
- `arena_config.json` - Arena zone and gameplay settings
- `arena_loadouts.json` - Predefined equipment loadouts
- `appsettings.json` - Application-level configuration

### Utilities (`/Utilities/`, `/Utils/`)
- **Helper Functions**: `Helper.cs`, `Utils.cs`
- **Converters**: `PrefabConverter.cs`, `ArenaConverters.cs`
- **Extensions**: `Extensions.cs`

### Build System
- **Project Configuration**: `CrowbaneArena.csproj` with V Rising and BepInEx dependencies
- **Build Outputs**: `/bin/` and `/obj/` directories with Debug/Release configurations
- **External Libraries**: `/libs/` containing V Rising and Unity assemblies

## Architectural Patterns

### Service-Oriented Architecture
- Clear separation of concerns with dedicated service classes
- Dependency injection pattern for service management
- Interface-based design for extensibility (`ISpawner.cs`)

### Configuration-Driven Design
- JSON-based configuration system with hot-reload capability
- Modular loadout and equipment management
- Environment-specific settings support

### Event-Driven System
- Arena entry/exit event handling
- Player state change notifications
- Progression snapshot triggers

### Plugin Integration
- BepInEx plugin lifecycle management
- Harmony patching for runtime modifications
- VampireCommandFramework integration for admin commands

## Component Relationships
- **Plugin** initializes **Core** which coordinates **Services**
- **Services** interact with **Models** and use **Utilities**
- **Patches** modify game behavior to trigger **Service** methods
- **Configuration** drives **Service** behavior and **Model** instantiation