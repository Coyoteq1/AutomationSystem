using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutomationSystem.Automation;
using AutomationSystem.Snapshots;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutomationSystem.Core
{
    /// <summary>
    /// Core implementation of data management functionality
    /// Based on DataPersistenceManager from VRising arena system
    /// </summary>
    public class CoreDataManager : ICoreDataManager
    {
        private readonly ILogger<CoreDataManager> _logger;
        private readonly AutomationSystemOptions _options;
        private readonly ISnapshotManager _snapshotManager;
        
        private string _dataDirectory;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // In-memory storage for active data
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object>> _entityData;
        
        // Persistence suppression system
        private int _persistenceSuppressionCount = 0;
        private readonly object _suppressionLock = new();

        /// <summary>
        /// Constructor for CoreDataManager
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="options">The automation system options</param>
        /// <param name="snapshotManager">The snapshot manager instance</param>
        public CoreDataManager(ILogger<CoreDataManager> logger, IOptions<AutomationSystemOptions> options, ISnapshotManager snapshotManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _snapshotManager = snapshotManager ?? throw new ArgumentNullException(nameof(snapshotManager));
            
            _dataDirectory = _options.DataDirectory;
            _entityData = new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();
            
            _logger.LogInformation("CoreDataManager initialized with data directory: {DataDirectory}", _dataDirectory);
        }

        /// <summary>
        /// Gets or sets the data directory path
        /// </summary>
        public string DataDirectory
        {
            get => _dataDirectory;
            set
            {
                _dataDirectory = value;
                EnsureDataDirectory();
                _logger.LogInformation("Data directory updated to: {DataDirectory}", _dataDirectory);
            }
        }

        /// <summary>
        /// Initializes the data manager
        /// </summary>
        public void Initialize()
        {
            EnsureDataDirectory();
            _logger.LogInformation("CoreDataManager initialized successfully");
        }

        /// <summary>
        /// Creates a data snapshot for the specified entity
        /// </summary>
        /// <param name="entityId">The ID of the entity to snapshot</param>
        /// <param name="description">Optional description for the snapshot</param>
        /// <returns>The snapshot ID</returns>
        public string CreateSnapshot(string entityId, string? description = null)
        {
            try
            {
                var snapshotId = $"{entityId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
                var snapshot = new AutomationSnapshot
                {
                    SnapshotId = snapshotId,
                    EntityId = entityId,
                    CreatedAt = DateTime.UtcNow,
                    Description = description ?? "Manual snapshot",
                    Type = SnapshotType.Manual,
                    CreatedBy = "System",
                    SnapshotData = new Dictionary<string, object>(),
                    Metadata = new Dictionary<string, object>
                    {
                        ["EntityCount"] = _entityData.TryGetValue(entityId, out var data) ? data.Count : 0,
                        ["CreatedFrom"] = "CoreDataManager"
                    }
                };

                // Capture all available entity data
                if (_entityData.TryGetValue(entityId, out var entityData))
                {
                    foreach (var kvp in entityData)
                    {
                        snapshot.SnapshotData[kvp.Key] = kvp.Value;
                    }
                }

                // Save snapshot to disk
                var filePath = Path.Combine(_dataDirectory, "snapshots", $"{snapshotId}.json");
                _snapshotManager.SaveSnapshot(filePath, snapshot);

                _logger.LogInformation("Created snapshot {SnapshotId} for entity {EntityId}", snapshotId, entityId);
                return snapshotId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create snapshot for entity {EntityId}", entityId);
                return string.Empty;
            }
        }

        /// <summary>
        /// Restores data from a snapshot
        /// </summary>
        /// <param name="entityId">The ID of the entity to restore</param>
        /// <param name="snapshotId">The ID of the snapshot to restore</param>
        /// <returns>True if restoration was successful</returns>
        public bool RestoreSnapshot(string entityId, string snapshotId)
        {
            try
            {
                var filePath = Path.Combine(_dataDirectory, "snapshots", $"{snapshotId}.json");
                var snapshot = _snapshotManager.LoadSnapshot<AutomationSnapshot>(filePath);

                if (snapshot == null || snapshot.EntityId != entityId)
                {
                    _logger.LogWarning("Snapshot {SnapshotId} not found or doesn't match entity {EntityId}", snapshotId, entityId);
                    return false;
                }

                // Restore data with persistence suppression
                using (SuppressPersistence())
                {
                    _entityData.AddOrUpdate(entityId, new ConcurrentDictionary<string, object>(), 
                        (key, oldValue) =>
                        {
                            oldValue.Clear();
                            foreach (var kvp in snapshot.SnapshotData)
                            {
                                oldValue.TryAdd(kvp.Key, kvp.Value);
                            }
                            return oldValue;
                        });
                }

                _logger.LogInformation("Restored snapshot {SnapshotId} for entity {EntityId}", snapshotId, entityId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore snapshot {SnapshotId} for entity {EntityId}", snapshotId, entityId);
                return false;
            }
        }

        /// <summary>
        /// Gets all snapshots for a specific entity
        /// </summary>
        /// <param name="entityId">The ID of the entity</param>
        /// <returns>List of snapshots</returns>
        public List<AutomationSnapshot> GetSnapshots(string entityId)
        {
            var snapshots = new List<AutomationSnapshot>();
            
            try
            {
                var snapshotsDir = Path.Combine(_dataDirectory, "snapshots");
                if (!Directory.Exists(snapshotsDir))
                    return snapshots;

                var pattern = $"{entityId}_*.json";
                var files = Directory.GetFiles(snapshotsDir, pattern);

                foreach (var file in files)
                {
                    try
                    {
                        var snapshot = _snapshotManager.LoadSnapshot<AutomationSnapshot>(file);
                        if (snapshot != null && snapshot.EntityId == entityId)
                        {
                            snapshots.Add(snapshot);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error loading snapshot file {File}", file);
                    }
                }

                return snapshots.OrderByDescending(s => s.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting snapshots for entity {EntityId}", entityId);
                return snapshots;
            }
        }

        /// <summary>
        /// Deletes a specific snapshot
        /// </summary>
        /// <param name="entityId">The ID of the entity</param>
        /// <param name="snapshotId">The ID of the snapshot to delete</param>
        /// <returns>True if deletion was successful</returns>
        public bool DeleteSnapshot(string entityId, string snapshotId)
        {
            try
            {
                var filePath = Path.Combine(_dataDirectory, "snapshots", $"{snapshotId}.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted snapshot {SnapshotId} for entity {EntityId}", snapshotId, entityId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete snapshot {SnapshotId} for entity {EntityId}", snapshotId, entityId);
                return false;
            }
        }

        /// <summary>
        /// Saves data with persistence suppression support
        /// </summary>
        /// <param name="entityId">The ID of the entity</param>
        /// <param name="dataType">The type of data</param>
        /// <param name="data">The data to save</param>
        /// <returns>True if save was successful</returns>
        public bool SaveData(string entityId, string dataType, object data)
        {
            try
            {
                if (IsPersistenceSuppressed)
                {
                    return true; // Consider suppressed saves as successful
                }

                var entityData = _entityData.GetOrAdd(entityId, _ => new ConcurrentDictionary<string, object>());
                entityData[dataType] = data ?? throw new ArgumentNullException(nameof(data));

                // Save to disk if auto-persistence is enabled
                if (_options.AutoPersistenceEnabled)
                {
                    SaveDataToDisk(entityId, dataType, data);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save data for entity {EntityId}, type {DataType}", entityId, dataType);
                return false;
            }
        }

        /// <summary>
        /// Loads data
        /// </summary>
        /// <param name="entityId">The ID of the entity</param>
        /// <param name="dataType">The type of data</param>
        /// <param name="data">When this method returns, contains the loaded data if successful, or default if failed</param>
        /// <returns>True if load was successful</returns>
        public bool LoadData(string entityId, string dataType, out object data)
        {
            data = default;

            try
            {
                if (_entityData.TryGetValue(entityId, out var entityData) && entityData.TryGetValue(dataType, out var loadedData))
                {
                    data = loadedData;
                    return true;
                }

                // Try to load from disk
                return LoadDataFromDisk(entityId, dataType, out data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load data for entity {EntityId}, type {DataType}", entityId, dataType);
                return false;
            }
        }

        /// <summary>
        /// Creates a backup of all data for an entity
        /// </summary>
        /// <param name="entityId">The ID of the entity</param>
        /// <param name="timestamp">Optional timestamp for the backup</param>
        /// <returns>True if backup was successful</returns>
        public bool BackupData(string entityId, string? timestamp = null)
        {
            try
            {
                EnsureDataDirectory();
                var backupDir = Path.Combine(_dataDirectory, "backups", entityId);
                
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                var timeStamp = timestamp ?? DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var backupFile = Path.Combine(backupDir, $"{timeStamp}_{entityId}_backup.json");

                var entityData = _entityData.GetOrAdd(entityId, _ => new ConcurrentDictionary<string, object>());
                _snapshotManager.SaveSnapshot(backupFile, entityData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

                _logger.LogInformation("Created backup for entity {EntityId} at {BackupFile}", entityId, backupFile);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to backup data for entity {EntityId}", entityId);
                return false;
            }
        }

        /// <summary>
        /// Restores data from a backup
        /// </summary>
        /// <param name="entityId">The ID of the entity</param>
        /// <param name="timestamp">The timestamp of the backup to restore</param>
        /// <returns>True if restoration was successful</returns>
        public bool RestoreDataFromBackup(string entityId, string timestamp)
        {
            try
            {
                var backupDir = Path.Combine(_dataDirectory, "backups", entityId);
                var backupFile = Path.Combine(backupDir, $"{timestamp}_{entityId}_backup.json");

                if (_snapshotManager.LoadSnapshot<Dictionary<string, object>>(backupFile) is var backupData && backupData != null)
                {
                    using (SuppressPersistence())
                    {
                        var entityData = _entityData.GetOrAdd(entityId, _ => new ConcurrentDictionary<string, object>());
                        entityData.Clear();
                        
                        foreach (var kvp in backupData)
                        {
                            entityData.TryAdd(kvp.Key, kvp.Value);
                        }
                    }

                    _logger.LogInformation("Restored backup for entity {EntityId} from {BackupFile}", entityId, backupFile);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore backup for entity {EntityId} with timestamp {Timestamp}", entityId, timestamp);
                return false;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Ensures the data directory exists
        /// </summary>
        private void EnsureDataDirectory()
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
        }

        /// <summary>
        /// Saves data to disk
        /// </summary>
        private void SaveDataToDisk(string entityId, string dataType, object data)
        {
            try
            {
                var filePath = Path.Combine(_dataDirectory, "data", $"{entityId}_{dataType}.json");
                _snapshotManager.SaveSnapshot(filePath, data);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save data to disk for entity {EntityId}, type {DataType}", entityId, dataType);
            }
        }

        /// <summary>
        /// Loads data from disk
        /// </summary>
        private bool LoadDataFromDisk(string entityId, string dataType, out object data)
        {
            data = default;

            try
            {
                var filePath = Path.Combine(_dataDirectory, "data", $"{entityId}_{dataType}.json");
                data = _snapshotManager.LoadSnapshot<object>(filePath);
                
                if (data != null)
                {
                    // Load into memory cache
                    var entityData = _entityData.GetOrAdd(entityId, _ => new ConcurrentDictionary<string, object>());
                    entityData[dataType] = data;
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load data from disk for entity {EntityId}, type {DataType}", entityId, dataType);
            }

            return false;
        }

        /// <summary>
        /// Returns an IDisposable scope that prevents automatic saves during data operations
        /// </summary>
        public IDisposable SuppressPersistence()
        {
            lock (_suppressionLock)
            {
                _persistenceSuppressionCount++;
                return new PersistenceSuppressionScope(this);
            }
        }

        /// <summary>
        /// Property to check if persistence is currently suppressed
        /// </summary>
        public bool IsPersistenceSuppressed
        {
            get
            {
                lock (_suppressionLock)
                {
                    return _persistenceSuppressionCount > 0;
                }
            }
        }

        /// <summary>
        /// Internal method to release persistence suppression
        /// </summary>
        internal void ReleasePersistenceSuppression()
        {
            lock (_suppressionLock)
            {
                _persistenceSuppressionCount = Math.Max(0, _persistenceSuppressionCount - 1);
            }
        }

        #endregion

        /// <summary>
        /// Persistence suppression scope implementation
        /// </summary>
        private class PersistenceSuppressionScope : IDisposable
        {
            private readonly CoreDataManager _manager;

            public PersistenceSuppressionScope(CoreDataManager manager)
            {
                _manager = manager;
            }

            public void Dispose()
            {
                _manager.ReleasePersistenceSuppression();
            }
        }
    }
}