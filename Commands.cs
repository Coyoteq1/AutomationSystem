using VampireCommandFramework;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using CrowbaneArena.Services;
using Stunlock.Core;

namespace CrowbaneArena
{
    public static class ArenaCommands
    {
        // ===== ARENA SETUP COMMANDS (Admin Only) =====
        
        [Command("arena_setzone", description: "Set arena zone radius", adminOnly: true)]
        public static void SetZone(ICommandContext ctx, float radius = 50f)
        {
            if (radius <= 0)
            {
                ctx.Error("Radius must be greater than 0!");
                return;
            }
            
            ArenaController.SetZoneRadius(radius);
            ctx.Reply($"Arena zone set with radius {radius}m");
        }

        [Command("arena_setentry", description: "Set entry point and radius", adminOnly: true)]
        public static void SetEntry(ICommandContext ctx, float radius = 10f)
        {
            if (radius <= 0)
            {
                ctx.Error("Entry radius must be greater than 0!");
                return;
            }
            
            var position = GetPlayerPosition(ctx);
            ArenaController.SetEntryPoint(position, radius);
            ctx.Reply($"Entry point set at your location with radius {radius}m");
        }

        [Command("arena_setexit", description: "Set exit point and radius", adminOnly: true)]
        public static void SetExit(ICommandContext ctx, float radius = 10f)
        {
            if (radius <= 0)
            {
                ctx.Error("Exit radius must be greater than 0!");
                return;
            }
            
            var position = GetPlayerPosition(ctx);
            ArenaController.SetExitPoint(position, radius);
            ctx.Reply($"Exit point set at your location with radius {radius}m");
        }

        [Command("arena_setspawn", description: "Set arena spawn point", adminOnly: true)]
        public static void SetSpawn(ICommandContext ctx)
        {
            var position = GetPlayerPosition(ctx);
            ArenaController.SetSpawnPoint(position);
            ctx.Reply("Arena spawn point set at your location");
        }

        [Command("arena_reload", description: "Reload arena configuration", adminOnly: true)]
        public static void ReloadConfig(ICommandContext ctx)
        {
            try
            {
                ArenaConfigLoader.Initialize();
                ctx.Reply("✅ Arena configuration reloaded successfully");
            }
            catch (Exception ex)
            {
                ctx.Error($"❌ Failed to reload config: {ex.Message}");
            }
        }

        [Command("arena_save", description: "Save arena configuration", adminOnly: true)]
        public static void SaveConfig(ICommandContext ctx)
        {
            try
            {
                ArenaConfigLoader.SaveConfig();
                ctx.Reply("✅ Arena configuration saved successfully");
            }
            catch (Exception ex)
            {
                ctx.Error($"❌ Failed to save config: {ex.Message}");
            }
        }

        [Command("arena_clear", adminOnly: true, description: "Clear all arena snapshots")]
        public static void ClearSnapshots(ICommandContext ctx)
        {
            try
            {
                SnapshotManagerService.ClearAllSnapshots();
                ctx.Reply("✅ All arena snapshots cleared");
            }
            catch (Exception ex)
            {
                ctx.Error($"❌ Error clearing snapshots: {ex.Message}");
            }
        }

        // ===== PLAYER ARENA COMMANDS =====

