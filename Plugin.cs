using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AutomationSystem;

namespace AutomationSystem.Entry
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("gg.Coyote.Framework")]
    public class AutomationPlugin : BasePlugin
    {
        internal static ManualLogSource Logger { get; private set; }
        Harmony _harmony;
        private IServiceProvider? _serviceProvider;
        private global::AutomationSystem.Core.AutomationSystem? _automationSystem;

        public override void Load()
        {
            // Initialize static logger
            Logger = Log;

            // Initialize VRisingCore with logger
            VRisingCore.Initialize(Logger);

            // Plugin startup logic
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");

            // Configure dependency injection
            ConfigureServices();

            // Initialize core services if service provider is available
            InitializeCoreServices();
            
            // Initialize automation system
            _automationSystem = new global::AutomationSystem.Core.AutomationSystem();

            // Harmony patching
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

            // Register all commands in the assembly with VCF
            CommandRegistry.RegisterAll();
        }

        public override bool Unload()
        {
            // Cleanup services
            CleanupServices();
            
            _automationSystem = null;

            // Unregister commands
            CommandRegistry.UnregisterAssembly();
            
            // Unpatch harmony
            _harmony?.UnpatchSelf();
            
            return true;
        }

        /// <summary>
        /// Configure dependency injection services
        /// </summary>
        private void ConfigureServices()
        {
            var services = new ServiceCollection();
            
            // Add AutomationSystem services
            services.AddAutomationSystem(options =>
            {
                options.DataDirectory = "Data/AutomationSystem";
                options.AutoPersistenceEnabled = true;
                options.PersistenceIntervalSeconds = 60;
                options.BackupEnabled = true;
                options.MaxBackups = 5;
            });

            // Build service provider
            _serviceProvider = services.BuildServiceProvider();
            
            // Set up CommandRegistry logger
            var logger = _serviceProvider.GetService<ILogger<CommandRegistry>>();
            AutomationSystem.Services.CommandRegistry.SetLogger(logger);
            
            Log.LogInfo("Dependency injection services configured successfully");
        }

        /// <summary>
        /// Initialize core automation services
        /// </summary>
        private void InitializeCoreServices()
        {
            if (_serviceProvider == null)
            {
                Log.LogWarning("Service provider is not available - core services will not be initialized");
                return;
            }

            try
            {
                // Get core services
                var coreDataManager = _serviceProvider.GetService<ICoreDataManager>();
                var snapshotManager = _serviceProvider.GetService<ISnapshotManager>();
                var configService = _serviceProvider.GetService<IConfigurationService>();
                var automationTracker = _serviceProvider.GetService<IAutomationTracker>();

                // Get additional wired services
                var snapshotManagerService = _serviceProvider.GetService<AutomationSystem.Services.SnapshotManagerService>();
                var progressManagerService = _serviceProvider.GetService<AutomationSystem.Services.ProgressManagerService>();
                var queueService = _serviceProvider.GetService<AutomationSystem.Services.QueueService>();
                var sessionService = _serviceProvider.GetService<AutomationSystem.Services.SessionService>();
                var systemService = _serviceProvider.GetService<AutomationSystem.Services.SystemService>();
                var loggingService = _serviceProvider.GetService<AutomationSystem.Services.LoggingService>();
                var localizationService = _serviceProvider.GetService<AutomationSystem.Services.LocalizationService>();
                var vectorService = _serviceProvider.GetService<AutomationSystem.Services.VectorService>();

                // Initialize core data manager
                coreDataManager?.Initialize();

                // Initialize snapshot manager
                snapshotManager?.Initialize();

                // Initialize static services
                global::AutomationSystem.Services.PlayerSnapshotService.Initialize();

                // Initialize additional services that need initialization
                progressManagerService?.Initialize();

                // Initialize VBlood Hook System
                global::AutomationSystem.Services.VBloodHookService.Initialize();

                Log.LogInfo("All automation services initialized successfully");
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to initialize services: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleanup services on unload
        /// </summary>
        private void CleanupServices()
        {
            if (_serviceProvider == null)
                return;

            try
            {
                var coreDataManager = _serviceProvider.GetService<ICoreDataManager>();
                var snapshotManager = _serviceProvider.GetService<ISnapshotManager>();

                // Cleanup services
                coreDataManager?.Shutdown();
                snapshotManager?.Shutdown();

                // Dispose service provider
                _serviceProvider.Dispose();
                _serviceProvider = null;

                Log.LogInfo("Services cleaned up successfully");
            }
            catch (Exception ex)
            {
                Log.LogError($"Error during service cleanup: {ex.Message}");
            }
        }

        // // Uncomment for example commmand or delete

        // /// <summary>
        // /// Example VCF command that demonstrated default values and primitive types
        // /// Visit https://github.com/decaprime/VampireCommandFramework for more info
        // /// </summary>
        // /// <remarks>
        // /// How you could call this command from chat:
        // ///
        // /// .automationsystem-example "some quoted string" 1 1.5
        // /// .automationsystem-example boop 21232
        // /// .automationsystem-example boop-boop
        // ///</remarks>
        // [Command("automationsystem-example", description: "Example command from automationsystem", adminOnly: true)]
        // public void ExampleCommand(ICommandContext ctx, string someString, int num = 5, float num2 = 1.5f)
        // {
        //     ctx.Reply($"You passed in {someString} and {num} and {num2}");
        // }
    }
}
