using System;
using ProjectM;
using Unity.Entities;
using VampireCommandFramework;
using Unity.Mathematics;
using Unity.Transforms;
using CrowbaneArena.Services;
using ProjectM.Network;

namespace CrowbaneArena
{
    /// <summary>
    /// Commands for managing the Crowbane Arena.
    /// </summary>
    [CommandGroup("arena")]
    public static class ArenaCommands
    {
        /// <summary>
        /// Sets the arena zone.
        /// </summary>
        [Command("setzone", "<radius=50f>", "Sets the arena zone.", adminOnly: true)]
        public static void SetArenaZoneCommand(ChatCommandContext ctx, float radius = 50f)
        {
            var player = ctx.Event.SenderCharacterEntity;
            var pos = player.Read<Translation>().Value;
            ZoneManager.SetArenaZone(pos, radius);
            
            var config = ArenaConfigLoader.ArenaSettings;
            config.ZoneCenter = pos;
            config.ZoneRadius = radius;
            ArenaConfigLoader.SaveConfig();
            
            ctx.Reply($"Arena zone set at {pos} with radius {radius}");
        }

        /// <summary>
        /// Sets the arena entry point.
        /// </summary>
        [Command("setentry", "<radius=10f>", "Sets the arena entry point.", adminOnly: true)]
        public static void SetEntryPointCommand(ChatCommandContext ctx, float radius = 10f)
        {
            var player = ctx.Event.SenderCharacterEntity;
            var pos = player.Read<Translation>().Value;
            ZoneManager.SetEntryPoint(pos, radius);
            
            var config = ArenaConfigLoader.ArenaSettings;
            config.EntryPoint = pos;
            config.EntryRadius = radius;
            ArenaConfigLoader.SaveConfig();
            
            ctx.Reply($"Entry point set at {pos} with radius {radius}");
        }

        /// <summary>
        /// Sets the arena exit point.
        /// </summary>
        [Command("setexit", "<radius=10f>", "Sets the arena exit point.", adminOnly: true)]
        public static void SetExitPointCommand(ChatCommandContext ctx, float radius = 10f)
        {
            var player = ctx.Event.SenderCharacterEntity;
            var pos = player.Read<Translation>().Value;
            ZoneManager.SetExitPoint(pos, radius);
            
            var config = ArenaConfigLoader.ArenaSettings;
            config.ExitPoint = pos;
            config.ExitRadius = radius;
            ArenaConfigLoader.SaveConfig();
            
            ctx.Reply($"Exit point set at {pos} with radius {radius}");
        }

        /// <summary>
        /// Sets the arena spawn point.
        /// </summary>
        [Command("setspawn", "Sets the arena spawn point.", adminOnly: true)]
        public static void SetSpawnPointCommand(ChatCommandContext ctx)
        {
            var player = ctx.Event.SenderCharacterEntity;
            var pos = player.Read<Translation>().Value;
            ZoneManager.SetSpawnPoint(pos);
            
            var config = ArenaConfigLoader.ArenaSettings;
            config.SpawnPoint = pos;
            ArenaConfigLoader.SaveConfig();
            
            ctx.Reply($"Arena spawn point set at {pos}");
        }

        /// <summary>
        /// Manually enters the arena.
        /// </summary>
        [Command("enter", "Manually enters the arena.")]
        public static void EnterArenaCommand(ChatCommandContext ctx)
        {
            var player = ctx.Event.SenderCharacterEntity;
            ZoneManager.ManualEnterArena(player);
            ctx.Reply($"You have manually entered the arena.");
        }

        /// <summary>
        /// Exits the arena.
        /// </summary>
        [Command("exit", "Exits the arena.")]
        public static void ExitArenaCommand(ChatCommandContext ctx)
        {
            var player = ctx.Event.SenderCharacterEntity;
            var pos = ArenaConfigLoader.ArenaSettings.ExitPoint;
            TeleportService.Teleport(player, pos);
            ctx.Reply($"You have exited the arena.");
        }

        /// <summary>
        /// Checks the arena status.
        /// </summary>
        [Command("status", "Checks the arena status.")]
        public static void ArenaStatusCommand(ChatCommandContext ctx)
        {
            var player = ctx.Event.SenderCharacterEntity;
            bool inArena = ZoneManager.IsPlayerInArena(player);
            int territoryIndex = ZoneManager.GetPlayerTerritoryIndex(player);
            ctx.Reply($"Arena status: {(inArena ? "In Arena" : "Outside")} (Grid {territoryIndex})");
        }

        /// <summary>
        /// Creates a castle heart.
        /// </summary>
        [Command("castle", "<radius=50f>", "Creates a castle heart.", adminOnly: true)]
        public static void CreateCastleCommand(ChatCommandContext ctx, float radius = 50f)
        {
            var player = ctx.Event.SenderCharacterEntity;
            var pos = player.Read<Translation>().Value;
            var heartEntity = CastleManager.CreateCastleHeart(pos, radius);
            ctx.Reply($"Created castle heart with {radius} radius at {pos}");
        }
    }
}
