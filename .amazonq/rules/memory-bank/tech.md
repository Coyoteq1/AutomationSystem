# CrowbaneArena Mod - Technology Stack

## Programming Languages
- **C# 10.0**: Primary development language with modern language features
- **Target Framework**: .NET 6.0 for cross-platform compatibility

## Core Dependencies

### Game Integration
- **V Rising Server**: Target game platform
- **BepInEx 6.0.0-be.738**: Plugin framework for Unity IL2CPP games
- **Unity ECS**: Entity Component System integration
- **Unity Mathematics**: Vector and mathematical operations

### Modding Framework
- **VampireCommandFramework**: Command system for V Rising mods
- **Harmony**: Runtime patching library for method interception
- **Il2CppInterop.Runtime**: IL2CPP interoperability layer

### Data Management
- **Newtonsoft.Json 13.0.3**: JSON serialization and configuration management
- **Microsoft.Extensions.DependencyInjection 8.0.0**: Service container and dependency injection
- **Microsoft.Extensions.Logging.Abstractions 8.0.0**: Structured logging framework

### Development Tools
- **NUnit 3.13.3**: Unit testing framework
- **Microsoft.CSharp**: Dynamic language features support

## Build System
- **MSBuild**: Project compilation and dependency management
- **NuGet Package Manager**: Dependency resolution and package management
- **Post-build Events**: Automatic deployment to V Rising server plugins directory

## Development Environment

### Project Configuration
```xml
<TargetFramework>net6.0</TargetFramework>
<LangVersion>10.0</LangVersion>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks>
<ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable>
```

### Build Commands
- **Debug Build**: `dotnet build --configuration Debug`
- **Release Build**: `dotnet build --configuration Release`
- **Clean**: `dotnet clean`
- **Restore**: `dotnet restore`

### Assembly References
- **V Rising Game Assemblies**: ProjectM.dll, Stunlock.Core.dll, Unity libraries
- **BepInEx Core**: Plugin framework and IL2CPP interop
- **Unity Engine**: CoreModule, PhysicsModule, Mathematics

## Runtime Environment
- **V Rising Server**: Windows/Linux dedicated server
- **BepInEx Plugin Directory**: Auto-deployment via post-build events
- **Configuration Directory**: JSON files for runtime configuration
- **Logging**: BepInEx logging system integration

## Development Workflow
1. **Code Development**: Visual Studio/VS Code with C# extensions
2. **Build Process**: MSBuild compilation with dependency resolution
3. **Testing**: NUnit test execution and validation
4. **Deployment**: Automatic copy to V Rising server plugins folder
5. **Runtime Testing**: In-game validation and log monitoring

## Performance Considerations
- **IL2CPP Compatibility**: Optimized for Unity's IL2CPP runtime
- **Memory Management**: Careful object lifecycle management
- **Entity System Integration**: Efficient ECS component access patterns
- **Harmony Patching**: Minimal runtime overhead for method interception