# AutomationSystem

A comprehensive automation plugin for V Rising servers that provides advanced arena management, player snapshots, automated progression, and extensive admin tools.

## Overview

AutomationSystem is a powerful plugin designed to enhance V Rising server administration and player experience through automated systems. Built with performance and reliability in mind, it offers seamless arena management, persistent player data snapshots, automated progression tracking, and a comprehensive suite of admin tools.

The plugin integrates deeply with V Rising's game mechanics to provide:
- **Automated Arena Systems**: Complete arena entry/exit workflows with state preservation
- **Player Snapshot Management**: Robust data persistence with backup and recovery systems
- **Dynamic Configuration**: Flexible settings that can be adjusted without server restarts
- **Extensive Command Framework**: Player and admin commands for comprehensive server control
- **Progression Automation**: Automated unlocking of content and achievements

## Features

### 🏟️ Arena System
- **Automated Arena Entry/Exit**: `.pe` (Portal Execute) and `.px` (Portal Exit) commands for seamless transitions
- **Player State Management**: Automatic snapshots save/restore complete player progression, including:
  - Experience levels and blood types
  - Full inventory and equipment
  - Active buffs and debuffs
  - Quest progress and achievements
  - Territory and castle ownership
- **Boss Unlocking**: All VBlood bosses automatically unlocked upon arena entry with configurable boss lists
- **Achievement Unlocking**: Complete achievement progression system that grants all achievements in arena mode
- **Blood Type Management**: Max Warrior/Rogue blood types (100%) applied automatically with custom blood type configurations
- **Name Tagging**: Players get `[arena]` prefix while in arena with customizable tag formats
- **Zone Management**: Configurable arena zones with proximity detection and auto-entry features

### 📸 Snapshot System
- **Automatic Persistence**: Player data saved every 60 seconds (configurable intervals from 10-300 seconds)
- **Backup Management**: Multiple backup levels with configurable retention policies:
  - Primary snapshots: Latest player state
  - Secondary backups: Hourly/daily archives
  - Emergency recovery: Pre-arena entry backups
- **Data Recovery**: Restore from latest snapshot or backup on failure with corruption detection
- **Cross-Session Continuity**: Player progress maintained across server restarts and crashes
- **Compression**: Optional data compression to reduce storage footprint
- **Integrity Checks**: Automatic validation of snapshot data integrity

### ⚔️ Combat & Progression
- **Legendary Warrior** (`.lw`): Full heal + buff reset (arena only) with cooldown management
- **Ancient Ritual Technique** (`.art`): Ultimate revive + 30s god mode (arena only) with configurable durations
- **Gear Loadouts**: Apply warrior, mage, or custom gear sets with preset management
- **Item Management**: Give items by PrefabGUID or name with quantity validation
- **Boss Control**: Lock/unlock specific bosses for server management with whitelist/blacklist systems
- **Progression Tracking**: Automated tracking of player advancement with milestone notifications

### 🛠️ Admin Tools
- **Player Management**: Teleport, revive, equip items, clear inventory with target validation
- **World Management**: Clear dropped items, kill entities by type with radius controls
- **Configuration**: Dynamic config reloading and saving without server restarts
- **Debug Tools**: Status checking, coordinate saving, territory management with logging
- **Performance Monitoring**: Real-time metrics and performance statistics
- **Audit Logging**: Comprehensive logging of all admin actions for security

### 🎮 Player Commands
- **Arena Navigation**: `.tp` (teleport), `.setpoint` (set points) with coordinate validation
- **Loadouts**: `.gear`, `.loadout`, `.build` (1-4 presets) with equipment validation
- **Utilities**: `.give`, `.equip`, `.clear`, `.revive` with permission checks
- **Status**: `.status` for comprehensive arena/player info with detailed statistics
- **Help System**: Built-in command help and usage examples

## Installation

### System Requirements
- **V Rising Server**: Dedicated server installation (version 1.0+ recommended)
- **BepInEx Framework**: Version 5.4.21 or higher
- **.NET Runtime**: .NET 8.0 Desktop Runtime (x64)
- **Operating System**: Windows Server 2019+, Windows 10/11, or Linux with Wine/Proton
- **RAM**: Minimum 8GB (16GB recommended for large servers)
- **Storage**: 10GB free space for logs and snapshots

