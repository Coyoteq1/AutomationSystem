# Configuration Guide

This guide covers all configuration options available in the AutomationSystem plugin.

## Table of Contents
- [Configuration Files](#configuration-files)
- [Arena Configuration](#arena-configuration)
- [Automation Settings](#automation-settings)
- [Loadout Configuration](#loadout-configuration)
- [Service Configuration](#service-configuration)
- [Advanced Settings](#advanced-settings)

## Configuration Files

The plugin uses several configuration files located in `BepInEx/config/AutomationSystem/`:

- `arena_config.json` - Main arena settings
- `arena_config.schema.json` - Configuration schema
- `VZoneConfig.json` - Zone-specific settings
- `ICB.core/arena_config.json` - Core arena configuration

## Arena Configuration

### Basic Arena Settings

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
  "ArenaSpawnPoint": {
    "X": 1200.0,
    "Y": 25.0,
    "Z": 600.0
  },
  "MaxPlayers": 8,
  "ArenaZoneRadius": 50.0,
  "EntryRadius": 10.0,
  "ExitRadius": 10.0
}
```

#### Field Descriptions

| Field | Type | Description | Default |
|-------|------|-------------|---------|
| `ArenaEnterPoint` | object | Coordinates where players enter arena | `{X:1000,Y:20,Z:500}` |
| `ArenaExitPoint` | object | Coordinates where players exit arena | `{X:1500,Y:20,Z:800}` |
| `ArenaSpawnPoint` | object | Spawn location within arena | `{X:1200,Y:25,Z:600}` |
| `MaxPlayers` | number | Maximum players allowed in arena | `8` |
| `ArenaZoneRadius` | number | Radius of arena zone in meters | `50.0` |
| `EntryRadius` | number | Radius of entry zone in meters | `10.0` |
| `ExitRadius` | number | Radius of exit zone in meters | `10.0` |

### Advanced Arena Settings

```json
{
  "AutoProximityEnabled": true,
  "VBloodHookEnabled": true,
  "AutoEnterEnabled": false,
  "CaveAutoEnterEnabled": false,
  "ProximityCheckInterval": 5.0,
  "MaxProximityDistance": 25.0,
  "ArenaNameTagEnabled": true,
  "ArenaNameTagPrefix": "[arena]",
  "ClearInventoryOnExit": true,
  "RestoreBloodTypeOnExit": true
}
```

#### Field Descriptions

| Field | Type | Description | Default |
|-------|------|-------------|---------|
| `AutoProximityEnabled` | boolean | Auto-detect players near arena | `true` |
| `VBloodHookEnabled` | boolean | Enable VBlood boss hooks | `true` |
| `AutoEnterEnabled` | boolean | Auto-enter players in range | `false` |
| `CaveAutoEnterEnabled` | boolean | Auto-enter from caves | `false` |
| `ProximityCheckInterval` | number | Seconds between proximity checks | `5.0` |
| `MaxProximityDistance` | number | Max distance for auto-enter | `25.0` |
| `ArenaNameTagEnabled` | boolean | Add [arena] to player names | `true` |
| `ArenaNameTagPrefix` | string | Prefix for arena players | `"[arena]"` |
| `ClearInventoryOnExit` | boolean | Clear inventory when exiting | `true` |
| `RestoreBloodTypeOnExit` | boolean | Restore blood type on exit | `true` |

## Automation Settings

### Persistence Configuration

```json
{
  "DataDirectory": "Data/AutomationSystem",
  "AutoPersistenceEnabled": true,
  "PersistenceIntervalSeconds": 60,
  "BackupEnabled": true,
  "MaxBackups": 5,
  "BackupIntervalHours": 24,
  "CompressSnapshots": true,
  "SnapshotRetentionDays": 30
}
```

#### Field Descriptions

| Field | Type | Description | Default |
|-------|------|-------------|---------|
| `DataDirectory` | string | Directory for plugin data | `"Data/AutomationSystem"` |
| `AutoPersistenceEnabled` | boolean | Enable automatic saving | `true` |
| `PersistenceIntervalSeconds` | number | Save interval in seconds | `60` |
| `BackupEnabled` | boolean | Enable backup creation | `true` |
| `MaxBackups` | number | Maximum backup files | `5` |
| `BackupIntervalHours` | number | Hours between backups | `24` |
| `CompressSnapshots` | boolean | Compress snapshot files | `true` |
| `SnapshotRetentionDays` | number | Days to keep snapshots | `30` |

### Performance Settings

```json
{
  "MaxConcurrentSnapshots": 3,
  "SnapshotQueueSize": 10,
  "MemoryCacheSize": 50,
  "FileBufferSize": 8192,
  "AsyncTimeoutSeconds": 30,
  "RetryAttempts": 3,
  "RetryDelaySeconds": 5
}
```

#### Field Descriptions

| Field | Type | Description | Default |
|-------|------|-------------|---------|
| `MaxConcurrentSnapshots` | number | Max simultaneous snapshots | `3` |
| `SnapshotQueueSize` | number | Max queued snapshots | `10` |
| `MemoryCacheSize` | number | Cached snapshots in memory | `50` |
| `FileBufferSize` | number | File I/O buffer size | `8192` |
| `AsyncTimeoutSeconds` | number | Timeout for async operations | `30` |
| `RetryAttempts` | number | Failed operation retries | `3` |
| `RetryDelaySeconds` | number | Delay between retries | `5` |

## Loadout Configuration

### Loadout Definitions

Loadouts are defined in JSON format with the following structure:

```json
{
  "loadouts": {
    "warrior": {
      "name": "Warrior Loadout",
      "weapons": [
        "-774462329",
        "-1569279652"
      ],
      "armor": [
        "-123456789"
      ],
      "consumables": [
        "123456789"
      ],
      "bloodType": "Warrior"
    },
    "mage": {
      "name": "Mage Loadout",
      "weapons": [
        "-774462329"
      ],
      "armor": [
        "-987654321"
      ],
      "consumables": [
        "987654321"
      ],
      "bloodType": "Mage"
    }
  }
}
```

### Build Presets

Build presets (1-4) are configured similarly:

```json
{
  "builds": {
    "build1": {
      "weapons": ["-774462329"],
      "armor": ["-123456789"],
      "description": "Basic Warrior Build"
    },
    "build2": {
      "weapons": ["-1569279652"],
      "armor": ["-987654321"],
      "description": "Advanced Mage Build"
    }
  }
}
```

## Service Configuration

### Dependency Injection Settings

```csharp
services.AddAutomationSystem(options =>
{
    options.DataDirectory = "Data/AutomationSystem";
    options.AutoPersistenceEnabled = true;
    options.PersistenceIntervalSeconds = 60;
    options.BackupEnabled = true;
    options.MaxBackups = 5;
    options.ServiceTimeoutSeconds = 30;
    options.EnableLogging = true;
    options.LogLevel = "Information";
});
```

### Service-Specific Settings

#### Snapshot Service
```json
{
  "SnapshotService": {
    "Enabled": true,
    "StorageType": "FileSystem",
    "CompressionLevel": "Optimal",
    "EncryptionEnabled": false,
    "ValidationEnabled": true
  }
}
```

#### Command Service
```json
{
  "CommandService": {
    "Enabled": true,
    "CommandPrefix": ".",
    "AdminPrefix": "!",
    "CooldownSeconds": 1,
    "RateLimitPerMinute": 60,
    "LogCommands": true
  }
}
```

#### Inventory Service
```json
{
  "InventoryService": {
    "Enabled": true,
    "MaxStackSize": 999,
    "AllowOverflow": false,
    "ValidateItems": true,
    "LogTransactions": true
  }
}
```

## Advanced Settings

### Territory Configuration

```json
{
  "territories": {
    "grid": {
      "size": 4,
      "points": {
        "1": { "X": 1000, "Y": 20, "Z": 500 },
        "2": { "X": 1100, "Y": 20, "Z": 500 },
        "3": { "X": 1100, "Y": 20, "Z": 600 },
        "4": { "X": 1000, "Y": 20, "Z": 600 }
      }
    }
  }
}
```

### VBlood Configuration

```json
{
  "vBloodConfig": {
    "unlockAllOnEnter": true,
    "lockAfterExit": false,
    "customUnlocks": {
      "Nicholaus": true,
      "Solarus": true,
      "Frost": false
    },
    "bossRespawnTimes": {
      "default": 1800,
      "custom": {
        "Nicholaus": 3600
      }
    }
  }
}
```

### Network Settings

```json
{
  "network": {
    "syncInterval": 1.0,
    "maxPacketSize": 4096,
    "compressionEnabled": true,
    "encryptionEnabled": false,
    "timeoutSeconds": 10,
    "retryCount": 3
  }
}
```

## Configuration Validation

The plugin includes configuration validation to ensure settings are correct:

### Validation Rules

- **Coordinates**: Must be valid float3 values
- **Radii**: Must be positive numbers
- **Intervals**: Must be positive numbers
- **GUIDs**: Must be valid PrefabGUID format
- **Paths**: Must be valid file system paths

### Schema Validation

Configuration files are validated against JSON schemas:

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "ArenaEnterPoint": {
      "type": "object",
      "properties": {
        "X": { "type": "number" },
        "Y": { "type": "number" },
        "Z": { "type": "number" }
      },
      "required": ["X", "Y", "Z"]
    }
  }
}
```

## Runtime Configuration Changes

### Reloading Configuration

Configuration can be reloaded without restarting the server:

```
/arena_reload
```

This will:
- Validate new configuration
- Apply changes to running services
- Log any validation errors
- Preserve existing snapshots

### Dynamic Settings

Some settings can be changed at runtime:

- Arena points (entry/exit/spawn)
- Persistence intervals
- Service timeouts
- Log levels

### Configuration Backup

The plugin automatically creates backups of configuration files:

- Daily backups in `config/backups/`
- Maximum 7 backup files
- Automatic cleanup of old backups

## Troubleshooting Configuration

### Common Issues

**Configuration not loading**:
- Check file permissions
- Validate JSON syntax
- Check file paths

**Settings not applying**:
- Use `/arena_reload` command
- Check server logs for errors
- Verify setting formats

**Invalid coordinates**:
- Use in-game coordinate commands
- Check for floating-point precision
- Validate against world boundaries

### Debug Commands

```bash
/arena_config_validate    # Validate configuration
/arena_config_dump        # Dump current config
/arena_config_backup      # Create manual backup
```

### Logging

Configuration operations are logged to:
- Server console
- `BepInEx/LogOutput.log`
- Plugin-specific logs in `Data/AutomationSystem/logs/`

## Best Practices

### Configuration Management

1. **Backup regularly**: Always backup before making changes
2. **Test changes**: Use a test server for configuration changes
3. **Version control**: Keep configuration files in version control
4. **Document changes**: Comment complex configurations

### Performance Tuning

1. **Adjust intervals**: Balance between responsiveness and performance
2. **Configure caching**: Set appropriate cache sizes for your server
3. **Monitor resources**: Watch memory usage with large configurations
4. **Optimize storage**: Use compression for large snapshot files

### Security

1. **Validate inputs**: Ensure all configuration values are validated
2. **Restrict access**: Limit who can modify configuration files
3. **Audit changes**: Log all configuration modifications
4. **Secure backups**: Protect backup files from unauthorized access
