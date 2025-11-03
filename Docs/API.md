# AutomationSystem API Documentation

This document provides detailed API documentation for the AutomationSystem plugin components.

## Table of Contents
- [Core Services](#core-services)
- [Snapshot System](#snapshot-system)
- [Command System](#command-system)
- [Configuration](#configuration)
- [Data Models](#data-models)

## Core Services

### ICoreDataManager
Main interface for core data management operations.

```csharp
public interface ICoreDataManager
{
    void Initialize();
    void Shutdown();
    Task SaveDataAsync();
    Task LoadDataAsync();
}
```

### ISnapshotManager
Interface for managing player snapshots and state persistence.

```csharp
public interface ISnapshotManager
{
    void Initialize();
    void Shutdown();
    Task CreateSnapshotAsync(ulong steamId, PlayerSnapshot snapshot);
    Task<PlayerSnapshot?> GetLatestSnapshotAsync(ulong steamId);
    Task RestoreSnapshotAsync(ulong steamId, string snapshotId);
}
```

### IAutomationTracker
Interface for tracking automated processes and events.

```csharp
public interface IAutomationTracker
{
    void TrackEvent(string eventType, object data);
    void TrackCommand(string command, ulong steamId);
    IEnumerable<AutomationEvent> GetEvents(DateTime since);
}
```

## Snapshot System

### PlayerSnapshot
Represents a complete snapshot of player state.

```csharp
public class PlayerSnapshot
{
    public string SnapshotId { get; set; }
    public ulong SteamId { get; set; }
    public DateTime CreatedAt { get; set; }
    public PlayerData PlayerData { get; set; }
    public InventoryData Inventory { get; set; }
    public ProgressionData Progression { get; set; }
    public LocationData Location { get; set; }
}
```

### SnapshotManager
Main service for handling player snapshots.

```csharp
public class SnapshotManager : ISnapshotManager
{
    public Task CreateSnapshotAsync(ulong steamId, Entity playerEntity);
    public Task RestoreSnapshotAsync(ulong steamId, string snapshotId);
    public Task<List<PlayerSnapshot>> GetPlayerSnapshotsAsync(ulong steamId);
    public Task CleanupOldSnapshotsAsync(int maxAgeDays);
}
```

## Command System

### Command Groups

#### Enter Commands
```csharp
[CommandGroup("enter", "Arena enter commands")]
public static class EnterCommands
{
    [Command("pe", description: "Portal Execute - Auto-enter arena")]
    public static void PortalExecute(ChatCommandContext ctx);

    [Command("tp", description: "Teleport to arena exit zone")]
    public static void TeleportToArena(ChatCommandContext ctx);
}
```

#### Exit Commands
```csharp
[CommandGroup("exit", "Arena exit commands")]
public static class ExitCommands
{
    [Command("px", description: "Portal Exit - Restore progression")]
    public static void PortalExit(ChatCommandContext ctx);
}
```

#### Combat Commands
```csharp
[Command("lw", description: "Arena heal and buff reset")]
public static void LegendaryWarrior(ChatCommandContext ctx);

[Command("art", description: "Arena ultimate revive")]
public static void AncientRitualTechnique(ChatCommandContext ctx);
```

### Command Context
```csharp
public class ChatCommandContext
{
    public string Name { get; }          // Player name
    public bool IsAdmin { get; }         // Admin status
    public Entity SenderEntity { get; }  // Player entity
    public User SenderUserEntity { get; } // User entity

    public void Reply(string message);
    public void Error(string message);
}
```

## Configuration

### Arena Configuration
```json
{
  "ArenaEnterPoint": {
    "X": 1000.0,
    "Y": 20.0,
    "Z": 500.0
  },
  "ArenaExitPoint": {
    "X": 1500.0,
    "Y": 20.0,
    "Z": 800.0
  },
  "MaxPlayers": 8,
  "AutoProximityEnabled": true,
  "VBloodHookEnabled": true,
  "PersistenceIntervalSeconds": 60,
  "MaxBackups": 5
}
```

### Service Configuration
```csharp
services.AddAutomationSystem(options =>
{
    options.DataDirectory = "Data/AutomationSystem";
    options.AutoPersistenceEnabled = true;
    options.PersistenceIntervalSeconds = 60;
    options.BackupEnabled = true;
    options.MaxBackups = 5;
});
```

## Data Models

### Player Data
```csharp
public class Player
{
    public ulong SteamId { get; set; }
    public string Name { get; set; }
    public PlayerCharacter Character { get; set; }
    public Blood BloodType { get; set; }
    public Equipment Equipment { get; set; }
    public Inventory Inventory { get; set; }
}
```

### Arena Loadout
```csharp
public class ArenaLoadout
{
    public string Name { get; set; }
    public List<PrefabGUID> Weapons { get; set; }
    public List<PrefabGUID> Armor { get; set; }
    public List<PrefabGUID> Consumables { get; set; }
    public BloodType PreferredBloodType { get; set; }
}
```

### Weapon Model
```csharp
public class WeaponModel
{
    public PrefabGUID Guid { get; set; }
    public string Name { get; set; }
    public WeaponType Type { get; set; }
    public int Level { get; set; }
    public Dictionary<string, float> Stats { get; set; }
}
```

## Services

### Service Interfaces

#### IConfigurationService
```csharp
public interface IConfigurationService
{
    T GetConfig<T>(string key);
    void SetConfig<T>(string key, T value);
    void SaveConfig();
    void ReloadConfig();
}
```

#### IPlayerService
```csharp
public interface IPlayerService
{
    Entity GetPlayerEntity(string playerName);
    ulong GetSteamId(Entity playerEntity);
    string GetPlayerName(Entity playerEntity);
    float3 GetPlayerPosition(Entity playerEntity);
    bool IsPlayerInArena(ulong steamId);
}
```

#### IInventoryService
```csharp
public interface IInventoryService
{
    bool GiveItem(Entity playerEntity, PrefabGUID itemGuid, int quantity);
    bool RemoveItem(Entity playerEntity, PrefabGUID itemGuid, int quantity);
    bool HasItem(Entity playerEntity, PrefabGUID itemGuid, int quantity);
    List<InventoryItem> GetInventory(Entity playerEntity);
}
```

### Service Registration
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAutomationSystem(
        this IServiceCollection services,
        Action<AutomationOptions> configureOptions);
}
```

## Events and Hooks

### Automation Events
```csharp
public class AutomationEvent
{
    public string EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public object Data { get; set; }
    public ulong? SteamId { get; set; }
}
```

### Event Types
- `ArenaEntered` - Player entered arena
- `ArenaExited` - Player exited arena
- `SnapshotCreated` - Player snapshot created
- `SnapshotRestored` - Player snapshot restored
- `CommandExecuted` - Command executed
- `ItemGiven` - Item given to player

## Error Handling

### Exception Types
```csharp
public class AutomationException : Exception
{
    public string ErrorCode { get; }
    public object Context { get; }
}

public class SnapshotException : AutomationException
{
    // Snapshot-specific errors
}

public class CommandException : AutomationException
{
    // Command-specific errors
}
```

### Error Codes
- `SNAPSHOT_CREATE_FAILED` - Failed to create snapshot
- `SNAPSHOT_RESTORE_FAILED` - Failed to restore snapshot
- `COMMAND_EXECUTION_FAILED` - Command execution failed
- `CONFIG_LOAD_FAILED` - Configuration load failed
- `SERVICE_INITIALIZATION_FAILED` - Service initialization failed

## Performance Considerations

### Memory Management
- Use `Unity.Collections.Allocator.Temp` for temporary collections
- Dispose native collections properly
- Avoid boxing/unboxing in hot paths

### Async Operations
- Use `Task.Run` for CPU-bound operations
- Avoid blocking calls in main thread
- Implement proper cancellation tokens

### Caching
- Cache frequently accessed data
- Implement LRU cache for snapshots
- Use object pooling for reusable objects

## Security Considerations

### Admin Commands
- All admin commands require `adminOnly: true`
- Validate input parameters
- Log admin actions

### Data Validation
- Validate PrefabGUIDs before use
- Sanitize player names
- Check entity existence before operations

### Rate Limiting
- Implement command cooldowns
- Limit snapshot creation frequency
- Prevent abuse of admin commands