        [Command("arena_enter", description: "Enter the arena")]
        public static void EnterArena(ICommandContext ctx)
        {
            try
            {
                var characterEntity = GetPlayerEntity(ctx);

                if (characterEntity == Entity.Null)
                {
                    ctx.Error("❌ Error: Invalid entity. Please try again.");
                    return;
                }

                if (!VRisingCore.EntityManager.HasComponent<PlayerCharacter>(characterEntity))
                {
                    ctx.Error("❌ Error: Invalid character entity. Please try reconnecting.");
                    return;
                }

                if (ZoneManager.IsPlayerInArena(characterEntity))
                {
                    ctx.Error("❌ You are already in the arena!");
                    return;
                }

                var playerCharacter = VRisingCore.EntityManager.GetComponentData<PlayerCharacter>(characterEntity);
                var userEntity = playerCharacter.UserEntity;

                if (userEntity == Entity.Null)
                {
                    ctx.Error("❌ Error: Could not find user entity.");
                    return;
                }

                if (ZoneManager.SpawnPoint.Equals(float3.zero))
                {
                    ctx.Error("❌ Error: Arena spawn point not configured! Please contact an admin.");
                    return;
                }

                var steamId = PlayerService.GetSteamId(userEntity);
                var arenaLocation = ZoneManager.SpawnPoint;

                Plugin.Logger?.LogInfo($"Attempting arena entry for player {PlayerService.GetPlayerName(userEntity)} (SteamID: {steamId})");

                if (SnapshotManagerService.EnterArena(userEntity, characterEntity, arenaLocation))
                {
                    ZoneManager.ManualEnterArena(characterEntity);
                    ctx.Reply("✅ Entering arena mode... Good luck!");
                    PlayerManager.UpdatePlayerState(characterEntity, new PlayerState { IsInArena = true, VBloodCount = 0 });
                }
                else
                {
                    ctx.Error("❌ Failed to enter arena. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in EnterArena command: {ex.Message}");
                ctx.Error("❌ An error occurred while entering the arena. Please try again.");
            }
        }

        [Command("arena_exit", description: "Exit the arena")]
        public static void ExitArena(ICommandContext ctx)
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
                if (!SnapshotManagerService.IsInArena(steamId))
                {
                    ctx.Error("You are not in the arena!");
                    return;
                }

                Plugin.Logger?.LogInfo($"Attempting arena exit for player {PlayerService.GetPlayerName(userEntity)} (SteamID: {steamId})");

                if (SnapshotManagerService.ExitArena(userEntity, characterEntity))
                {
                    ZoneManager.ManualExitArena(characterEntity);
                    ctx.Reply("✅ Exiting arena mode... Thanks for playing!");
                    PlayerManager.UpdatePlayerState(characterEntity, new PlayerState { IsInArena = false, VBloodCount = 0 });
                }
                else
                {
                    ctx.Error("❌ Failed to exit arena. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in ExitArena command: {ex.Message}");
                ctx.Error("❌ An error occurred while exiting the arena. Please try again.");
            }
        }

        [Command("arena_status", description: "Check arena status")]
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

                ctx.Reply($"📊 Arena Status for {playerName}:");
                ctx.Reply($"   • In Arena: {(isInArena ? "✅ Yes" : "❌ No")}");
                ctx.Reply($"   • Position: {position.x:F1}, {position.y:F1}, {position.z:F1}");
                ctx.Reply($"   • Steam ID: {steamId}");
                
                if (ZoneManager.SpawnPoint.Equals(float3.zero))
                {
                    ctx.Reply($"   • ⚠️ Arena spawn point not configured!");
                }
                else
                {
                    ctx.Reply($"   • Spawn Point: {ZoneManager.SpawnPoint.x:F1}, {ZoneManager.SpawnPoint.y:F1}, {ZoneManager.SpawnPoint.z:F1}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in CheckStatus command: {ex.Message}");
                ctx.Error("❌ An error occurred while checking status.");
            }
        }

