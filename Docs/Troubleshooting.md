# Troubleshooting Guide

Common issues and solutions for the AutomationSystem plugin.

## Table of Contents
- [Installation Issues](#installation-issues)
- [Plugin Loading Problems](#plugin-loading-problems)
- [Command Issues](#command-issues)
- [Arena System Problems](#arena-system-problems)
- [Snapshot Issues](#snapshot-issues)
- [Performance Issues](#performance-issues)
- [Configuration Problems](#configuration-problems)
- [Debug Tools](#debug-tools)

## Installation Issues

### Plugin Not Loading

**Symptoms**:
- Plugin doesn't appear in server logs
- Commands not available in-game
- No AutomationSystem folder in BepInEx/plugins

**Solutions**:

1. **Check BepInEx Installation**:
   ```bash
   # Verify BepInEx files exist
   ls BepInEx/core/
   ls BepInEx/plugins/
   ```

2. **Verify Plugin Location**:
   ```
   BepInEx/plugins/AutomationSystem/AutomationSystem.dll
   ```

3. **Check Dependencies**:
   - Ensure .NET 8.0 runtime is installed
   - Verify BepInEx version compatibility
   - Check for VampireCommandFramework

4. **Server Logs**:
   ```
   tail -f BepInEx/LogOutput.log
   ```

### Missing Dependencies

**Error**: `Could not load file or assembly`

**Solutions**:

1. **Install Required Runtimes**:
   - Download .NET 8.0 Desktop Runtime
   - Install VC++ Redistributables

2. **Check Referenced Assemblies**:
   ```xml
   <!-- Verify these exist in project -->
   <Reference Include="ProjectM.Shared">
     <HintPath>C:\Program Files (x86)\Steam\steamapps\common\VRising\VRising_Server\BepInEx\interop\ProjectM.Shared.dll</HintPath>
   </Reference>
   ```

3. **Update BepInEx**:
   - Download latest BepInEx
   - Reinstall in V Rising server directory

## Plugin Loading Problems

### Initialization Errors

**Error**: `Failed to initialize services`

**Check**:
1. **Service Registration**:
   ```csharp
   services.AddAutomationSystem(options =>
   {
       options.DataDirectory = "Data/AutomationSystem";
   });
   ```

2. **Directory Permissions**:
   ```bash
   chmod 755 Data/AutomationSystem/
   ```

3. **Configuration Files**:
   - Verify `arena_config.json` exists
   - Check JSON syntax with online validator

### Service Initialization Failures

**Symptoms**:
- Services not starting
- Null reference exceptions
- Dependency injection errors

**Debug Steps**:

1. **Check Service Logs**:
   ```bash
   grep "AutomationSystem" BepInEx/LogOutput.log
   ```

2. **Verify Service Registration**:
   ```csharp
   var serviceProvider = services.BuildServiceProvider();
   var coreDataManager = serviceProvider.GetService<ICoreDataManager>();
   ```

3. **Test Individual Services**:
   - Comment out services one by one
   - Identify failing service

## Command Issues

### Commands Not Recognized

**Symptoms**:
- Commands return "Unknown command"
- No response to `.pe`, `.px`, etc.

**Solutions**:

1. **Check Plugin Load Order**:
   - Ensure AutomationSystem loads after VampireCommandFramework
   - Check BepInEx config for load order

2. **Verify Command Registration**:
   ```csharp
   CommandRegistry.RegisterAll();
   ```

3. **Test Basic Commands**:
   ```bash
   # Try VampireCommandFramework commands first
   .vcf help
   ```

### Permission Errors

**Error**: `You must be an admin to use this command`

**Solutions**:

1. **Check Admin Status**:
   ```bash
   # In server console
   adminauth <steamid>
   ```

2. **Verify Admin List**:
   ```
   VRisingServer_Data/StreamingAssets/Settings/adminlist.txt
   ```

3. **Command Attributes**:
   ```csharp
   [Command("give", adminOnly: true)]
   ```

### Command Execution Errors

**Error**: `Character not found!`

**Causes**:
- Player not fully loaded
- Entity not spawned yet
- Network synchronization issues

**Solutions**:
1. Wait a few seconds after joining
2. Reconnect to server
3. Check server performance

## Arena System Problems

### Arena Entry Failures

**Error**: `Failed to execute portal entry`

**Check**:

1. **Arena Configuration**:
   ```json
   {
     "ArenaEnterPoint": { "X": 1000, "Y": 20, "Z": 500 },
     "ArenaExitPoint": { "X": 1500, "Y": 20, "Z": 800 }
   }
   ```

2. **Snapshot Creation**:
   - Check disk space
   - Verify Data directory permissions
   - Test snapshot service independently

3. **Player State**:
   - Ensure player is not already in arena
   - Check for existing snapshots

### Arena Exit Issues

**Error**: `Failed to execute portal exit`

**Debug**:

1. **Snapshot Restoration**:
   ```csharp
   var success = SnapshotManagerService.LeaveArena(steamId, userEntity, characterEntity);
   ```

2. **Inventory Clearing**:
   - Check for stuck items
   - Verify equipment removal

3. **Blood Type Restoration**:
   - Ensure snapshot contains blood data
   - Check for blood component issues

### Proximity Detection Problems

**Symptoms**:
- Auto-enter not triggering
- Players not detected near arena

**Check**:

1. **Proximity Settings**:
   ```json
   {
     "AutoProximityEnabled": true,
     "ProximityCheckInterval": 5.0,
     "MaxProximityDistance": 25.0
   }
   ```

2. **Coordinate Accuracy**:
   - Verify arena coordinates
   - Check for floating-point precision issues

3. **Performance**:
   - Reduce check interval if server lagging
   - Increase distance for better detection

## Snapshot Issues

### Snapshot Creation Failures

**Error**: `Failed to create snapshot`

**Causes**:
- Insufficient disk space
- File permission issues
- Serialization errors

**Solutions**:

1. **Check Disk Space**:
   ```bash
   df -h Data/AutomationSystem/
   ```

2. **Verify Permissions**:
   ```bash
   chmod -R 755 Data/AutomationSystem/
   ```

3. **Test Serialization**:
   ```csharp
   var json = JsonSerializer.Serialize(playerData);
   ```

### Snapshot Restoration Problems

**Error**: `Failed to restore snapshot`

**Debug**:

1. **Snapshot Integrity**:
   ```bash
   # Check file exists and is readable
   ls -la Data/AutomationSystem/snapshots/
   ```

2. **Data Validation**:
   - Verify snapshot contains required data
   - Check for corrupted JSON

3. **Entity State**:
   - Ensure player entity is valid
   - Check for component conflicts

### Snapshot Performance Issues

**Symptoms**:
- Server lag during snapshots
- Large snapshot files

**Optimizations**:

1. **Compression**:
   ```json
   {
     "CompressSnapshots": true
   }
   ```

2. **Cleanup**:
   ```json
   {
     "SnapshotRetentionDays": 30
   }
   ```

3. **Async Processing**:
   ```csharp
   await Task.Run(() => CreateSnapshotAsync(steamId, snapshot));
   ```

## Performance Issues

### High CPU Usage

**Symptoms**:
- Server performance degradation
- Command delays

**Check**:

1. **Snapshot Frequency**:
   ```json
   {
     "PersistenceIntervalSeconds": 60
   }
   ```

2. **Concurrent Operations**:
   ```json
   {
     "MaxConcurrentSnapshots": 3
   }
   ```

3. **Memory Usage**:
   - Monitor garbage collection
   - Check for memory leaks

### Memory Issues

**Symptoms**:
- Out of memory errors
- Frequent garbage collection

**Solutions**:

1. **Cache Management**:
   ```json
   {
     "MemoryCacheSize": 50
   }
   ```

2. **Object Pooling**:
   - Use Unity's object pooling for entities
   - Dispose native collections properly

3. **Memory Monitoring**:
   ```csharp
   var memoryUsed = GC.GetTotalMemory(false);
   ```

### Network Latency

**Symptoms**:
- Command response delays
- Synchronization issues

**Check**:

1. **Network Settings**:
   ```json
   {
     "SyncInterval": 1.0,
     "TimeoutSeconds": 10
   }
   ```

2. **Packet Size**:
   ```json
   {
     "MaxPacketSize": 4096
   }
   ```

## Configuration Problems

### Configuration Not Loading

**Error**: `Configuration file not found`

**Solutions**:

1. **File Location**:
   ```
   BepInEx/config/AutomationSystem/arena_config.json
   ```

2. **File Permissions**:
   ```bash
   chmod 644 arena_config.json
   ```

3. **JSON Validation**:
   - Use online JSON validator
   - Check for syntax errors

### Invalid Configuration

**Error**: `Configuration validation failed`

**Check**:

1. **Schema Compliance**:
   ```json
   {
     "$schema": "arena_config.schema.json"
   }
   ```

2. **Required Fields**:
   - ArenaEnterPoint
   - ArenaExitPoint
   - MaxPlayers

3. **Data Types**:
   - Coordinates as float3 objects
   - Booleans as true/false
   - Numbers as integers/floats

### Runtime Configuration Changes

**Issue**: Changes not taking effect

**Solutions**:

1. **Reload Command**:
   ```bash
   .arena_reload
   ```

2. **Restart Required**:
   - Some changes require server restart
   - Check documentation for specific settings

## Debug Tools

### Built-in Debug Commands

#### Status Commands
```bash
.arena_status     # System status
.arena_stats      # Statistics
.status          # Player status
```

#### Configuration Commands
```bash
.arena_reload    # Reload config
.arena_save      # Save config
```

#### Logging Commands
```bash
# Check logs
tail -f BepInEx/LogOutput.log
grep "AutomationSystem" BepInEx/LogOutput.log
```

### External Debug Tools

#### Process Monitoring
```bash
# Monitor server process
top -p $(pgrep VRisingServer)

# Memory usage
ps aux | grep VRisingServer
```

#### File System Monitoring
```bash
# Watch config changes
inotifywait -m BepInEx/config/AutomationSystem/

# Check file sizes
du -sh Data/AutomationSystem/
```

### Debug Configuration

Enable debug logging:

```json
{
  "EnableDebugLogging": true,
  "LogLevel": "Debug",
  "LogToFile": true,
  "LogToConsole": true
}
```

### Performance Profiling

1. **Enable Profiling**:
   ```json
   {
     "EnablePerformanceProfiling": true
   }
   ```

2. **Monitor Metrics**:
   - Snapshot creation time
   - Command execution time
   - Memory usage patterns

3. **Profile Commands**:
   ```csharp
   var stopwatch = Stopwatch.StartNew();
   // Execute operation
   stopwatch.Stop();
   Logger.LogInfo($"Operation took {stopwatch.ElapsedMilliseconds}ms");
   ```

## Common Error Codes

| Error Code | Description | Solution |
|------------|-------------|----------|
| `SNAPSHOT_CREATE_FAILED` | Snapshot creation failed | Check disk space and permissions |
| `SNAPSHOT_RESTORE_FAILED` | Snapshot restoration failed | Verify snapshot integrity |
| `COMMAND_EXECUTION_FAILED` | Command failed to execute | Check command parameters |
| `CONFIG_LOAD_FAILED` | Configuration failed to load | Validate JSON syntax |
| `SERVICE_INIT_FAILED` | Service initialization failed | Check dependencies |

## Emergency Procedures

### Plugin Disable
If plugin causes server instability:

1. **Move Plugin**:
   ```bash
   mv BepInEx/plugins/AutomationSystem BepInEx/plugins/AutomationSystem.disabled
   ```

2. **Restart Server**:
   ```bash
   systemctl restart vrising-server
   ```

3. **Restore Backups**:
   - Restore player data from backups
   - Reconfigure arena points

### Data Recovery

1. **Snapshot Recovery**:
   ```bash
   cp -r Data/AutomationSystem.backup Data/AutomationSystem
   ```

2. **Configuration Recovery**:
   ```bash
   cp config/backup/arena_config.json config/AutomationSystem/
   ```

3. **Database Recovery**:
   - Use backup database files
   - Restore from last known good state

## Support Resources

### Log File Locations
- Server logs: `BepInEx/LogOutput.log`
- Plugin logs: `Data/AutomationSystem/logs/`
- Config backups: `BepInEx/config/backups/`

### Diagnostic Commands
```bash
# System information
.arena_status
.arena_stats

# Configuration validation
.arena_reload

# Service testing
.status
```

### Community Support
- GitHub Issues: Report bugs and request features
- Discord: Community discussions and support
- Documentation: Check Docs/ folder for detailed guides

## Prevention Best Practices

### Regular Maintenance

1. **Backup Schedule**:
   - Daily configuration backups
   - Weekly data backups
   - Monthly full server backups

2. **Monitoring**:
   - Monitor server performance
   - Check log files regularly
   - Validate configurations

3. **Updates**:
   - Keep plugin updated
   - Test updates on staging server
   - Review changelog for breaking changes

### Configuration Management

1. **Version Control**:
   - Store configurations in Git
   - Document changes
   - Tag releases

2. **Testing**:
   - Test configurations on development server
   - Validate before production deployment
   - Have rollback procedures ready

3. **Documentation**:
   - Document custom configurations
   - Keep change logs
   - Train administrators
