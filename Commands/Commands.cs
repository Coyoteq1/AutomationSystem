using ProjectM;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;

// namespace CrowbaneArena
namespace AutomationSystem.Commands
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// Legacy Arena Commands implementation
    /// This class is obsolete and will be removed in a future version.
    /// Please use ArenaCommands class instead.
    /// </summary>
    [Obsolete("This class is deprecated. Use ArenaCommands class instead.")]
    public static class LegacyArenaCommands
    {
        private static readonly Dictionary<int, float3> SavedPoints = new();
        private static readonly Dictionary<int, (float3 p1, float3 p2, float3 p3, float3 p4)> Territories = new();

        // ===== ARENA SETUP COMMANDS (Admin Only) =====

        [Command("automationsystem_setzone", description: "Set automation system zone with name and radius", adminOnly: true)]
        public static void SetZone(ICommandContext ctx, string name = "default", float radius = 50f)
        {
            if (radius <= 0)
            {
                ctx.Error("Radius must be greater than 0!");
                return;
            }

            ArenaController.SetZoneRadius(radius);
            ctx.Reply($"Automation system zone '{name}' set with radius {radius}m");
        }

        [Command("automationsystem_setentry", description: "Set entry point and radius", adminOnly: true)]
        public static void SetEntry(ICommandContext ctx, float radius = 10f)
        {
            if (radius <= 0)
            {
                ctx.Error("Entry radius must be greater than 0!");
                return;
            }

            var position = GetPlayerPosition(ctx);
            ArenaController.SetEntryPoint(position, radius);
            ctx.Reply($"Entry point set at ({position.x:F1}, {position.y:F1}, {position.z:F1}) with radius {radius}m");
        }

        [Command("automationsystem_setexit", description: "Set exit point and radius", adminOnly: true)]
        public static void SetExit(ICommandContext ctx, float radius = 10f)
        {
            if (radius <= 0)
            {
                ctx.Error("Exit radius must be greater than 0!");
                return;
            }

            var position = GetPlayerPosition(ctx);
            ArenaController.SetExitPoint(position, radius);
            ctx.Reply($"Exit point set at ({position.x:F1}, {position.y:F1}, {position.z:F1}) with radius {radius}m");
        }

        [Command("automationsystem_setspawn", description: "Set automation system spawn point", adminOnly: true)]
        public static void SetSpawn(ICommandContext ctx)
        {
            var position = GetPlayerPosition(ctx);
            ArenaController.SetSpawnPoint(position);
            ctx.Reply($"Automation system spawn point set at ({position.x:F1}, {position.y:F1}, {position.z:F1})");
        }

        [Command("automationsystem_setterritory", description: "Set index territory grid index using 4 saved corner points", adminOnly: true)]
        public static void SetTerritory(ICommandContext ctx, int gridIndex, int point1, int point2, int point3, int point4)
        {
            if (!SavedPoints.ContainsKey(point1) || !SavedPoints.ContainsKey(point2) || 
                !SavedPoints.ContainsKey(point3) || !SavedPoints.ContainsKey(point4))
            {
                ctx.Error("All 4 points must be saved first! Use .sc s p <1-4>");
                return;
            }

            var p1 = SavedPoints[point1];
            var p2 = SavedPoints[point2];
            var p3 = SavedPoints[point3];
            var p4 = SavedPoints[point4];
            
            Territories[gridIndex] = (p1, p2, p3, p4);
            ArenaTerritory.InitializeArenaGrid();
            
            var action = Territories.Count == 1 ? "created" : "updated";
            ctx.Reply($"✅ Automation system territory grid index {gridIndex} {action} using points {point1}, {point2}, {point3}, {point4}");
            ctx.Reply($"   P{point1}: ({p1.x:F1}, {p1.y:F1}, {p1.z:F1})");
            ctx.Reply($"   P{point2}: ({p2.x:F1}, {p2.y:F1}, {p2.z:F1})");
            ctx.Reply($"   P{point3}: ({p3.x:F1}, {p3.y:F1}, {p3.z:F1})");
            ctx.Reply($"   P{point4}: ({p4.x:F1}, {p4.y:F1}, {p4.z:F1})");
            ctx.Reply($"   Total territories: {Territories.Count}");
        }

        [Command("automationsystem_listterritories", description: "List all configured territories", adminOnly: true)]
        public static void ListTerritories(ICommandContext ctx)
        {
            if (Territories.Count == 0)
            {
                ctx.Reply("⚠️ No territories configured");
                return;
            }

            ctx.Reply($"📍 Configured Territories ({Territories.Count}):");
            foreach (var kvp in Territories.OrderBy(t => t.Key))
            {
                var (p1, p2, p3, p4) = kvp.Value;
                ctx.Reply($"   Grid {kvp.Key}: ({p1.x:F0},{p1.z:F0}) ({p2.x:F0},{p2.z:F0}) ({p3.x:F0},{p3.z:F0}) ({p4.x:F0},{p4.z:F0})");
            }
        }

        [Command("sc", description: "Save coordinate point (1-4)", adminOnly: true)]
        public static void SaveCoordinate(ICommandContext ctx, string action, string type, int pointNumber)
        {
            if (action.ToLower() != "save" && action.ToLower() != "s")
            {
                ctx.Error("Use: .sc save p <1-4> or .sc s p <1-4>");
                return;
            }

            if (type.ToLower() != "p" && type.ToLower() != "point")
            {
                ctx.Error("Use: .sc save p <1-4> or .sc s p <1-4>");
                return;
            }

            if (pointNumber < 1 || pointNumber > 4)
            {
                ctx.Error("Point number must be 1-4");
                return;
            }

            var position = GetPlayerPosition(ctx);
            SavedPoints[pointNumber] = position;
            ctx.Reply($"✅ Saved point {pointNumber} at ({position.x:F1}, {position.y:F1}, {position.z:F1})");
        }

        [Command("automationsystem_reload", description: "Reload automation system configuration", adminOnly: true)]
        public static void ReloadConfig(ICommandContext ctx)
        {
            try
            {
                ArenaConfigLoader.Initialize();
                ctx.Reply("✅ Automation system configuration reloaded successfully");
            }
            catch (Exception ex)
            {
                ctx.Error($"❌ Failed to reload config: {ex.Message}");
            }
        }

        [Command("automationsystem_save", description: "Save automation system configuration", adminOnly: true)]
        public static void SaveConfig(ICommandContext ctx)
        {
            try
            {
                ArenaConfigLoader.SaveConfig();
                ctx.Reply("✅ Automation system configuration saved successfully");
            }
            catch (Exception ex)
            {
                ctx.Error($"❌ Failed to save config: {ex.Message}");
            }
        }

        [Command("automationsystem_clear", adminOnly: true, description: "Clear all automation system snapshots")]
        public static void ClearSnapshots(ICommandContext ctx)
        {
            try
            {
                SnapshotManagerService.ClearAllSnapshots();
                ctx.Reply("✅ All automation system snapshots cleared");
            }
            catch (Exception ex)
            {
                ctx.Error($"❌ Error clearing snapshots: {ex.Message}");
            }
        }

        // ===== PLAYER ARENA COMMANDS =====
        // Legacy arena_enter and arena_exit removed - use .arena join / .arena leave instead

        [Command("automationsystem_status", description: "Check your automation system status")]
        public static void CheckStatus(ICommandContext ctx)
        {
            try
            {
                var characterEntity = GetPlayerEntity(ctx);
                if (characterEntity.Equals(Entity.Null))
                {
                    ctx.Error("Could not find your player entity!");
                    return;
                }

                var playerCharacter = VRisingCore.EntityManager.GetComponentData<PlayerCharacter>(characterEntity);
                var userEntity = playerCharacter.UserEntity;

                if (userEntity == Entity.Null)
                {
                    ctx.Error("❌ Error: Could not find user entity.");
                    return;
                }

                var steamId = PlayerService.GetSteamId(userEntity);
                var playerName = PlayerService.GetPlayerName(userEntity);
                var isInArena = SnapshotManagerService.IsInArena(steamId);
                var position = PlayerService.GetPlayerPosition(characterEntity);

                ctx.Reply($"📊 Automation system Status for {playerName}:");
                ctx.Reply($"   • In Automation System: {(isInArena ? "✅ Yes" : "❌ No")}");
                ctx.Reply($"   • Position: {position.x:F1}, {position.y:F1}, {position.z:F1}");
                ctx.Reply($"   • Steam ID: {steamId}");

                if (ZoneManager.SpawnPoint.Equals(float3.zero))
                {
                    ctx.Reply($"   • ⚠️ Automation system spawn point not configured!");
                }
                else
                {
                    ctx.Reply(
                        $"   • Spawn Point: {ZoneManager.SpawnPoint.x:F1}, {ZoneManager.SpawnPoint.y:F1}, {ZoneManager.SpawnPoint.z:F1}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in CheckStatus command: {ex.Message}");
                ctx.Error("❌ An error occurred while checking status.");
            }
        }

        [Command("tp", description: "Teleport to zone (arena/out)")]
        public static void Teleport(ICommandContext ctx, string zone = "arena")
        {
            try
            {
                var player = GetPlayerEntity(ctx);
                if (player.Equals(Entity.Null))
                {
                    ctx.Error("Could not find your player entity!");
                    return;
                }

                var position = zone.ToLower() == "out" 
                    ? ArenaController.GetExitPoint()
                    : ArenaController.GetEntryPoint();
                    
                if (position.Equals(float3.zero))
                {
                    ctx.Error($"{zone} point not set!");
                    return;
                }

                if (VRisingCore.EntityManager.HasComponent<Translation>(player))
                {
                    var translation = VRisingCore.EntityManager.GetComponentData<Translation>(player);
                    translation.Value = position;
                    VRisingCore.EntityManager.SetComponentData(player, translation);
                    ctx.Reply($"✅ Teleported to {zone} ({position.x:F1}, {position.y:F1}, {position.z:F1})");
                }
                else
                {
                    ctx.Error("Could not teleport player!");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in Teleport command: {ex.Message}");
                ctx.Error("❌ An error occurred while teleporting.");
            }
        }

        // ===== EQUIPMENT AND LOADOUT COMMANDS =====

        [Command("automationsystem_build", description: "Select a build preset (1-4)")]
        public static void SelectBuild(ICommandContext ctx, int buildNumber = 1)
        {
            try
            {
                if (buildNumber < 1 || buildNumber > 4)
                {
                    ctx.Error("Build number must be between 1 and 4!");
                    return;
                }

                var player = GetPlayerEntity(ctx);
                if (player.Equals(Entity.Null))
                {
                    ctx.Error("Could not find your player entity!");
                    return;
                }

                var buildName = $"build{buildNumber}";
                if (InventoryManagementService.GiveLoadout(player, buildName))
                {
                    ctx.Reply($"✅ Applied build preset {buildNumber}");
                }
                else
                {
                    ctx.Error($"❌ Build preset {buildNumber} not found or failed to apply");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in SelectBuild command: {ex.Message}");
                ctx.Error("❌ An error occurred while applying build.");
            }
        }

        [Command("automationsystem_give", description: "Give an item by name")]
        public static void GiveItem(ICommandContext ctx, string itemName, int amount = 1)
        {
            try
            {
                if (amount <= 0)
                {
                    ctx.Error("Amount must be greater than 0!");
                    return;
                }

                var player = GetPlayerEntity(ctx);
                if (player.Equals(Entity.Null))
                {
                    ctx.Error("Could not find your player entity!");
                    return;
                }

                if (InventoryManagementService.GiveItem(player, itemName, amount))
                {
                    ctx.Reply($"✅ Gave {amount}x {itemName}");
                }
                else
                {
                    ctx.Error($"❌ Could not find item: {itemName}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in GiveItem command: {ex.Message}");
                ctx.Error("❌ An error occurred while giving item.");
            }
        }

        [Command("automationsystem_list", description: "List available items")]
        public static void ListItems(ICommandContext ctx, string category = "")
        {
            try
            {
                var items = InventoryManagementService.GetAvailableItems(category);
                if (items.Count == 0)
                {
                    ctx.Error("No items found!");
                    return;
                }

                ctx.Reply($"📋 Available items{(string.IsNullOrEmpty(category) ? "" : $" in {category}")}:");
                foreach (var item in items.Take(10))
                {
                    ctx.Reply($"   • {item}");
                }

                if (items.Count > 10)
                {
                    ctx.Reply($"   ... and {items.Count - 10} more items");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in ListItems command: {ex.Message}");
                ctx.Error("❌ An error occurred while listing items.");
            }
        }

        [Command("automationsystem_loadout", description: "Apply a loadout by name")]
        public static void ApplyLoadout(ICommandContext ctx, string loadoutName = "default")
        {
            try
            {
                var player = GetPlayerEntity(ctx);
                if (player.Equals(Entity.Null))
                {
                    ctx.Error("Could not find your player entity!");
                    return;
                }

                if (InventoryManagementService.GiveLoadout(player, loadoutName))
                {
                    ctx.Reply($"✅ Applied loadout: {loadoutName}");
                }
                else
                {
                    ctx.Error($"❌ Loadout '{loadoutName}' not found or failed to apply");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in ApplyLoadout command: {ex.Message}");
                ctx.Error("❌ An error occurred while applying loadout.");
            }
        }

        // ===== ADMIN MANAGEMENT COMMANDS =====

        [Command("automationsystem_add", adminOnly: true, description: "Add an item to prefabs")]
        public static void AddPrefab(ICommandContext ctx, string category, string name, string guidStr)
        {
            try
            {
                if (!Guid.TryParse(guidStr, out var guid))
                {
                    ctx.Error("Invalid GUID format!");
                    return;
                }

                InventoryManagementService.AddPrefab(category, name, guid);
                ctx.Reply($"✅ Added {name} to {category} prefabs");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in AddPrefab command: {ex.Message}");
                ctx.Error("❌ An error occurred while adding prefab.");
            }
        }

        [Command("automationsystem_import", adminOnly: true, description: "Import prefabs from JSON")]
        public static void ImportPrefabs(ICommandContext ctx, string jsonData)
        {
            try
            {
                var count = InventoryManagementService.ImportPrefabsFromJson(jsonData);
                ctx.Reply($"✅ Successfully imported {count} prefabs");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in ImportPrefabs command: {ex.Message}");
                ctx.Error($"❌ Failed to import prefabs: {ex.Message}");
            }
        }

        [Command("automationsystem_export", adminOnly: true, description: "Export all prefabs to JSON")]
        public static void ExportPrefabs(ICommandContext ctx)
        {
            try
            {
                var json = InventoryManagementService.ExportPrefabsToJson();
                ctx.Reply($"✅ Exported {json.Length} characters of prefab data");
                Plugin.Logger?.LogInfo($"Exported prefab data: {json}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in ExportPrefabs command: {ex.Message}");
                ctx.Error($"❌ Failed to export prefabs: {ex.Message}");
            }
        }


        // ===== INFORMATION COMMANDS =====

        [Command("automationsystem_help", description: "Show Automation System command help")]
        public static void ShowHelp(ICommandContext ctx, string category = "")
        {
            if (string.IsNullOrEmpty(category))
            {
                ctx.Reply("🏟️ Automation System Commands Help:");
                ctx.Reply("📋 Categories: setup, player, pvp, admin, info");
                ctx.Reply("💡 Use '.automationsystem_help <category>' for specific commands");
                ctx.Reply("🎮 Quick commands: automationsystem_join, automationsystem_leave, automationsystem_status");
                ctx.Reply("⚔️ PvP: .pvp on/off - Toggle PvP mode");
            }
            else
            {
                switch (category.ToLower())
                {
                    case "setup":
                        ctx.Reply("🔧 Setup Commands (Admin Only):");
                        ctx.Reply("   • automationsystem_setzone <radius> - Set automation system zone");
                        ctx.Reply("   • automationsystem_setentry <radius> - Set entry point");
                        ctx.Reply("   • automationsystem_setexit <radius> - Set exit point");
                        ctx.Reply("   • automationsystem_setspawn - Set spawn point");
                        break;
                    case "player":
                        ctx.Reply("🎮 Player Commands:");
                        ctx.Reply("   • automationsystem_join - Join the automation system");
                        ctx.Reply("   • automationsystem_leave - Leave the automation system");
                        ctx.Reply("   • automationsystem_status - Check your status");
                        ctx.Reply("   • automationsystem_tp <e/x> - Teleport to entry/exit");
                        ctx.Reply("   • automationsystem_build <1-4> - Apply build preset");
                        ctx.Reply("   • automationsystem_loadout <name> - Apply loadout");
                        break;
                    case "pvp":
                        ctx.Reply("⚔️ PvP Commands:");
                        ctx.Reply("   • .pvp - Toggle PvP on/off");
                        ctx.Reply("   • .pvp on - Enable PvP");
                        ctx.Reply("   • .pvp off - Disable PvP");
                        ctx.Reply("   • PvP is automatically disabled when entering automation system");
                        break;
                    case "admin":
                        ctx.Reply("👑 Admin Commands:");
                        ctx.Reply("   • automationsystem_reload - Reload configuration");
                        ctx.Reply("   • automationsystem_save - Save configuration");
                        ctx.Reply("   • automationsystem_clear - Clear all snapshots");
                        ctx.Reply("   • automationsystem_add - Add prefab");
                        ctx.Reply("   • automationsystem_import/export - Manage prefabs");
                        break;
                    case "info":
                        ctx.Reply("ℹ️ Information Commands:");
                        ctx.Reply("   • automationsystem_help - Show this help");
                        ctx.Reply("   • automationsystem_stats - Show statistics");
                        ctx.Reply("   • automationsystem_list - List available items");
                        break;
                    default:
                        ctx.Reply("❌ Unknown category. Available: setup, player, pvp, admin, info");
                        break;
                }
            }
        }

        [Command("automationsystem_stats", description: "Show Automation System statistics")]
        public static void ShowStats(ICommandContext ctx)
        {
            try
            {
                var player = GetPlayerEntity(ctx);
                if (player.Equals(Entity.Null))
                {
                    ctx.Error("Could not find your player entity!");
                    return;
                }

                var inArena = ZoneManager.IsPlayerInArena(player);
                var snapshotCount = SnapshotManagerService.GetSnapshotCount();

                ctx.Reply("📊 Automation System Statistics:");
                ctx.Reply($"   • Automation System Status: Available");
                ctx.Reply($"   • Your Status: {(inArena ? "In Automation System" : "Outside Automation System")}");
                ctx.Reply($"   • Active Snapshots: {snapshotCount}");
                ctx.Reply($"   • Available Commands: 20+");
                ctx.Reply(
                    $"   • Spawn Point: {(ZoneManager.SpawnPoint.Equals(float3.zero) ? "Not Set" : "Configured")}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in ShowStats command: {ex.Message}");
                ctx.Error("❌ An error occurred while showing stats.");
            }
        }

        // ===== HELPER METHODS =====

        private static Entity GetPlayerEntity(ICommandContext ctx)
        {
            return PlayerManager.GetPlayerByName(ctx.Name);
        }

        private static float3 GetPlayerPosition(ICommandContext ctx)
        {
            var playerEntity = GetPlayerEntity(ctx);
            if (playerEntity != Entity.Null)
            {
                return PlayerService.GetPlayerPosition(playerEntity);
            }

            return float3.zero;
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
