using System;
using CrowbaneArena.Services;

namespace CrowbaneArena
{
    /// <summary>
    /// God-Mode system for arena progression. Unlocks all V Blood on entry, restores on exit.
    /// </summary>
    public class GodModeSystem
    {
        private SnapshotService snapshotService;
        private bool originalProgressionCaptured = false;

        public GodModeSystem()
        {
            snapshotService = new SnapshotService();
        }

        /// <summary>
        /// Enters arena mode: captures progression and unlocks all.
        /// </summary>
        public void EnterArena(object playerEntity)
        {
            snapshotService.CaptureProgression(playerEntity);
            originalProgressionCaptured = true;
            snapshotService.ActivateArenaMode();
        }

        /// <summary>
        /// Exits arena mode: restores progression.
        /// </summary>
        public void ExitArena(object playerEntity)
        {
            if (originalProgressionCaptured)
            {
                snapshotService.DeactivateArenaMode(playerEntity);
                originalProgressionCaptured = false;
            }
        }
    }
}