        [Command("arena_tp", description: "Teleport to entry or exit (e for entry, x for exit)")]
        public static void TeleportArena(ICommandContext ctx, string location = "e")
        {
            try
            {
                var player = GetPlayerEntity(ctx);
                if (player.Equals(Entity.Null))
                {
                    ctx.Error("Could not find your player entity!");
                    return;
                }

                var position = location.ToLower() == "x" ? ArenaController.GetExitPoint() : ArenaController.GetEntryPoint();
                if (position.Equals(float3.zero))
                {
                    ctx.Error("Arena point not set!");
                    return;
                }

                if (VRisingCore.EntityManager.HasComponent<Translation>(player))
                {
                    var translation = VRisingCore.EntityManager.GetComponentData<Translation>(player);
                    translation.Value = position;
                    VRisingCore.EntityManager.SetComponentData(player, translation);
                    ctx.Reply($"✅ Teleported to {(location.ToLower() == "x" ? "exit" : "entry")} point.");
                }
                else
                {
                    ctx.Error("Could not teleport player!");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in TeleportArena command: {ex.Message}");
                ctx.Error("❌ An error occurred while teleporting.");
            }
        }

        // ===== EQUIPMENT AND LOADOUT COMMANDS =====

        [Command("arena_build", description: "Select a build preset (1-4)")]
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

        [Command("arena_give", description: "Give an item by name")]
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

        [Command("arena_list", description: "List available items")]
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

        [Command("arena_loadout", description: "Apply a loadout by name")]
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

        [Command("arena_add", adminOnly: true, description: "Add an item to prefabs")]
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

        [Command("arena_import", adminOnly: true, description: "Import prefabs from JSON")]
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

        [Command("arena_export", adminOnly: true, description: "Export all prefabs to JSON")]
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

        [Command("arena_help", description: "Show Arena command help")]
        public static void ShowHelp(ICommandContext ctx, string category = "")
        {
            if (string.IsNullOrEmpty(category))
            {
                ctx.Reply("🏟️ Arena Commands Help:");
                ctx.Reply("📋 Categories: setup, player, admin, info");
                ctx.Reply("💡 Use '.arena_help <category>' for specific commands");
                ctx.Reply("🎮 Quick commands: arena_enter, arena_exit, arena_status");
            }
            else
            {
                switch (category.ToLower())
                {
                    case "setup":
                        ctx.Reply("🔧 Setup Commands (Admin Only):");
                        ctx.Reply("   • arena_setzone <radius> - Set arena zone");
                        ctx.Reply("   • arena_setentry <radius> - Set entry point");
                        ctx.Reply("   • arena_setexit <radius> - Set exit point");
                        ctx.Reply("   • arena_setspawn - Set spawn point");
                        break;
                    case "player":
                        ctx.Reply("🎮 Player Commands:");
                        ctx.Reply("   • arena_enter - Enter the arena");
                        ctx.Reply("   • arena_exit - Exit the arena");
                        ctx.Reply("   • arena_status - Check your status");
                        ctx.Reply("   • arena_tp <e/x> - Teleport to entry/exit");
                        ctx.Reply("   • arena_build <1-4> - Apply build preset");
                        ctx.Reply("   • arena_loadout <name> - Apply loadout");
                        break;
                    case "admin":
                        ctx.Reply("👑 Admin Commands:");
                        ctx.Reply("   • arena_reload - Reload configuration");
                        ctx.Reply("   • arena_save - Save configuration");
                        ctx.Reply("   • arena_clear - Clear all snapshots");
                        ctx.Reply("   • arena_add - Add prefab");
                        ctx.Reply("   • arena_import/export - Manage prefabs");
                        break;
                    case "info":
                        ctx.Reply("ℹ️ Information Commands:");
                        ctx.Reply("   • arena_help - Show this help");
                        ctx.Reply("   • arena_stats - Show statistics");
                        ctx.Reply("   • arena_list - List available items");
                        break;
                    default:
                        ctx.Reply("❌ Unknown category. Available: setup, player, admin, info");
                        break;
                }
            }
        }

        [Command("arena_stats", description: "Show Arena statistics")]
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

                ctx.Reply("📊 Arena Statistics:");
                ctx.Reply($"   • Arena Status: Available");
                ctx.Reply($"   • Your Status: {(inArena ? "In Arena" : "Outside Arena")}");
                ctx.Reply($"   • Active Snapshots: {snapshotCount}");
                ctx.Reply($"   • Available Commands: 20+");
                ctx.Reply($"   • Spawn Point: {(ZoneManager.SpawnPoint.Equals(float3.zero) ? "Not Set" : "Configured")}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Error in ShowStats command: {ex.Message}");
                ctx.Error("❌ An error occurred while showing stats.");
            }
        }

        // ===== COMMAND ALIASES =====

        [Command("join", description: "Alias for arena_enter")]
        public static void JoinArena(ICommandContext ctx) => EnterArena(ctx);

        [Command("leave", description: "Alias for arena_exit")]
        public static void LeaveArena(ICommandContext ctx) => ExitArena(ctx);

        [Command("arena", description: "Alias for arena_status")]
        public static void ArenaStatus(ICommandContext ctx) => CheckStatus(ctx);

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
}