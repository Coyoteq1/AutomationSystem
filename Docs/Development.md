# Development Guide

Guide for developers working on the AutomationSystem plugin.

## Table of Contents
- [Project Structure](#project-structure)
- [Development Environment](#development-environment)
- [Building and Testing](#building-and-testing)
- [Code Standards](#code-standards)
- [Adding New Features](#adding-new-features)
- [Command Development](#command-development)
- [Service Development](#service-development)
- [Testing](#testing)
- [Debugging](#debugging)
- [Contributing](#contributing)

## Project Structure

```
AutomationSystem/
├── Plugin.cs                 # Main plugin entry point
├── MyPluginInfo.cs           # Plugin metadata
├── ServiceCollectionExtensions.cs # DI configuration
├── AutomationSystem.csproj   # Project configuration
├── README.md                 # User documentation
├── Docs/                     # Developer documentation
│   ├── API.md               # API reference
│   ├── Commands.md          # Command reference
│   ├── Configuration.md     # Configuration guide
│   ├── Development.md       # This file
│   └── Troubleshooting.md   # Troubleshooting guide
├── Core/                     # Core systems
│   ├── ArenaService.cs      # Arena management
│   ├── ArenaLifecycleManager.cs # Arena lifecycle
│   ├── ArenaEngineConfig.cs # Arena configuration
│   ├── VSystemBase.cs       # Base system class
│   ├── SystemContext.cs     # System context
│   └── ISystemWork.cs       # Work interface
├── Services/                 # Business logic services
│   ├── PlayerService.cs     # Player management
│   ├── SnapshotManagerService.cs # Snapshot operations
│   ├── DataPersistenceManager.cs # Data persistence
│   ├── InventoryManagementService.cs # Inventory handling
│   ├── BuildManager.cs      # Build/loadout management
│   └── [Other services...]
├── Automation/               # Automation logic
│   ├── AutomationTracker.cs # Event tracking
│   ├── IAutomationTracker.cs # Tracking interface
│   └── Commands/            # Command implementations
│       ├── Commands.cs      # Main commands
│       ├── PortalCommands.cs # Portal commands
│       └── [Other command files...]
├── Snapshots/               # Snapshot system
│   ├── SnapshotManager.cs   # Snapshot management
│   ├── ISnapshotManager.cs  # Snapshot interface
│   ├── PlayerSnapshot.cs    # Snapshot data model
│   └── PlayerSnapshotService.cs # Snapshot service
├── Models/                  # Data models
│   ├── Player.cs            # Player data
│   ├── WeaponModel.cs       # Weapon data
│   ├── ArenaLoadout.cs      # Loadout data
│   └── CommandArguments/    # Command argument models
├── Data/                    # Game data and configurations
│   ├── Loadouts.cs          # Loadout definitions
│   ├── PlayerSnapshot.cs    # Snapshot data
│   ├── VBloodGUIDs.cs       # VBlood GUIDs
│   ├── PrefabMappings.cs    # Prefab mappings
│   ├── FoundVBlood.cs       # VBlood data
│   └── Shared/              # Shared data models
├── Utils/                   # Utility classes
│   ├── Persistence.cs       # Persistence utilities
│   └── Float3JsonConverter.cs # JSON converters
└── Helpers/                 # Helper functions
    ├── WeaponManager.cs     # Weapon utilities
    └── EffectsHelper.cs     # Effect utilities
```

## Development Environment

### Prerequisites

1. **.NET 8.0 SDK**
   ```bash
   # Install .NET 8.0 SDK
   wget https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh
   chmod +x dotnet-install.sh
   ./dotnet-install.sh --channel 8.0
   ```

2. **Visual Studio 2022** or **VS Code**
   - C# Dev Kit extension for VS Code
   - .NET SDK integration

3. **V Rising Server** (for testing)
   - Steam installation
   - BepInEx framework

4. **Git** for version control

### Environment Setup

1. **Clone Repository**:
   ```bash
   git clone https://github.com/Coyoteq1/AutomationSystem.git
   cd AutomationSystem
   ```

2. **Restore Dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build Project**:
   ```bash
   dotnet build --configuration Debug
   ```

4. **Setup Test Environment**:
   ```bash
   # Copy to test server
   cp bin/Debug/net8.0/AutomationSystem.dll /path/to/vrising/BepInEx/plugins/
   ```

## Building and Testing

### Build Configurations

```xml
<!-- AutomationSystem.csproj -->
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <OutputType>Library</OutputType>
  <AssemblyName>AutomationSystem</AssemblyName>
</PropertyGroup>
```

### Build Commands

```bash
# Debug build
dotnet build --configuration Debug

# Release build
dotnet build --configuration Release

# Clean build
dotnet clean
dotnet build

# Publish
dotnet publish --configuration Release --output publish/
```

### Testing Strategy

1. **Unit Tests** (Future implementation)
   ```csharp
   [TestMethod]
   public void TestSnapshotCreation()
   {
       // Test snapshot creation logic
   }
   ```

2. **Integration Tests**
   - Test with V Rising server
   - Verify command execution
   - Check data persistence

3. **Manual Testing**
   - Arena entry/exit flow
   - Command functionality
   - Performance under load

### Test Server Setup

1. **Dedicated Test Server**:
   ```bash
   # Create test server directory
   mkdir vrising-test-server
   cd vrising-test-server

   # Copy V Rising server files
   cp -r /path/to/steam/vrising/server/* ./

   # Install BepInEx
   # Install plugin
   ```

2. **Configuration for Testing**:
   ```json
   {
     "DataDirectory": "Data/TestAutomationSystem",
     "AutoPersistenceEnabled": false,
     "EnableDebugLogging": true
   }
   ```

## Code Standards

### C# Coding Standards

1. **Naming Conventions**:
   ```csharp
   // Classes and interfaces
   public class PlayerService { }
   public interface IPlayerService { }

   // Methods
   public void CreateSnapshot() { }
   public async Task RestoreSnapshotAsync() { }

   // Properties
   public string PlayerName { get; set; }
   public ulong SteamId { get; private set; }

   // Private fields
   private readonly ILogger _logger;
   private const int MaxRetries = 3;
   ```

2. **File Organization**:
   - One class per file
   - Related classes in same namespace
   - Clear folder structure

3. **Documentation**:
   ```csharp
   /// <summary>
   /// Creates a snapshot of player data for restoration later.
   /// </summary>
   /// <param name="steamId">The player's Steam ID</param>
   /// <param name="playerEntity">The player's entity</param>
   /// <returns>Task representing the async operation</returns>
   public async Task CreateSnapshotAsync(ulong steamId, Entity playerEntity)
   ```

### Error Handling

```csharp
public class AutomationException : Exception
{
    public string ErrorCode { get; }
    public object Context { get; }

    public AutomationException(string message, string errorCode, object context = null)
        : base(message)
    {
        ErrorCode = errorCode;
        Context = context;
    }
}

public async Task ExecuteCommandAsync()
{
    try
    {
        // Operation logic
        await PerformOperationAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Command execution failed");
        throw new AutomationException("Command failed", "COMMAND_EXECUTION_FAILED", ex);
    }
}
```

### Logging Standards

```csharp
public class PlayerService
{
    private readonly ILogger _logger;

    public PlayerService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void ProcessPlayerAction(ulong steamId, string action)
    {
        _logger.LogInformation("Player {SteamId} performed action: {Action}", steamId, action);

        try
        {
            // Action logic
            _logger.LogDebug("Action completed successfully for player {SteamId}", steamId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process action for player {SteamId}", steamId);
            throw;
        }
    }
}
```

## Adding New Features

### Feature Development Process

1. **Planning**:
   - Define feature requirements
   - Identify affected components
   - Plan data structures

2. **Implementation**:
   - Create feature branch
   - Implement core logic
   - Add tests

3. **Integration**:
   - Update dependencies
   - Modify existing code
   - Update documentation

4. **Testing**:
   - Unit tests
   - Integration tests
   - Manual testing

### Example: Adding New Command

1. **Define Command Interface**:
   ```csharp
   [CommandGroup("newfeature", "New feature commands")]
   public static class NewFeatureCommands
   {
       [Command("dostuff", description: "Does cool stuff")]
       public static void DoStuff(ChatCommandContext ctx, string parameter)
       {
           // Implementation
       }
   }
   ```

2. **Add to Commands.cs**:
   ```csharp
   // In Commands.cs
   internal static class Commands
   {
       // Existing commands...

       [Command("newcommand", description: "New command")]
       public static void NewCommand(ChatCommandContext ctx)
       {
           // Implementation
       }
   }
   ```

3. **Update Documentation**:
   - Add to Commands.md
   - Update README.md if needed

## Command Development

### Command Structure

```csharp
[Command("commandname", description: "Command description", adminOnly: false)]
public static void CommandName(ChatCommandContext ctx, [parameters...])
{
    // Command implementation
}
```

### Command Parameters

```csharp
// Basic parameters
public static void BasicCommand(ChatCommandContext ctx, string text, int number)

// Optional parameters
public static void OptionalCommand(ChatCommandContext ctx, string required, int optional = 5)

// Enum parameters
public static void EnumCommand(ChatCommandContext ctx, MyEnum value)

// Complex parameters
public static void ComplexCommand(ChatCommandContext ctx, PrefabGUID guid)
```

### Command Context Usage

```csharp
public static void ExampleCommand(ChatCommandContext ctx)
{
    // Check permissions
    if (!ctx.IsAdmin)
    {
        ctx.Error("Admin required");
        return;
    }

    // Get player info
    var characterEntity = PlayerManager.GetPlayerByName(ctx.Name);
    if (characterEntity == Entity.Null)
    {
        ctx.Error("Player not found");
        return;
    }

    // Execute logic
    var result = PerformOperation(characterEntity);

    // Respond to player
    if (result.Success)
    {
        ctx.Reply($"Success: {result.Message}");
    }
    else
    {
        ctx.Error($"Failed: {result.Message}");
    }
}
```

### Command Groups

```csharp
[CommandGroup("arena", "Arena management commands")]
public static class ArenaCommands
{
    [Command("enter", "Enter the arena")]
    public static void EnterArena(ChatCommandContext ctx) { }

    [Command("exit", "Exit the arena")]
    public static void ExitArena(ChatCommandContext ctx) { }
}
```

## Service Development

### Service Interface Pattern

```csharp
// Interface
public interface IExampleService
{
    Task InitializeAsync();
    Task ShutdownAsync();
    Task<OperationResult> PerformOperationAsync(OperationRequest request);
}

// Implementation
public class ExampleService : IExampleService
{
    private readonly ILogger _logger;
    private readonly IConfigurationService _config;

    public ExampleService(ILogger logger, IConfigurationService config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("ExampleService initializing");
        // Initialization logic
    }

    public async Task ShutdownAsync()
    {
        _logger.LogInformation("ExampleService shutting down");
        // Cleanup logic
    }

    public async Task<OperationResult> PerformOperationAsync(OperationRequest request)
    {
        try
        {
            // Operation logic
            return OperationResult.Success("Operation completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Operation failed");
            return OperationResult.Failure("Operation failed", ex);
        }
    }
}
```

### Service Registration

```csharp
// In ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAutomationSystem(
        this IServiceCollection services,
        Action<AutomationOptions> configureOptions)
    {
        var options = new AutomationOptions();
        configureOptions?.Invoke(options);

        // Register services
        services.AddSingleton<IExampleService, ExampleService>();
        services.AddScoped<IOperationService, OperationService>();

        return services;
    }
}
```

### Dependency Injection

```csharp
public class DependentService
{
    private readonly IExampleService _exampleService;
    private readonly IOperationService _operationService;

    public DependentService(
        IExampleService exampleService,
        IOperationService operationService)
    {
        _exampleService = exampleService;
        _operationService = operationService;
    }
}
```

## Testing

### Unit Testing Setup

```xml
<!-- Add to .csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.0.0" />
  <PackageReference Include="MSTest.TestAdapter" Version="2.2.10" />
  <PackageReference Include="MSTest.TestFramework" Version="2.2.10" />
  <PackageReference Include="Moq" Version="4.18.2" />
</ItemGroup>
```

### Example Unit Test

```csharp
[TestClass]
public class PlayerServiceTests
{
    private Mock<ILogger> _loggerMock;
    private PlayerService _service;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger>();
        _service = new PlayerService(_loggerMock.Object);
    }

    [TestMethod]
    public async Task GetPlayerEntity_ValidName_ReturnsEntity()
    {
        // Arrange
        var playerName = "TestPlayer";

        // Act
        var result = await _service.GetPlayerEntityAsync(playerName);

        // Assert
        Assert.IsNotNull(result);
        _loggerMock.Verify(
            x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()),
            Times.Once);
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class ArenaIntegrationTests
{
    [TestMethod]
    public async Task EnterArena_ValidPlayer_Succeeds()
    {
        // Setup test server environment
        var testServer = new TestServerEnvironment();

        try
        {
            // Arrange
            var player = await testServer.CreateTestPlayerAsync();

            // Act
            var result = await testServer.ExecuteCommandAsync(".pe", player);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(await testServer.IsPlayerInArenaAsync(player));
        }
        finally
        {
            await testServer.CleanupAsync();
        }
    }
}
```

## Debugging

### Debug Configuration

```json
{
  "EnableDebugLogging": true,
  "LogLevel": "Debug",
  "LogToFile": true,
  "LogToConsole": true,
  "EnablePerformanceProfiling": true
}
```

### Debug Commands

```csharp
// Add debug commands
[Command("debug_snapshot", adminOnly: true)]
public static void DebugSnapshot(ChatCommandContext ctx)
{
    var snapshotCount = SnapshotManagerService.GetSnapshotCount();
    var memoryUsage = GC.GetTotalMemory(false);

    ctx.Reply($"Snapshots: {snapshotCount}, Memory: {memoryUsage / 1024 / 1024}MB");
}
```

### Logging Best Practices

```csharp
public void ProcessData(object data)
{
    _logger.LogDebug("Processing data: {@Data}", data);

    var stopwatch = Stopwatch.StartNew();
    try
    {
        // Processing logic
        _logger.LogInformation("Data processed successfully in {Elapsed}ms", stopwatch.ElapsedMilliseconds);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to process data after {Elapsed}ms", stopwatch.ElapsedMilliseconds);
        throw;
    }
}
```

### Performance Profiling

```csharp
public class PerformanceProfiler
{
    private readonly Stopwatch _stopwatch = new();
    private readonly ILogger _logger;

    public PerformanceTimer Start(string operationName)
    {
        return new PerformanceTimer(operationName, _logger);
    }
}

public class PerformanceTimer : IDisposable
{
    private readonly string _operationName;
    private readonly ILogger _logger;
    private readonly Stopwatch _stopwatch;

    public PerformanceTimer(string operationName, ILogger logger)
    {
        _operationName = operationName;
        _logger = logger;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        _logger.LogInformation("Operation '{Operation}' completed in {Elapsed}ms",
            _operationName, _stopwatch.ElapsedMilliseconds);
    }
}
```

## Contributing

### Contribution Workflow

1. **Fork Repository**
2. **Create Feature Branch**
   ```bash
   git checkout -b feature/new-feature
   ```

3. **Make Changes**
   - Follow code standards
   - Add tests
   - Update documentation

4. **Commit Changes**
   ```bash
   git add .
   git commit -m "Add new feature"
   ```

5. **Push and Create PR**
   ```bash
   git push origin feature/new-feature
   # Create pull request on GitHub
   ```

### Pull Request Guidelines

- **Title**: Clear, descriptive title
- **Description**: Detailed explanation of changes
- **Tests**: Include tests for new functionality
- **Documentation**: Update docs for changes
- **Breaking Changes**: Clearly mark breaking changes

### Code Review Checklist

- [ ] Code follows established patterns
- [ ] Unit tests added/updated
- [ ] Documentation updated
- [ ] No breaking changes without justification
- [ ] Performance considerations addressed
- [ ] Security implications reviewed
- [ ] Error handling appropriate

### Release Process

1. **Version Bump**
   ```csharp
   // In MyPluginInfo.cs
   public const string PLUGIN_VERSION = "1.1.0";
   ```

2. **Changelog Update**
   ```
   ## v1.1.0
   - Added new feature
   - Fixed bug
   - Updated documentation
   ```

3. **Build Release**
   ```bash
   dotnet build --configuration Release
   dotnet publish --configuration Release
   ```

4. **Create GitHub Release**
   - Tag version
   - Upload build artifacts
   - Write release notes

### Issue Reporting

When reporting bugs or requesting features:

1. **Bug Reports**:
   - Clear title and description
   - Steps to reproduce
   - Expected vs actual behavior
   - Server logs
   - System information

2. **Feature Requests**:
   - Clear description of feature
   - Use case justification
   - Implementation suggestions
   - Impact assessment

### Community Guidelines

- Be respectful and constructive
- Provide detailed information
- Test solutions before reporting
- Help others when possible
- Follow project code of conduct

## Advanced Topics

### Plugin Architecture

The plugin follows a modular architecture with clear separation of concerns:

- **Plugin.cs**: Entry point and lifecycle management
- **Core**: Core business logic and systems
- **Services**: Reusable business services
- **Automation**: Command and automation logic
- **Models**: Data structures and DTOs
- **Utils**: Utility functions and helpers

### Dependency Management

```xml
<!-- AutomationSystem.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
  <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  <PackageReference Include="VRising.VampireCommandFramework" Version="0.10.4" />
</ItemGroup>

<ItemGroup>
  <Reference Include="ProjectM.Shared">
    <HintPath>C:\Program Files (x86)\Steam\steamapps\common\VRising\VRising_Server\BepInEx\interop\ProjectM.Shared.dll</HintPath>
  </Reference>
</ItemGroup>
```

### Performance Optimization

1. **Async Operations**:
   ```csharp
   public async Task ProcessAsync()
   {
       await Task.Run(() => HeavyOperation());
   }
   ```

2. **Memory Management**:
   ```csharp
   using var entities = query.ToEntityArray(Allocator.Temp);
   try
   {
       // Use entities
   }
   finally
   {
       entities.Dispose();
   }
   ```

3. **Caching**:
   ```csharp
   private readonly MemoryCache _cache = new(new MemoryCacheOptions());

   public T GetOrCreate<T>(string key, Func<T> factory)
   {
       return _cache.GetOrCreate(key, entry =>
       {
           entry.SlidingExpiration = TimeSpan.FromMinutes(5);
           return factory();
       });
   }
   ```

### Security Considerations

1. **Input Validation**:
   ```csharp
   public void ProcessCommand(string input)
   {
       if (string.IsNullOrWhiteSpace(input))
           throw new ArgumentException("Input cannot be empty");

       // Sanitize input
       var sanitized = SanitizeInput(input);
   }
   ```

2. **Permission Checks**:
   ```csharp
   [Command("admincommand", adminOnly: true)]
   public static void AdminCommand(ChatCommandContext ctx)
   {
       // Additional permission checks if needed
   }
   ```

3. **Rate Limiting**:
   ```csharp
   private readonly Dictionary<ulong, DateTime> _lastCommand = new();

   public bool CheckRateLimit(ulong steamId)
   {
       if (_lastCommand.TryGetValue(steamId, out var lastTime))
       {
           if (DateTime.UtcNow - lastTime < TimeSpan.FromSeconds(1))
               return false;
       }

       _lastCommand[steamId] = DateTime.UtcNow;
       return true;
   }
   ```

This development guide provides the foundation for contributing to the AutomationSystem plugin. Remember to always test changes thoroughly and follow the established patterns and standards.
