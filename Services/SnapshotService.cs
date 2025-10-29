using System;
using System.Collections.Generic;

namespace CrowbaneArena.Services
{
    /// <summary>
    /// Service for capturing and restoring V Blood progression snapshots for arena mode.
    /// </summary>
    public class SnapshotService
    {
        private readonly LoggingService _logger = new LoggingService();
        private Dictionary<object, bool> originalProgression;
        private bool isInArenaMode;

        /// <summary>
        /// Captures the current V Blood progression state.
        /// Safe to call multiple times; first capture per session is kept until restore.
        /// </summary>
        public void CaptureProgression(object playerEntity)
        {
            if (playerEntity == null)
            {
                _logger.LogEvent("CaptureProgression called with invalid player entity.");
                return;
            }

            if (originalProgression == null || originalProgression.Count == 0)
            {
                originalProgression = new Dictionary<object, bool>();
                // Placeholder for actual progression capture logic
                // This would query the game's progression system and store per-boss flags.
                originalProgression[playerEntity] = true; // Marker that we captured something for this player
                _logger.LogEvent($"Capturing V Blood progression for player: {playerEntity}");
            }
            else
            {
                _logger.LogEvent("CaptureProgression skipped: snapshot already captured.");
            }
        }

        /// <summary>
        /// Restores the original V Blood progression state.
        /// </summary>
        public void RestoreProgression(object playerEntity)
        {
            if (originalProgression != null && originalProgression.Count > 0)
            {
                // Placeholder for actual restoration logic
                _logger.LogEvent($"Restoring V Blood progression for player: {playerEntity}");
                originalProgression.Clear();
            }
            else
            {
                _logger.LogEvent("RestoreProgression skipped: no snapshot present.");
            }
        }

        /// <summary>
        /// Activates arena mode by unlocking all V Blood bosses and loading config.
        /// </summary>
        public void ActivateArenaMode()
        {
            if (isInArenaMode)
            {
                _logger.LogEvent("ActivateArenaMode called but arena mode is already active.");
                return;
            }

            isInArenaMode = true;
            // Load arena config
            var weapons = ArenaConfigLoader.GetWeapons();
            var armorSets = ArenaConfigLoader.GetArmorSets();
            var loadouts = ArenaConfigLoader.GetLoadouts();
            var builds = ArenaConfigLoader.GetBuilds();

            _logger.LogEvent($"Arena mode activated: Loaded {weapons.Count} weapons, {armorSets.Count} armor sets, {loadouts.Count} loadouts, {builds.Count} builds.");
            _logger.LogEvent("All V Blood unlocked (logical state).");
        }

        /// <summary>
        /// Deactivates arena mode and restores original state.
        /// </summary>
        public void DeactivateArenaMode(object playerEntity)
        {
            if (!isInArenaMode)
            {
                _logger.LogEvent("DeactivateArenaMode called but arena mode is not active.");
                return;
            }

            isInArenaMode = false;
            RestoreProgression(playerEntity);
            _logger.LogEvent("Arena mode deactivated.");
        }

        /// <summary>
        /// Gets available loadouts for the player.
        /// </summary>
        public List<Loadout> GetAvailableLoadouts()
        {
            return ArenaConfigLoader.GetLoadouts() ?? new List<Loadout>();
        }
    }
}