### Prerequisites Checklist
- [ ] V Rising server installed and running
- [ ] Administrative access to server files
- [ ] Backup of server configuration
- [ ] Stable internet connection for downloads

### Detailed Setup Steps

#### 1. Prepare Your Server
```bash
# Stop your V Rising server first
# Backup your existing server configuration
cp -r VRisingServer ~/
```

#### 2. Install BepInEx Framework
1. Visit [BepInEx Releases](https://github.com/BepInEx/BepInEx/releases)
2. Download `BepInEx_x64_5.4.21.0.zip` (or latest stable version)
3. Extract the archive to your V Rising server root directory:
   ```
   VRisingServer/
   ├── BepInEx/          # ← Extract here
   ├── VRisingServer.exe
   └── ...
   ```
4. Run the server once to generate initial configuration:
   ```bash
   ./VRisingServer.exe -batchmode
   ```
5. Verify BepInEx installation by checking for `BepInEx/config/BepInEx.cfg`

#### 3. Install AutomationSystem Plugin
1. Download the latest `AutomationSystem.dll` from [Releases](https://github.com/Coyoteq1/AutomationSystem/releases)
2. Create the plugin directory:
   ```bash
   mkdir -p BepInEx/plugins/AutomationSystem
   ```
3. Copy `AutomationSystem.dll` to the plugin directory
4. Verify file placement:
   ```
   BepInEx/plugins/AutomationSystem/
   └── AutomationSystem.dll
   ```

#### 4. Initial Configuration
1. Start the server to auto-generate configuration files:
   ```bash
   ./VRisingServer.exe
   ```
2. Stop the server after 30 seconds
3. Navigate to `BepInEx/config/AutomationSystem/`
4. Edit configuration files as needed (see Configuration section below)

#### 5. Verify Dependencies
- **VRising.Unhollowed.Client**: Automatically installed with the plugin
- **VampireCommandFramework**: Bundled with AutomationSystem
- **Newtonsoft.Json**: Included in dependencies

### Post-Installation Verification

1. **Start Server**:
   ```bash
   ./VRisingServer.exe
   ```

2. **Check Logs**:
   - Open `BepInEx/LogOutput.log`
   - Look for: `[AutomationSystem] Plugin loaded successfully`
   - Verify no error messages

3. **Test Commands**:
   - Connect to server as admin
   - Try `.status` command in chat
   - Verify admin commands work

### File Structure After Installation
```
VRisingServer/
├── BepInEx/
│   ├── plugins/
│   │   └── AutomationSystem/
│   │       └── AutomationSystem.dll
│   ├── config/
│   │   └── AutomationSystem/
│   │       ├── arena_config.json
│   │       └── snapshot_config.json
│   └── LogOutput.log
├── Data/
│   └── AutomationSystem/
│       └── Snapshots/
└── VRisingServer.exe
```

### Troubleshooting Installation
- **Plugin not loading**: Check `BepInEx/LogOutput.log` for errors
- **Missing dependencies**: Ensure .NET 8.0 is installed
- **Permission issues**: Run server as administrator
- **Port conflicts**: Ensure default ports (27015-27016) are available

## Configuration

The plugin uses JSON configuration files located in `BepInEx/config/AutomationSystem/`. Configuration changes can be applied without restarting the server using `.arena_reload`.

### Core Settings (`arena_config.json`)

#### Global Settings
```json
{
  "ArenaEnabled": true,                    // Enable/disable arena system
  "MaxPlayers": 20,                        // Maximum players allowed in arena
  "NoDurabilityLoss": false,               // Prevent weapon durability loss
  "NoBloodLoss": false,                    // Prevent blood loss in arena
  "BlockHealingPotions": false,            // Block healing potions in arena
  "BlockOPAbilities": true                 // Block overpowered abilities
}
```

#### Arena Points
```json
{
  "ArenaEnterPoint": { "x": -500.0, "y": 0.0, "z": -500.0 },  // Entry teleport location
  "ArenaExitPoint": { "x": 600.0, "y": 0.0, "z": 600.0 },     // Exit teleport location
  "PortalX": -1000.0,                       // Portal X coordinate
  "PortalY": 0.0,                           // Portal Y coordinate
  "PortalZ": -500.0,                        // Portal Z coordinate
  "PortalRadius": 50.0                      // Portal activation radius
}
```

#### Arena Zones
```json
"Zones": [
  {
    "Name": "default",                      // Zone identifier
    "Description": "Default PvP Arena",     // Zone description
    "SpawnX": -1000.0,                      // Spawn X coordinate
    "SpawnY": 0.0,                          // Spawn Y coordinate
    "SpawnZ": -500.0,                       // Spawn Z coordinate
    "Radius": 200.0,                        // Zone radius
    "Enabled": false                        // Zone enabled status
  }
]
```

### Weapon Configuration

#### Available Weapons
```json
"Weapons": [
  {
    "Name": "sword",                        // Weapon identifier
    "Description": "Sanguine Sword",        // Display description
    "Guid": -774462329,                     // Item PrefabGUID
    "Enabled": true,                        // Weapon availability
    "Default": true,                        // Default weapon status
    "status": [],                           // Status effects
    "spell": "bloodmend"                    // Associated spell
  }
  // ... additional weapons
]
```

#### Weapon Modifiers
Weapons support spell school modifiers:
- `s` - Storm (lightning/teleport effects)
- `c` - Chaos (physical power/damage)
- `f` - Frost (ice/slow effects)
- `u` - Unholy (necrotic/poison effects)
- `b` - Blood (lifesteal/healing effects)
- `i` - Illusion (stealth/speed effects)

### Armor Set Configuration

#### Available Armor Sets
```json
"ArmorSets": [
  {
    "Name": "warrior",                      // Armor set identifier
    "Description": "Dracula's Warrior Set", // Display description
    "ChestGuid": 1392314162,                // Chest armor PrefabGUID
    "LegsGuid": 205207385,                  // Leg armor PrefabGUID
    "BootsGuid": -382349289,                // Boot armor PrefabGUID
    "GlovesGuid": 1982551454,               // Glove armor PrefabGUID
    "Enabled": true,                        // Armor set availability
    "Default": true,                        // Default armor status
    "status": [],                           // Status effects
    "spell": "rage"                         // Associated spell
  }
  // ... additional armor sets
]
```

### Loadout Configuration

#### Pre-configured Loadouts
```json
"Loadouts": [
  {
    "Name": "berserker",                    // Loadout identifier
    "Description": "Chaos Greatsword Berserker - High damage melee",
    "Weapons": ["greatsword"],              // Weapon selection
    "WeaponMods": "c4",                     // Weapon modifiers (Chaos Physical Power)
    "ArmorSets": ["warrior"],               // Armor selection
    "BloodType": "Warrior",                 // Blood type
    "Consumables": ["blood_rose_potion", "physical_brew"], // Starting items
    "Enabled": true,                        // Loadout availability
    "Default": true,                        // Default loadout status
    "status": [],                           // Status effects
    "spell": "bear"                         // Associated spell
  }
  // ... additional loadouts (20+ available)
]
```

### Build Configuration

#### Character Builds
```json
"Builds": [
  {
    "Name": "warrior",                      // Build identifier
    "Description": "Warrior Build - Greatsword with Chaos Physical Power",
    "Weapon": "greatsword",                 // Primary weapon
    "WeaponMods": "c4",                     // Weapon modifiers
    "ArmorSet": "warrior",                  // Armor selection
    "BloodType": "Warrior",                 // Blood type
    "Enabled": true,                        // Build availability
    "Default": false,                       // Default build status
    "status": [],                           // Status effects
    "spell": "bear"                         // Associated spell
  }
  // ... additional builds
]
```

### Blood Type Configuration

#### Available Blood Types
```json
"BloodTypes": [
  {
    "Name": "Warrior",                      // Blood type name
    "Guid": -516976528,                     // Blood type PrefabGUID
    "Enabled": true,                        // Blood type availability
    "Default": true,                        // Default blood type status
    "status": [],                           // Status effects
    "spell": "rage"                         // Associated spell
  }
  // ... additional blood types
]
```

### Automation Settings

#### Snapshot Configuration
- **Data Directory**: `Data/AutomationSystem/Snapshots/` (configurable)
- **Persistence Interval**: 60 seconds (range: 10-300 seconds)
- **Max Backups**: 5 (configurable, 1-20 range)
- **Auto Persistence**: Enabled by default
- **Compression**: Optional data compression (reduces storage by ~30%)
- **Integrity Checks**: Automatic corruption detection

#### Performance Settings
- **Memory Limit**: 512MB per player snapshot (configurable)
- **Cleanup Interval**: 24 hours (automatic old backup removal)
- **Concurrent Operations**: Max 3 simultaneous snapshots

### Advanced Configuration

#### Custom GUIDs
The `ExtractedGuids` section contains all game item GUIDs:
```json
"ExtractedGuids": {
  "SpellSchools": [/* Spell school definitions */],
  "StatMods": [/* Stat modifier definitions */],
  "BloodTypes": [/* Blood type GUIDs */],
  "Weapons": [/* Weapon GUID mappings */],
  "ArmorSets": [/* Armor set GUID mappings */],
  "Consumables": [/* Consumable item definitions */],
  "Abilities": [/* Special ability GUIDs */],
  "Buffs": [/* Buff effect definitions */],
  "Emotes": [/* Emote GUIDs */]
}
```

#### Configuration Validation
- All configurations are validated on load
- Invalid GUIDs are logged with warnings
- Missing required fields use defaults
- Schema validation prevents corruption

#### Dynamic Reloading
- Use `.arena_reload` to apply configuration changes
- Most settings take effect immediately
- Some changes require server restart (marked in tooltips)
- Configuration errors are logged to console

## Usage

### Quick Start Guide

1. **Server Setup**: Install BepInEx and AutomationSystem plugin
2. **Configuration**: Edit `arena_config.json` with your desired settings
3. **Start Server**: Launch with plugin loaded
4. **Test Commands**: Use `.status` to verify functionality
5. **Arena Setup**: Configure entry/exit points and zones

### Basic Arena Flow

#### Entering the Arena
```
.pe
```
**What happens:**
- ✅ Creates automatic snapshot of current player state
- ✅ Teleports player to arena entry point
- ✅ Unlocks all VBlood bosses and achievements
- ✅ Sets blood types to maximum (Warrior/Rogue 100%)
- ✅ Applies `[arena]` name tag
- ✅ Clears inventory (configurable)
- ✅ Grants temporary buffs and abilities

#### Arena Gameplay
**Available Commands:**
```
.lw                    // Legendary Warrior: Full heal + buff reset
.art                   // Ancient Ritual Technique: Ultimate revive + 30s god mode
.gear warrior          // Apply warrior gear set
.loadout berserker     // Apply berserker loadout
.build mage            // Switch to mage build
.status                // Check current arena status
```

**Arena Features:**
- No durability loss on weapons
- Blocked overpowered abilities (configurable)
- Restricted healing potions (configurable)
- Blood loss prevention (configurable)

#### Exiting the Arena
```
.px
```
**What happens:**
- ✅ Restores player from snapshot
- ✅ Teleports to exit point
- ✅ Removes arena name tag
- ✅ Clears arena-specific inventory
- ✅ Restores original progression state

### Advanced Usage Examples

#### Loadout Management
```bash
# Apply pre-configured loadouts
.loadout berserker     # Chaos Greatsword + Warrior armor
.loadout assassin      # Illusion Daggers + Rogue armor
.loadout frost_mage    # Frost Spear + Scholar armor
.loadout tank          # Unholy Mace + Brute armor

# Use gear command for quick armor swaps
.gear warrior          # Dracula's Warrior Set
.gear rogue           # Dracula's Rogue Set
.gear scholar         # Dracula's Scholar Set
.gear brute           # Dracula's Brute Set

# Build commands for complete character changes
.build warrior         # Full warrior build
.build rogue          # Full rogue build
.build mage           # Full mage build
.build tank           # Full tank build
```

#### Admin Arena Management
```bash
# Configure arena zones
.arena_setzone pvp 100              # Create 100m radius PvP zone
.arena_setzone training 50          # Create 50m radius training zone

# Set teleport points
.arena_setentry 100 50 200          # Set entry point coordinates
.arena_setexit 300 50 400           # Set exit point coordinates
.arena_setspawn                      # Set spawn point at current location

# Player management
.revive PlayerName                  # Revive specific player
.clear 20                           # Clear items in 20m radius
.give -774462329 5                 # Give 5 Sanguine Swords
.equip PlayerName -774462329       # Equip player with sword

# System management
.arena_reload                       # Reload configuration
.arena_save                         # Save current config
.arena_clear                        # Clear all snapshots
```

#### Item Management Examples
```bash
# Weapons by GUID
.give -774462329 1                  # Sanguine Sword
.give -2044057823 1                 # Sanguine Axe
.give -1569279652 1                 # Sanguine Mace
.give 1532449451 1                  # Sanguine Spear

# Armor sets
.give 1392314162 1                  # Warrior Chest
.give 205207385 1                   # Warrior Legs
.give -382349289 1                  # Warrior Boots
.give 1982551454 1                  # Warrior Gloves

# Consumables
.give 828432508 10                  # Blood Rose Potions
.give 1223264867 5                  # Exquisite Brew
.give -1568756102 3                 # Physical Brew
```

#### Coordinate and Navigation
```bash
# Set custom teleport points
.setpoint home                      # Save current location as 'home'
.tp home                           # Teleport to saved point

# Coordinate-based teleportation
.tp 1000 50 500                    # Teleport to coordinates
.tp -500 0 -500                    # Teleport to arena

# Status and information
.status                             # Show player/arena status
.status full                        # Detailed status information
```

### Automated Workflows

#### Tournament Setup
```bash
# 1. Configure tournament arena
.arena_setzone tournament 150
.arena_setentry 0 50 0
.arena_setexit 500 50 500

# 2. Prepare loadouts
# Edit arena_config.json to enable tournament loadouts

# 3. Start tournament
.arena_reload

# 4. Players join with .pe
# 5. Monitor with .status
```

#### Training Server Setup
```bash
# 1. Create training zones
.arena_setzone training 75
.arena_setzone practice 25

# 2. Configure safe settings
# Edit config: NoBloodLoss=true, BlockOPAbilities=false

# 3. Set up loadout stations
# Players can use .loadout commands to try different builds
```

#### PvP League Configuration
```bash
# 1. Configure competitive settings
# Edit config: MaxPlayers=8, NoDurabilityLoss=false

# 2. Set up multiple arenas
.arena_setzone arena1 100
.arena_setzone arena2 100

# 3. Enable ranking system
# Configure snapshot persistence for match recording
```

### Best Practices

#### Server Administration
- **Regular Backups**: Use `.arena_save` before major changes
- **Monitor Performance**: Check logs for performance warnings
- **Update Regularly**: Keep BepInEx and plugin updated
- **Test Changes**: Use test server for configuration changes

#### Player Experience
- **Clear Communication**: Announce arena events and rules
- **Fair Play**: Monitor for exploits using admin tools
- **Loadout Balance**: Regularly review and balance loadouts
- **Feedback Loop**: Listen to player suggestions for improvements

#### Troubleshooting Common Issues
- **Stuck Players**: Use `.revive PlayerName` and `.tp` commands
- **Lost Items**: Check snapshots and restore if needed
- **Command Failures**: Verify permissions and configuration
- **Performance Issues**: Reduce snapshot frequency or enable compression

## Command Reference

### Player Commands

#### Arena Navigation
| Command | Parameters | Description | Examples |
|---------|------------|-------------|----------|
| `.pe` | None | Enter arena with full state preservation | `.pe` |
| `.px` | None | Exit arena and restore original state | `.px` |
| `.tp` | `<x> <y> <z>` or `<point_name>` | Teleport to coordinates or saved point | `.tp 1000 50 500`<br>`.tp home` |
| `.setpoint` | `<name>` | Save current location as teleport point | `.setpoint base` |
| `.status` | `[full]` | Show arena/player status information | `.status`<br>`.status full` |

#### Combat & Loadouts
| Command | Parameters | Description | Examples |
|---------|------------|-------------|----------|
| `.lw` | None | Legendary Warrior: Full heal + buff reset | `.lw` |
| `.art` | None | Ancient Ritual Technique: Ultimate revive + god mode | `.art` |
| `.gear` | `<type>` | Apply armor set (warrior/rogue/scholar/brute) | `.gear warrior`<br>`.gear rogue` |
| `.loadout` | `<name>` | Apply complete loadout with weapon and gear | `.loadout berserker`<br>`.loadout assassin` |
| `.build` | `<name>` | Switch to complete character build | `.build mage`<br>`.build tank` |

#### Utilities
| Command | Parameters | Description | Examples |
|---------|------------|-------------|----------|
| `.give` | `<guid> [quantity]` | Give item to yourself by GUID | `.give -774462329 5` |
| `.equip` | `<player> <guid>` | Equip player with specific item | `.equip PlayerName -774462329` |
| `.clear` | `[radius]` | Clear items in radius (default 10m) | `.clear 20` |
| `.revive` | `[player]` | Revive yourself or another player | `.revive`<br>`.revive PlayerName` |

### Admin Commands

#### Arena Management
| Command | Parameters | Description | Examples |
|---------|------------|-------------|----------|
| `.arena_setzone` | `<name> <radius>` | Create/configure arena zone | `.arena_setzone pvp 100` |
| `.arena_setentry` | `<x> <y> <z>` | Set arena entry point coordinates | `.arena_setentry 100 50 200` |
| `.arena_setexit` | `<x> <y> <z>` | Set arena exit point coordinates | `.arena_setexit 300 50 400` |
| `.arena_setspawn` | None | Set spawn point at current location | `.arena_setspawn` |
| `.arena_reload` | None | Reload configuration from files | `.arena_reload` |
| `.arena_save` | None | Save current configuration | `.arena_save` |
| `.arena_clear` | None | Clear all player snapshots | `.arena_clear` |

#### Player Management
| Command | Parameters | Description | Examples |
|---------|------------|-------------|----------|
| `.give` | `<guid> <quantity> [player]` | Give item to player (admin version) | `.give -774462329 1 PlayerName` |
| `.revive` | `<player>` | Revive specific player | `.revive PlayerName` |
| `.clear` | `<radius>` | Clear all items in radius | `.clear 50` |
| `.kill` | `<type>` | Kill entities by type | `.kill f` (Fallen)<br>`.kill v` (VBlood) |

#### System Management
| Command | Parameters | Description | Examples |
|---------|------------|-------------|----------|
| `.sys_status` | None | Show system performance stats | `.sys_status` |
| `.sys_restart` | None | Restart automation systems | `.sys_restart` |
| `.sys_backup` | None | Force immediate backup | `.sys_backup` |

### Command Parameters Guide

#### Item GUIDs (Common)
| Item | GUID | Description |
|------|------|-------------|
| Sanguine Sword | -774462329 | Basic melee weapon |
| Sanguine Axe | -2044057823 | Chaos physical power weapon |
| Sanguine Mace | -1569279652 | Unholy weapon |
| Sanguine Spear | 1532449451 | Frost spell power weapon |
| Sanguine Greatsword | 147836723 | Heavy melee weapon |
| Sanguine Crossbow | 1389040540 | Ranged weapon |
| Sanguine Daggers | 1031107636 | Fast attack weapon |
| Sanguine Pistols | 1651523865 | Dual pistols |

#### Armor GUIDs (Dracula Sets)
| Armor Piece | Warrior | Rogue | Scholar | Brute |
|-------------|---------|-------|---------|-------|
| Chest | 1392314162 | 933057100 | 114259912 | 1033753207 |
| Legs | 205207385 | -345596442 | 1592149279 | 993033515 |
| Boots | -382349289 | 1855323424 | 1531721602 | 1646489863 |
| Gloves | 1982551454 | -1826382550 | -1899539896 | 1039083725 |

#### Consumables
| Item | GUID | Description |
|------|------|-------------|
| Blood Rose Potion | 828432508 | Basic healing potion |
| Exquisite Brew | 1223264867 | High-tier potion |
| Physical Brew | -1568756102 | Physical power buff |
| Spell Brew | 1510182325 | Spell power buff |

### Permission System

#### Access Levels
- **Player**: Basic arena commands (`.pe`, `.px`, `.status`)
- **Arena**: Combat commands when in arena (`.lw`, `.art`, `.gear`, `.loadout`)
- **Admin**: Full administrative access to all commands
- **Moderator**: Limited admin access (player management, basic admin tools)

#### Permission Configuration
Permissions are managed through V Rising's built-in permission system:
```json
{
  "Commands": {
    "AutomationSystem.Admin": ["admin", "moderator"],
    "AutomationSystem.Player": ["player", "admin", "moderator"]
  }
}
```

### Command Cooldowns & Limits

#### Player Command Limits
- **`.pe`**: No cooldown (arena entry)
- **`.px`**: No cooldown (arena exit)
- **`.lw`**: 30 seconds cooldown
- **`.art`**: 5 minutes cooldown
- **`.tp`**: 10 seconds cooldown
- **`.give`**: 60 seconds cooldown (self-give)

#### Admin Command Limits
- **`.revive`**: No cooldown
- **`.clear`**: 30 seconds cooldown
- **`.give`**: No cooldown (admin give)
- **System commands**: No cooldown

### Error Messages & Troubleshooting

#### Common Error Responses
- `"Command not found"`: Plugin not loaded or command misspelled
- `"Insufficient permissions"`: Check user permissions
- `"Player not found"`: Verify player name spelling
- `"Invalid coordinates"`: Check coordinate format (x y z)
- `"Cooldown active"`: Wait for cooldown to expire
- `"Arena not configured"`: Set up arena points first

## Development

### Building
```bash
dotnet build AutomationSystem.csproj --configuration Release
```

### Dependencies
- .NET 8.0
- BepInEx 5.4+
- VRising.Unhollowed.Client 1.1.9+
- VampireCommandFramework 0.10.4+

### Project Structure
```
AutomationSystem/
├── Plugin.cs                 # Main plugin entry point
├── Core/                     # Core systems and services
├── Services/                 # Business logic services
├── Automation/               # Automation commands and logic
├── Snapshots/               # Player snapshot management
├── Data/                    # Game data and configurations
├── Models/                  # Data models and DTOs
├── Utils/                   # Utility classes
└── Helpers/                 # Helper functions
```

## Troubleshooting

### Common Issues

**Plugin not loading**:
- Check BepInEx installation
- Verify .NET 8.0 runtime
- Check server logs for errors

**Commands not working**:
- Ensure VampireCommandFramework is loaded
- Check player permissions
- Verify arena configuration

**Snapshots not saving**:
- Check file permissions on Data directory
- Verify disk space
- Check persistence interval settings

### Logs
- Server logs: `BepInEx/LogOutput.log`
- Plugin logs: Check console for AutomationSystem messages
- Config errors: Check `config/AutomationSystem/` files

## FAQ

### General Questions

**Q: What is AutomationSystem?**
A: AutomationSystem is a comprehensive plugin for V Rising servers that provides automated arena management, player snapshots, and extensive admin tools to enhance gameplay and server administration.

**Q: Is this plugin compatible with other V Rising plugins?**
A: Yes, AutomationSystem is designed to be compatible with most other plugins. It uses standard BepInEx integration and doesn't modify core game files.

**Q: Does this plugin work on dedicated servers?**
A: Yes, AutomationSystem is specifically designed for dedicated V Rising servers and requires BepInEx to function.

**Q: Can I use this on a local/single-player world?**
A: While technically possible, the plugin is optimized for multiplayer server environments. Single-player usage may have limited functionality.

### Installation & Setup

**Q: The plugin doesn't load. What should I check?**
A: Check these common issues:
- Ensure BepInEx is properly installed and the correct version
- Verify .NET 8.0 runtime is installed
- Check file permissions on the plugin directory
- Review `BepInEx/LogOutput.log` for specific error messages

**Q: Commands aren't working. What's wrong?**
A: Common causes:
- Plugin not loaded (check logs)
- Insufficient permissions (verify user roles)
- Commands typed incorrectly (check syntax)
- VampireCommandFramework not loaded

**Q: How do I configure the arena points?**
A: Use admin commands:
```
.arena_setentry <x> <y> <z>    # Set entry coordinates
.arena_setexit <x> <y> <z>     # Set exit coordinates
.arena_setspawn                 # Set spawn at current location
```

### Arena & Gameplay

**Q: What happens when players enter the arena?**
A: Players get:
- Automatic snapshot of their current state
- Teleported to arena entry point
- All VBlood bosses unlocked
- Max blood types (Warrior/Rogue 100%)
- `[arena]` name tag
- Temporary buffs and abilities

**Q: Can players lose their progress in the arena?**
A: No, the snapshot system automatically saves player state before arena entry and restores it upon exit. All progress, items, and achievements are preserved.

**Q: How do loadouts work?**
A: Loadouts are pre-configured combinations of weapons, armor, blood types, and consumables. Players can apply them using `.loadout <name>` or `.build <name>` commands.

**Q: Are there cooldowns on abilities?**
A: Yes:
- `.lw` (Legendary Warrior): 30 seconds
- `.art` (Ancient Ritual Technique): 5 minutes
- `.tp` (Teleport): 10 seconds
- `.give` (self-give): 60 seconds

### Configuration

**Q: How do I change configuration without restarting?**
A: Edit the JSON files in `BepInEx/config/AutomationSystem/`, then use `.arena_reload` to apply changes immediately.

**Q: What do the weapon modifier codes mean?**
A: Weapon modifiers enhance weapons with spell effects:
- `c` = Chaos (physical damage/power)
- `f` = Frost (ice/slow effects)
- `u` = Unholy (necrotic/poison)
- `b` = Blood (lifesteal/healing)
- `i` = Illusion (stealth/speed)
- `s` = Storm (lightning/teleport)

**Q: Can I disable certain features?**
A: Yes, most features can be disabled in `arena_config.json`:
- Set `"ArenaEnabled": false` to disable arena system
- Set `"NoDurabilityLoss": true` to prevent weapon degradation
- Configure `"BlockOPAbilities": false` to allow overpowered abilities

### Performance & Technical

**Q: How much server resources does this use?**
A: Typical usage:
- RAM: 50-200MB additional (depends on player count and snapshot frequency)
- CPU: Minimal impact (<1% additional load)
- Storage: ~10MB per player for snapshots (configurable)

**Q: How often are snapshots saved?**
A: Configurable from 10-300 seconds (default 60 seconds). More frequent snapshots use more resources but provide better data protection.

**Q: What happens if the server crashes?**
A: Player progress is automatically restored from the last snapshot when they reconnect. No progress is lost.

**Q: Can I backup/restore player data?**
A: Yes, snapshots are automatically managed. Use `.arena_clear` to reset all snapshots if needed.

### Troubleshooting

**Q: Players are getting stuck after arena exit**
A: Use `.revive PlayerName` and `.tp PlayerName <coordinates>` to fix stuck players.

**Q: Items are disappearing**
A: Check snapshot integrity. Items should be preserved in snapshots. Use `.give` commands to restore lost items.

**Q: Commands are slow or laggy**
A: Reduce snapshot frequency in config or enable compression. Check server performance with `.sys_status`.

**Q: Configuration changes aren't applying**
A: Use `.arena_reload` after editing files. Some changes require server restart (noted in configuration).

### Development & Support

**Q: Can I contribute to the project?**
A: Yes! Fork the repository, make your changes, test thoroughly, and submit a pull request. Follow the code standards in the Contributing section.

**Q: Where can I get help?**
A: Check the documentation first, then:
- GitHub Issues for bugs
- GitHub Discussions for questions
- Discord community server for real-time help

**Q: Is there a roadmap for future features?**
A: Check GitHub Issues and Discussions for planned features. Community suggestions are welcome!

**Q: How do I report bugs?**
A: Use GitHub Issues with:
- Detailed description of the problem
- Steps to reproduce
- Server logs (`BepInEx/LogOutput.log`)
- Your configuration files
- V Rising server version

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

- **Issues**: [GitHub Issues](https://github.com/Coyoteq1/AutomationSystem/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Coyoteq1/AutomationSystem/discussions)
- **Discord**: Join our community server

## Changelog

### v1.2.0 (Lifecycle Cleanup)
- **Removed VRisingCore Lifecycle Management**: Eliminated dependency on VRisingCore shared component
- **Simplified Plugin Architecture**: Direct integration with VRising game systems
- **Removed Plugin Management Layer**: Streamlined service initialization and lifecycle
- **Updated Dependencies**: Removed VRising.Unhollowed.Client lifecycle wrapper

### v1.1.0 (Documentation Update)
- **Major Documentation Overhaul**: Comprehensive README expansion with detailed guides
- **Enhanced Installation Guide**: Step-by-step setup with troubleshooting
- **Complete Configuration Reference**: All config options with examples and explanations
- **Advanced Usage Examples**: Workflows for tournaments, training servers, and PvP leagues
- **Detailed Command Reference**: Complete parameter guide with examples and cooldowns
- **FAQ Section**: Common questions and troubleshooting scenarios
- **Performance Guidelines**: Resource usage and optimization tips
- **Developer Resources**: Building instructions and contribution guidelines

### v1.0.0
- Initial release
- Arena system with snapshots
- Basic automation commands
- Admin management tools
- Configuration system

---

**Note**: This plugin is designed for V Rising servers. Ensure compatibility with your server version before installing.
