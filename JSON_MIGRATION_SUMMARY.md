# JSON Handling Migration to ICB.core Pattern

## Summary
Updated CrowbaneArena JSON handling to match ICB.core pattern for consistency and improved reliability.

## Key Changes

### 1. Enhanced Persistence.cs
- Added backup functionality (creates .bak files)
- Added JsonStringEnumConverter for enum support
- Improved error handling with backup restoration
- Follows ICB.core error handling patterns

### 2. New ConfigService.cs
- Static service following ICB.core ArenaConfigService pattern
- Robust initialization with fallback mechanisms
- Automatic backup creation and restoration
- Validation and sanitization of configuration data
- Centralized configuration management

### 3. Updated ArenaConfigLoader.cs
- Now acts as legacy wrapper for backward compatibility
- Delegates all operations to ConfigService
- Maintains existing API for existing code
- Simplified implementation removes duplicate code

### 4. Configuration Structure
- Removed ConfigFile wrapper class
- ArenaConfig is now the root configuration object
- Simplified class definitions with minimal code
- Maintained all existing properties and functionality

### 5. Core Integration
- ConfigService.Initialize() called in CrowbaneArenaCore.Initialize()
- Proper initialization order maintained
- Configuration loaded before other services

## Benefits

### Consistency
- Matches ICB.core JSON handling patterns exactly
- Uses same JsonSerializerOptions configuration
- Same error handling and logging approach

### Reliability
- Automatic backup creation and restoration
- Better error recovery mechanisms
- Validation and sanitization of configuration data
- Graceful handling of corrupted files

### Maintainability
- Centralized configuration management
- Reduced code duplication
- Clear separation of concerns
- Easier to extend and modify

## Backward Compatibility
- All existing ArenaConfigLoader methods still work
- No breaking changes to existing code
- Configuration file format remains the same
- Legacy properties maintained

## Testing
- Added ConfigServiceTests.cs for basic functionality testing
- Tests run automatically in debug builds
- Validates initialization, saving, and persistence utilities

## File Structure
```
Services/
  ConfigService.cs          # New centralized config service
Utils/
  Persistence.cs           # Enhanced with ICB.core features
Tests/
  ConfigServiceTests.cs    # Basic functionality tests
ArenaConfigLoader.cs       # Legacy wrapper (backward compatibility)
```

## Usage Example
```csharp
// Initialize (called automatically in Core.Initialize())
ConfigService.Initialize();

// Access configuration
var config = ConfigService.Config;
bool arenaEnabled = config.Enabled;

// Save changes
ConfigService.SaveConfig();

// Use persistence utility
Persistence.SaveToFile("CrowbaneArena", "data.json", myData);
var loadedData = Persistence.LoadFromFile<MyType>("CrowbaneArena", "data.json");
```

This migration ensures CrowbaneArena follows the same robust JSON handling patterns as ICB.core while maintaining full backward compatibility.