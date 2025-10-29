using System;
using Unity.Entities;
using CrowbaneArena.Services;

namespace CrowbaneArena
{
    /// <summary>
    /// Core initialization and service management for CrowbaneArena.
    /// Provides centralized access to all arena services and VRising systems.
    /// </summary>
    public static class CrowbaneArenaCore
{
    private static World? _world;
    private static EntityManager? _entityManager;
    private static SystemService? _systemService;

    /// <summary>
    /// Gets the main VRising world (equivalent to Core.Server in original VAMP).
    /// </summary>
    public static World World
    {
        get
        {
            if (_world == null)
            {
                _world = VRisingCore.EntityManager.World;
            }
            return _world ?? throw new InvalidOperationException("World not available");
        }
    }

    /// <summary>
    /// EntityManager for the current VRising world.
    /// </summary>
    public static EntityManager EntityManager
    {
        get
        {
            if (_entityManager == null)
            {
                _entityManager = VRisingCore.EntityManager;
            }
            return _entityManager ?? throw new InvalidOperationException("EntityManager not available");
        }
    }

    /// <summary>
    /// System service providing access to ECS systems.
    /// </summary>
    public static SystemService SystemService
    {
        get
        {
            if (_systemService == null)
            {
                _systemService = new SystemService(World);
            }
            return _systemService ?? throw new InvalidOperationException("SystemService not available");
        }
    }

    /// <summary>
    /// Player service for player management and lookups (static class - no initialization needed).
    /// </summary>
    public static bool PlayerServiceInitialized { get; private set; } = true;

    /// <summary>
    /// Spawn service for PvP player positioning (static class).
    /// </summary>
    public static bool SpawnServiceInitialized { get; private set; } = true;

    /// <summary>
    /// Castle heart service for territory ownership.
    /// </summary>
    public static CastleHeartService CastleHeartService { get; private set; }

    /// <summary>
    /// Castle territory service for arena territory management.
    /// </summary>
    public static CastleTerritoryService CastleTerritoryService { get; private set; }

    /// <summary>
    /// Indicates whether the core services have been initialized.
    /// </summary>
    public static bool HasInitialized { get; private set; } = false;

    /// <summary>
    /// Initializes all CrowbaneArena core services.
    /// Call this after VRising has loaded all its systems.
    /// </summary>
    public static void Initialize()
    {
        if (HasInitialized)
        {
            VRisingCore.Log?.LogInfo("CrowbaneArenaCore already initialized");
            return;
        }

        if (!VRisingCore.IsReady())
        {
            VRisingCore.Log?.LogWarning("VRisingCore not ready - delaying CrowbaneArenaCore initialization");
            return;
        }

        try
        {
            VRisingCore.Log?.LogInfo("Initializing CrowbaneArenaCore services...");

            // Initialize services in dependency order
            CastleHeartService = new CastleHeartService();
            CastleTerritoryService = new CastleTerritoryService();
            // SpawnService and PlayerService are static, always available

            // Initialize PlayerService cache
            CrowbaneArena.Services.PlayerService.Initialize();

            // Initialize SnapshotManagerService cache
            CrowbaneArena.Services.SnapshotManagerService.Initialize();

            // Force initialization of lazy-loaded properties
            _ = World;
            _ = EntityManager;
            _ = SystemService;

            HasInitialized = true;

            VRisingCore.Log?.LogInfo("CrowbaneArenaCore initialization completed successfully");

            // Optional: Trigger any post-initialization events here
            OnCoreLoaded?.Invoke();
        }
        catch (Exception ex)
        {
            VRisingCore.Log?.LogError($"Failed to initialize CrowbaneArenaCore: {ex.Message}");
            HasInitialized = false;
        }
    }

    /// <summary>
    /// Event triggered after core initialization completes.
    /// Subscribe to this to perform custom setup after services are available.
    /// </summary>
    public static event Action OnCoreLoaded;

    /// <summary>
    /// Gets information about the current initialization state.
    /// </summary>
    public static string GetStatus()
    {
        if (!VRisingCore.IsReady())
            return "Waiting for VRising to initialize";

        if (!HasInitialized)
            return "Ready for initialization";

        return $"Initialized - Services: Player={PlayerServiceInitialized}, System={_systemService != null}, Spawn={SpawnServiceInitialized}, CastleHeart={CastleHeartService != null}, CastleTerritory={CastleTerritoryService != null}";
    }

    /// <summary>
    /// Force reinitialize all services (useful for testing or world changes).
    /// </summary>
    public static void Reinitialize()
    {
        VRisingCore.Log?.LogInfo("Reinitializing CrowbaneArenaCore...");

        HasInitialized = false;

        // Reset lazy-loaded properties
        _world = null;
        _entityManager = null;
        _systemService = null;

        // Reset services
        CastleHeartService = null;
        CastleTerritoryService = null;

        // Reinitialize services
        Initialize();
    }
}
}
