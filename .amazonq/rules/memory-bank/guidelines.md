# CrowbaneArena Mod - Development Guidelines

## Code Quality Standards

### Naming Conventions
- **Classes**: PascalCase with descriptive names (`SnapshotManagerService`, `PlayerSnapshot`)
- **Methods**: PascalCase with action verbs (`EnterArena`, `CaptureInventory`, `RestoreSnapshot`)
- **Properties**: PascalCase (`IsInArena`, `OriginalLocation`, `EntityManager`)
- **Fields**: camelCase with underscore prefix for private fields (`_snapshots`, `_world`, `_entityManager`)
- **Constants**: PascalCase with descriptive names (`SnapshotsFileName`)

### Documentation Standards
- **XML Documentation**: Comprehensive `<summary>` tags for all public methods and classes
- **Parameter Documentation**: `<param>` tags explaining purpose and constraints
- **Return Documentation**: `<returns>` tags describing return values and conditions
- **Exception Documentation**: Document potential exceptions and error conditions

### Error Handling Patterns
- **Try-Catch Blocks**: Wrap all entity operations and external calls
- **Entity Validation**: Always validate entities before component access using `EntityManager.Exists()`
- **Component Checks**: Use `TryGetComponentData` instead of direct `GetComponentData` when possible
- **Graceful Degradation**: Return default values or empty collections on errors
- **Comprehensive Logging**: Log errors with context, stack traces, and recovery actions

## Architectural Patterns

### Service Layer Architecture
- **Static Service Classes**: Use static classes for stateless services (`SnapshotManagerService`, `PlayerService`)
- **Instance Services**: Use instance classes for stateful services with dependency injection (`SystemService`)
- **Service Initialization**: Centralized initialization through `CrowbaneArenaCore.Initialize()`
- **Lazy Loading**: Properties with null-coalescing assignment for expensive resources

### Entity Component System Integration
- **Entity Manager Access**: Centralized through `CrowbaneArenaCore.EntityManager`
- **Component Safety**: Always check component existence before access
- **Buffer Management**: Proper disposal of `NativeList` and temporary allocations
- **Query Patterns**: Use `ToEntityArray(Allocator.Temp)` with proper disposal

### Configuration Management
- **JSON Serialization**: Use Newtonsoft.Json for configuration files
- **Default Values**: Provide sensible defaults for all configuration options
- **Hot Reload**: Support runtime configuration changes without restart
- **Validation**: Validate configuration data on load with error recovery

## Implementation Patterns

### Error Recovery Strategies
```csharp
// Pattern: Entity validation with ArgumentException handling
try
{
    if (!EM.HasComponent<PlayerCharacter>(characterEntity))
    {
        Plugin.Logger?.LogError("Entity validation failed");
        return false;
    }
}
catch (ArgumentException)
{
    Plugin.Logger?.LogError("Entity may be from different world");
    return false;
}
```

### Logging Patterns
```csharp
// Pattern: Structured logging with context
Plugin.Logger?.LogInfo($"Player {user.CharacterName} entered arena successfully");
Plugin.Logger?.LogError($"Critical error in {nameof(EnterArena)}: {ex.Message}");
Plugin.Logger?.LogError($"Stack trace: {ex.StackTrace}");
```

### Data Capture and Restoration
```csharp
// Pattern: Comprehensive state capture with validation
private static void CaptureInventory(Entity characterEntity, PlayerSnapshot snapshot)
{
    Plugin.Logger?.LogInfo("--- Capturing Inventory ---");
    try
    {
        if (ProjectM.InventoryUtilities.TryGetInventoryEntity(EM, characterEntity, out Entity inventoryEntity))
        {
            // Implementation with proper error handling
        }
    }
    catch (Exception ex)
    {
        Plugin.Logger?.LogError($"Error capturing inventory: {ex.Message}");
    }
}
```

### Harmony Patching Standards
```csharp
// Pattern: Safe Harmony patches with comprehensive error handling
[HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
public static class ArenaBuffSystemPatch
{
    public static void Postfix(BuffSystem_Spawn_Server __instance)
    {
        try
        {
            var entityManager = __instance.EntityManager;
            var buffEntities = __instance.__query_401358634_0.ToEntityArray(Allocator.Temp);
            
            // Process entities with proper disposal
            buffEntities.Dispose();
        }
        catch (System.Exception e)
        {
            Plugin.Logger?.LogError($"Error in ArenaBuffSystemPatch: {e}");
        }
    }
}
```

## Memory Management

### Resource Disposal
- **NativeList Disposal**: Always dispose temporary native collections
- **Entity Array Cleanup**: Dispose `ToEntityArray(Allocator.Temp)` results
- **Buffer Management**: Proper lifecycle management for entity buffers

### Performance Optimization
- **Lazy Initialization**: Use null-coalescing assignment for expensive operations
- **Caching Strategies**: Cache frequently accessed components and systems
- **Batch Operations**: Group related entity operations together
- **Query Optimization**: Reuse entity queries where possible

## Integration Standards

### V Rising Game Integration
- **VRisingCore Dependency**: Always check `VRisingCore.IsReady()` before initialization
- **ServerGameManager Usage**: Use for proper item and inventory management
- **Unity ECS Patterns**: Follow Unity's Entity Component System conventions

### BepInEx Plugin Standards
- **Plugin Metadata**: Proper `[BepInPlugin]` attributes with dependencies
- **Initialization Order**: Respect plugin loading sequence and dependencies
- **Configuration Integration**: Use BepInEx configuration system where appropriate

### External Dependencies
- **Null Safety**: Always use null-conditional operators for optional dependencies
- **Version Compatibility**: Handle API changes gracefully with fallback implementations
- **Dependency Injection**: Use constructor injection for required dependencies

## Testing and Validation

### Entity Validation
- Always validate entity existence before component access
- Handle `ArgumentException` for cross-world entity access
- Implement fallback behavior for invalid entities

### Configuration Validation
- Validate all configuration values on load
- Provide meaningful error messages for invalid configurations
- Implement configuration migration for version updates

### Runtime Safety
- Comprehensive exception handling in all public methods
- Graceful degradation when services are unavailable
- Proper cleanup in error scenarios