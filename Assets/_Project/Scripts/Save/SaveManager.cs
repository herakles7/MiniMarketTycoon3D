using System;
using System.IO;
using UnityEngine;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.Save
{
    /// <summary>
    /// Manages persistent save/load operations, save versioning, and migration logic.
    /// Uses robust local JSON serialization with atomic writes to prevent corruption.
    /// </summary>
    public class SaveManager : MonoBehaviourSingleton<SaveManager>
    {
        public const int CURRENT_SAVE_VERSION = 1;
        private const string SAVE_FILE_NAME = "gamesave.json";
        private const string SAVE_BACKUP_NAME = "gamesave.json.bak";

        [Header("Auto Save")]
        [SerializeField] private bool _autoSaveEnabled = true;
        [SerializeField] private float _autoSaveIntervalSeconds = 60.0f;

        private GameSaveData _currentSave;
        private string _saveFilePath;
        private string _backupFilePath;
        private float _autoSaveTimer = 0.0f;

        public GameSaveData CurrentSave => _currentSave;
        public bool HasLoadedSave => _currentSave != null;

        public event Action<GameSaveData> OnSaveLoaded;
        public event Action<GameSaveData> OnBeforeSave;
        public event Action<GameSaveData> OnSaveCompleted;

        private void EnsurePathsInitialized()
        {
            if (string.IsNullOrEmpty(_saveFilePath))
            {
                _saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
                _backupFilePath = Path.Combine(Application.persistentDataPath, SAVE_BACKUP_NAME);
            }
        }

        protected override void OnInitialized()
        {
            EnsurePathsInitialized();
            LoadGame();
        }

        private void Update()
        {
            if (!_autoSaveEnabled || _currentSave == null) return;

            _autoSaveTimer += Time.unscaledDeltaTime;
            if (_autoSaveTimer >= _autoSaveIntervalSeconds)
            {
                _autoSaveTimer = 0.0f;
                SaveGame();
            }
        }

        public void LoadGame()
        {
            EnsurePathsInitialized();

            if (File.Exists(_saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_saveFilePath);
                    _currentSave = JsonUtility.FromJson<GameSaveData>(json);

                    if (_currentSave == null)
                    {
                        Debug.LogWarning("[SaveManager] Deserialized save was null. Creating new save.");
                        CreateNewSave();
                    }
                    else
                    {
                        CheckAndMigrateSave(_currentSave);
                        Debug.Log($"[SaveManager] Save loaded successfully. Version: {_currentSave.SaveVersion}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SaveManager] Failed to read save file: {ex.Message}. Attempting backup.");
                    TryLoadBackupOrCreateNew();
                }
            }
            else
            {
                Debug.Log("[SaveManager] No existing save found. Creating initial save data.");
                CreateNewSave();
            }

            OnSaveLoaded?.Invoke(_currentSave);
        }

        private void TryLoadBackupOrCreateNew()
        {
            if (File.Exists(_backupFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_backupFilePath);
                    _currentSave = JsonUtility.FromJson<GameSaveData>(json);
                    CheckAndMigrateSave(_currentSave);
                    Debug.Log("[SaveManager] Backup save loaded successfully.");
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SaveManager] Backup save corrupted: {ex.Message}");
                }
            }

            CreateNewSave();
        }

        public void CreateNewSave()
        {
            _currentSave = new GameSaveData
            {
                SaveVersion = CURRENT_SAVE_VERSION,
                LastSaveTimeUtc = DateTime.UtcNow.ToString("o")
            };
            SaveGame();
        }

        public void SaveGame()
        {
            EnsurePathsInitialized();

            if (_currentSave == null)
            {
                Debug.LogWarning("[SaveManager] Cannot save null data.");
                return;
            }

            try
            {
                OnBeforeSave?.Invoke(_currentSave);

                _currentSave.SaveVersion = CURRENT_SAVE_VERSION;
                _currentSave.LastSaveTimeUtc = DateTime.UtcNow.ToString("o");

                string json = JsonUtility.ToJson(_currentSave, true);
                string tempFilePath = _saveFilePath + ".tmp";

                File.WriteAllText(tempFilePath, json);

                if (File.Exists(_saveFilePath))
                {
                    File.Copy(_saveFilePath, _backupFilePath, true);
                }

                File.Copy(tempFilePath, _saveFilePath, true);
                File.Delete(tempFilePath);

                Debug.Log($"[SaveManager] Game saved successfully at {_currentSave.LastSaveTimeUtc}");
                OnSaveCompleted?.Invoke(_currentSave);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Exception while saving game: {ex.Message}");
            }
        }

        private void CheckAndMigrateSave(GameSaveData save)
        {
            if (save.SaveVersion < CURRENT_SAVE_VERSION)
            {
                Debug.Log($"[SaveManager] Migrating save from v{save.SaveVersion} to v{CURRENT_SAVE_VERSION}");
                
                // Extensible version migration loop
                for (int v = save.SaveVersion; v < CURRENT_SAVE_VERSION; v++)
                {
                    MigrateVersion(save, v, v + 1);
                }

                save.SaveVersion = CURRENT_SAVE_VERSION;
                SaveGame();
            }
        }

        private void MigrateVersion(GameSaveData save, int fromVersion, int toVersion)
        {
            Debug.Log($"[SaveManager] Applied migration step from v{fromVersion} to v{toVersion}");
            // Future migration steps are placed here cleanly
        }

        public TimeSpan GetOfflineTimeElapsed()
        {
            if (_currentSave == null || string.IsNullOrEmpty(_currentSave.LastSaveTimeUtc))
            {
                return TimeSpan.Zero;
            }

            if (DateTime.TryParse(_currentSave.LastSaveTimeUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime lastSaveUtc))
            {
                TimeSpan elapsed = DateTime.UtcNow - lastSaveUtc;
                return elapsed > TimeSpan.Zero ? elapsed : TimeSpan.Zero;
            }

            return TimeSpan.Zero;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGame();
            }
        }

        protected override void OnApplicationQuit()
        {
            SaveGame();
            base.OnApplicationQuit();
        }
    }
}
