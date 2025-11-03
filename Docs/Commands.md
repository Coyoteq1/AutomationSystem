# Command Reference

Complete reference for all AutomationSystem plugin commands.

## Table of Contents
- [Arena Commands](#arena-commands)
- [Combat Commands](#combat-commands)
- [Admin Commands](#admin-commands)
- [Utility Commands](#utility-commands)
- [Configuration Commands](#configuration-commands)
- [Debug Commands](#debug-commands)

## Arena Commands

### Enter Commands

#### `.pe` - Portal Execute
**Description**: Auto-enter arena with unlock and snapshot creation
**Usage**: `.pe`
**Access**: Players
**Effects**:
- Creates player snapshot
- Unlocks all VBlood bosses
- Unlocks all achievements
- Sets Warrior/Rogue blood types to 100%
- Adds [arena] tag to player name
- Teleports to arena (no teleport if already in range)

#### `.tp` - Teleport to Arena Exit
**Description**: Teleport to configured arena exit zone
**Usage**: `.tp`
**Access**: Players
**Requirements**: Arena exit point configured

#### `.setpoint` - Set Enter Point
**Description**: Set arena enter point at current location
**Usage**: `.setpoint`
**Access**: Admin only
**Effects**: Updates arena_config.json with current coordinates

### Exit Commands

#### `.px` - Portal Exit
**Description**: Restore progression and exit arena
**Usage**: `.px`
**Access**: Players
**Requirements**: Must be in arena
**Effects**:
- Restores player snapshot
- Clears arena inventory
- Restores blood type and name
- Removes [arena] name tag

#### `.setpoint` - Set Exit Point
**Description**: Set arena exit point at current location
**Usage**: `.setpoint`
**Access**: Admin only
**Effects**: Updates arena_config.json with current coordinates

### General Arena Commands

#### `.status` - Arena Status
**Description**: Check comprehensive arena and player status
**Usage**: `.status`
**Access**: All players
**Displays**:
- Arena status (available/in arena)
- Current position
- Steam ID
- Spawn point status

#### `.leave` - Leave Arena
**Description**: Exit arena and restore progression
**Usage**: `.leave`
**Access**: Players
**Requirements**: Must be in arena
**Effects**: Same as `.px` but with confirmation

## Combat Commands

### Healing & Buffs

#### `.lw` - Legendary Warrior
**Description**: Full heal and buff reset
**Usage**: `.lw`
**Access**: Arena only
**Effects**:
- Restores full health
- Clears all buffs/debuffs
- Resets ability cooldowns

#### `.art` - Ancient Ritual Technique
**Description**: Ultimate revive with god mode
**Usage**: `.art`
**Access**: Arena only
**Effects**:
- Full health restoration
- Clear all buffs/debuffs
- Reset all ability cooldowns
- 30-second god mode buff

### Gear & Loadouts

#### `.gear <type>` - Apply Gear Loadout
**Description**: Apply predefined gear set
**Usage**: `.gear <warrior|mage|default>`
**Access**: Arena only
**Parameters**:
- `warrior` - Melee weapons, heavy armor
- `mage` - Spell weapons, light armor
- `default` - Balanced gear

#### `.loadout <name>` - Apply Custom Loadout
**Description**: Apply custom loadout by name
**Usage**: `.loadout <loadout_name>`
**Access**: Arena only
**Examples**:
- `.loadout warrior`
- `.loadout mage`
- `.loadout custom`

#### `.build <1-4>` - Apply Build Preset
**Description**: Apply numbered build preset
**Usage**: `.build <1|2|3|4>`
**Access**: Players
**Effects**: Applies predefined build configuration

## Admin Commands

### Player Management

#### `.give <guid> <quantity>` - Give Item
**Description**: Give item to player by PrefabGUID
**Usage**: `.give <prefab_guid> <quantity>`
**Access**: Admin only
**Examples**:
- `.give -774462329 1` (Legendary Sword)
- `.give 123456789 5` (Consumables)

#### `.revive <player>` - Revive Player
**Description**: Revive a player character
**Usage**: `.revive <player_name>` (optional)
**Access**: Admin only
**Effects**:
- Restores full health
- Removes death buffs
- Works on self if no name provided

#### `.unlock <player>` - Unlock Player
**Description**: Unlock all research, VBloods, and achievements
**Usage**: `.unlock <player_name>` (optional)
**Access**: Admin only
**Effects**: Grants complete progression

### World Management

#### `.clear <radius>` - Clear Dropped Items
**Description**: Clear all dropped items in radius
**Usage**: `.clear <radius>`
**Access**: Admin only
**Default**: 10 meters
**Effects**: Removes dropped items, preserves equipped gear

#### `.clearall` - Clear All Dropped Items
**Description**: Clear all dropped items in world
**Usage**: `.clearall`
**Access**: Admin only
**Effects**: Removes all dropped items globally

#### `.kill <type>` - Kill Entities
**Description**: Kill entities by type
**Usage**: `.kill f` (Fallen)
**Access**: Admin only
**Types**:
- `f` - Fallen entities

### Arena Setup

#### `.arena_setzone <radius>` - Set Arena Zone
**Description**: Set arena zone with radius
**Usage**: `.arena_setzone <name> <radius>`
**Access**: Admin only
**Default name**: "default"

#### `.arena_setentry <radius>` - Set Entry Point
**Description**: Set arena entry point and radius
**Usage**: `.arena_setentry <radius>`
**Access**: Admin only
**Default radius**: 10

#### `.arena_setexit <radius>` - Set Exit Point
**Description**: Set arena exit point and radius
**Usage**: `.arena_setexit <radius>`
**Access**: Admin only
**Default radius**: 10

#### `.arena_setspawn` - Set Spawn Point
**Description**: Set arena spawn point at current location
**Usage**: `.arena_setspawn`
**Access**: Admin only

### Territory Management

#### `.arena_setterritory <grid_index> <p1> <p2> <p3> <p4>` - Set Territory
**Description**: Set arena territory using saved points
**Usage**: `.arena_setterritory <grid_index> <point1> <point2> <point3> <point4>`
**Access**: Admin only
**Requirements**: Points must be saved first with `.sc s p <1-4>`

#### `.arena_listterritories` - List Territories
**Description**: List all configured territories
**Usage**: `.arena_listterritories`
**Access**: Admin only

#### `.sc s p <1-4>` - Save Coordinate Point
**Description**: Save current location as coordinate point
**Usage**: `.sc save point <1-4>`
**Access**: Admin only
**Effects**: Stores point for territory creation

### Boss Management

#### `.lockboss <boss_guid>` - Lock Boss
**Description**: Lock boss from spawning
**Usage**: `.lockboss <prefab_guid>`
**Access**: Admin only

#### `.unlockboss <boss_guid>` - Unlock Boss
**Description**: Allow boss to spawn
**Usage**: `.unlockboss <prefab_guid>`
**Access**: Admin only

#### `.listlockedbosses` - List Locked Bosses
**Description**: Show all currently locked bosses
**Usage**: `.listlockedbosses`
**Access**: All players

## Utility Commands

### Equipment

#### `.equip <guid>` - Equip Item
**Description**: Equip item from inventory
**Usage**: `.equip <prefab_guid>`
**Access**: Players

### Inventory

#### `.additem <guid> <quantity>` - Add Item
**Description**: Add item to inventory
**Usage**: `.additem <prefab_guid> <quantity>`
**Access**: Admin only
**Alias**: `.add`

### Legacy Commands

#### `.arena_enter` - Enter Arena (Legacy)
**Description**: Legacy arena entry command
**Usage**: `.arena_enter`
**Access**: Players
**Note**: Use `.pe` instead

#### `.arena_exit` - Exit Arena (Legacy)
**Description**: Legacy arena exit command
**Usage**: `.arena_exit`
**Access**: Players
**Note**: Use `.px` instead

## Configuration Commands

### Config Management

#### `.arena_reload` - Reload Configuration
**Description**: Reload arena configuration from files
**Usage**: `.arena_reload`
**Access**: Admin only
**Effects**:
- Validates new configuration
- Applies changes to running services
- Preserves existing snapshots

#### `.arena_save` - Save Configuration
**Description**: Save current configuration to files
**Usage**: `.arena_save`
**Access**: Admin only

#### `.arena_clear` - Clear Snapshots
**Description**: Clear all arena snapshots
**Usage**: `.arena_clear`
**Access**: Admin only
**Warning**: Irreversible operation

### Prefab Management

#### `.arena_add <category> <name> <guid>` - Add Prefab
**Description**: Add item to prefab database
**Usage**: `.arena_add <category> <name> <guid>`
**Access**: Admin only
**Examples**:
- `.arena_add weapons "Legendary Sword" -774462329`
- `.arena_add armor "Warrior Armor" -123456789`

#### `.arena_import <json>` - Import Prefabs
**Description**: Import prefabs from JSON data
**Usage**: `.arena_import <json_string>`
**Access**: Admin only

#### `.arena_export` - Export Prefabs
**Description**: Export all prefabs to JSON
**Usage**: `.arena_export`
**Access**: Admin only
**Effects**: Logs JSON data to console

## Debug Commands

### Status & Information

#### `.arena_status` - Arena System Status
**Description**: Show comprehensive arena system status
**Usage**: `.arena_status`
**Access**: All players
**Displays**:
- Service status
- Active players
- Loaded builds
- Configuration summary

#### `.arena_stats` - Arena Statistics
**Description**: Show arena statistics
**Usage**: `.arena_stats`
**Access**: All players
**Displays**:
- Arena status
- Player status
- Active snapshots
- Available commands

#### `.arena_help <category>` - Command Help
**Description**: Show help for command categories
**Usage**: `.arena_help <category>`
**Access**: All players
**Categories**:
- `setup` - Setup commands
- `player` - Player commands
- `pvp` - PvP commands
- `admin` - Admin commands
- `info` - Information commands

### Testing Commands

#### `.swap` - Swap to Secondary Character
**Description**: Manually swap to secondary character
**Usage**: `.swap`
**Access**: Admin only
**Requirements**: Dual character setup

#### `.swapback` - Swap to Primary Character
**Description**: Manually swap back to primary character
**Usage**: `.swapback`
**Access**: Admin only

### Coordinate Commands

#### `.savepoint <type>` - Save Current Location
**Description**: Save current location as point
**Usage**: `.savepoint <type>`
**Access**: Admin only
**Types**: Various point types for arena setup

## Command Groups

Commands are organized into logical groups:

### Enter Group
- `.pe` - Portal Execute
- `.tp` - Teleport to exit
- `.setpoint` - Set enter point

### Exit Group
- `.px` - Portal Exit
- `.setpoint` - Set exit point

### Combat Group
- `.lw` - Legendary Warrior
- `.art` - Ancient Ritual Technique
- `.gear` - Apply gear
- `.loadout` - Apply loadout

### Admin Group
- `.give` - Give items
- `.revive` - Revive players
- `.clear` - Clear items
- `.kill` - Kill entities

## Command Permissions

### Access Levels

| Level | Description | Commands |
|-------|-------------|----------|
| **Players** | All users | `.pe`, `.px`, `.tp`, `.status`, `.lw`, `.art`, `.gear`, `.loadout` |
| **Admins** | Server admins | All commands marked `adminOnly: true` |
| **Arena Only** | In-arena only | `.lw`, `.art`, `.gear`, `.loadout` |

### Permission Checks

- Commands validate admin status using `ctx.IsAdmin`
- Arena-only commands check `SnapshotManagerService.IsInArena()`
- Entity validation prevents operations on invalid targets

## Error Handling

### Common Error Messages

| Error | Cause | Solution |
|-------|-------|----------|
| `Character not found!` | Player entity not found | Reconnect to server |
| `Already in arena!` | Attempting to enter while in arena | Use `.px` to exit first |
| `Not in arena!` | Using arena-only command outside | Enter arena with `.pe` |
| `Invalid item name or PrefabGUID` | Invalid item identifier | Check GUID format |
| `Config not available` | Configuration not loaded | Check config files |

### Error Response Format

```csharp
ctx.Error("Error message here");
// Displays in red text to player
```

### Success Response Format

```csharp
ctx.Reply("Success message here");
// Displays in default color to player
```

## Command Logging

All commands are logged with the following information:

- Player name and Steam ID
- Command executed
- Timestamp
- Success/failure status
- Admin status

Logs are written to:
- Server console
- `BepInEx/LogOutput.log`
- Plugin-specific logs

## Best Practices

### For Players

1. **Use appropriate commands**: Arena commands only work in arena
2. **Check status**: Use `.status` to verify arena state
3. **Backup important items**: Arena clears inventory on exit
4. **Coordinate with admins**: For complex operations

### For Admins

1. **Test commands**: Use test server for new configurations
2. **Backup configurations**: Before making changes
3. **Monitor logs**: Check for command abuse
4. **Validate coordinates**: Ensure points are in valid locations

### Command Usage Tips

1. **Batch operations**: Group related commands together
2. **Error checking**: Always check command responses
3. **Documentation**: Keep command references handy
4. **Training**: Train players on available commands

## Troubleshooting Commands

### Commands Not Working

**Check permissions**:
- Ensure admin status for admin commands
- Verify arena status for arena commands

**Validate syntax**:
- Check command format and parameters
- Use `.arena_help` for syntax help

**Check configuration**:
- Ensure arena points are set
- Verify loadouts are configured

### Common Issues

**Commands not recognized**:
- VampireCommandFramework not loaded
- Plugin not initialized
- Command prefix incorrect

**Arena commands failing**:
- Arena not configured
- Player not in correct state
- Configuration errors

**Item commands failing**:
- Invalid PrefabGUID
- Item not in database
- Inventory full

### Debug Steps

1. Check server console for errors
2. Verify plugin loaded with `.arena_status`
3. Test with simple commands first
4. Check configuration with `.arena_reload`
5. Review logs for detailed error information
