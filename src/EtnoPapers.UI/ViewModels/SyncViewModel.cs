using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EtnoPapers.Core.Models;
using EtnoPapers.Core.Services;
using EtnoPapers.UI.Commands;

namespace EtnoPapers.UI.ViewModels
{
    /// <summary>
    /// ViewModel for managing MongoDB synchronization of records.
    /// Handles record selection, sync workflow, and progress tracking.
    /// </summary>
    public class SyncViewModel : ViewModelBase
    {
        private readonly DataStorageService _storageService;
        private readonly MongoDBSyncService _syncService;
        private readonly ConfigurationService _configService;
        private readonly LoggerService _loggerService;

        private ObservableCollection<ArticleRecord> _availableRecords;
        private ObservableCollection<ArticleRecord> _selectedRecords;
        private bool _isSyncing = false;
        private int _syncProgress = 0;
        private string _currentSyncStatus = "";
        private string _mongodbUri = "";
        private bool _isMongoDBConnected = false;
        private bool _showSyncReminder = false;
        private string _syncReminderMessage = "";
        private string _errorMessage = "";
        private bool _hasError = false;
        private DateTime? _lastSyncTime;

        public SyncViewModel()
        {
            _storageService = new DataStorageService();
            _syncService = new MongoDBSyncService();
            _configService = new ConfigurationService();
            _loggerService = new LoggerService();

            _availableRecords = new ObservableCollection<ArticleRecord>();
            _selectedRecords = new ObservableCollection<ArticleRecord>();

            LoadRecordsCommand = new RelayCommand(_ => LoadAvailableRecords());
            TestConnectionCommand = new RelayCommand(_ => TestMongoDBConnection());
            StartSyncCommand = new RelayCommand(_ => StartSync(), _ => !IsSyncing && SelectedRecords.Count > 0 && IsMongoDBConnected);
            CancelSyncCommand = new RelayCommand(_ => CancelSync(), _ => IsSyncing);
            DismissSyncReminderCommand = new RelayCommand(_ => DismissSyncReminder());

            _loggerService.Info("SyncViewModel initialized");
        }

        #region Properties

        public ObservableCollection<ArticleRecord> AvailableRecords
        {
            get => _availableRecords;
            set => SetProperty(ref _availableRecords, value);
        }

        public ObservableCollection<ArticleRecord> SelectedRecords
        {
            get => _selectedRecords;
            set => SetProperty(ref _selectedRecords, value);
        }

        public bool IsSyncing
        {
            get => _isSyncing;
            set => SetProperty(ref _isSyncing, value);
        }

        public int SyncProgress
        {
            get => _syncProgress;
            set => SetProperty(ref _syncProgress, value);
        }

        public string CurrentSyncStatus
        {
            get => _currentSyncStatus;
            set => SetProperty(ref _currentSyncStatus, value);
        }

        public string MongodbUri
        {
            get => _mongodbUri;
            set => SetProperty(ref _mongodbUri, value);
        }

        public bool IsMongoDBConnected
        {
            get => _isMongoDBConnected;
            set => SetProperty(ref _isMongoDBConnected, value);
        }

        public bool ShowSyncReminder
        {
            get => _showSyncReminder;
            set => SetProperty(ref _showSyncReminder, value);
        }

        public string SyncReminderMessage
        {
            get => _syncReminderMessage;
            set => SetProperty(ref _syncReminderMessage, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public DateTime? LastSyncTime
        {
            get => _lastSyncTime;
            set => SetProperty(ref _lastSyncTime, value);
        }

        public int SelectedRecordCount => SelectedRecords.Count;

        #endregion

        #region Commands

        public ICommand LoadRecordsCommand { get; }
        public ICommand TestConnectionCommand { get; }
        public ICommand StartSyncCommand { get; }
        public ICommand CancelSyncCommand { get; }
        public ICommand DismissSyncReminderCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads all available records from local storage for sync selection.
        /// </summary>
        public void LoadAvailableRecords()
        {
            try
            {
                var allRecords = _storageService.LoadAll();
                AvailableRecords.Clear();
                foreach (var record in allRecords)
                {
                    AvailableRecords.Add(record);
                }

                CheckSyncReminder();
                _loggerService.Info($"Loaded {AvailableRecords.Count} records for sync");
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Erro ao carregar registros: {ex.Message}";
                _loggerService.Error($"Failed to load records: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Tests MongoDB connection using current URI configuration.
        /// </summary>
        public async void TestMongoDBConnection()
        {
            try
            {
                CurrentSyncStatus = "Testando conexão...";
                var config = _configService.LoadConfiguration();

                if (string.IsNullOrWhiteSpace(config?.MongodbUri))
                {
                    IsMongoDBConnected = false;
                    CurrentSyncStatus = "URI do MongoDB não configurado";
                    ErrorMessage = "Configure o URI do MongoDB nas configurações primeiro.";
                    HasError = true;
                    _loggerService.Warn("MongoDB URI not configured");
                    return;
                }

                // Test connection asynchronously
                IsMongoDBConnected = await _syncService.TestConnectionAsync(config.MongodbUri);

                if (IsMongoDBConnected)
                {
                    CurrentSyncStatus = "✓ Conectado ao MongoDB";
                    HasError = false;
                    ErrorMessage = "";
                    _loggerService.Info("MongoDB connection successful");
                }
                else
                {
                    CurrentSyncStatus = "✗ Erro ao conectar";
                    HasError = true;
                    ErrorMessage = "Não foi possível conectar ao MongoDB. Verifique as configurações e a disponibilidade do serviço.";
                    _loggerService.Error("MongoDB connection failed");
                }
            }
            catch (Exception ex)
            {
                IsMongoDBConnected = false;
                CurrentSyncStatus = "✗ Erro na conexão";
                HasError = true;
                ErrorMessage = $"Erro ao testar conexão: {ex.Message}";
                _loggerService.Error($"Error testing MongoDB connection: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Starts synchronization of selected records to MongoDB.
        /// </summary>
        public async void StartSync()
        {
            if (SelectedRecords.Count == 0 || !IsMongoDBConnected)
                return;

            IsSyncing = true;
            SyncProgress = 0;
            HasError = false;
            ErrorMessage = "";

            try
            {
                CurrentSyncStatus = "Iniciando sincronização...";
                var config = _configService.LoadConfiguration();

                if (string.IsNullOrWhiteSpace(config?.MongodbUri))
                {
                    throw new Exception("URI do MongoDB não configurado");
                }

                var recordsToSync = SelectedRecords.ToList();
                int successCount = 0;

                for (int i = 0; i < recordsToSync.Count; i++)
                {
                    var record = recordsToSync[i];
                    try
                    {
                        CurrentSyncStatus = $"Sincronizando: {i + 1}/{recordsToSync.Count}";

                        // Upload to MongoDB asynchronously
                        if (await _syncService.UploadRecordAsync(record))
                        {
                            // Delete local copy on success
                            _storageService.Delete(record.Id);
                            successCount++;
                        }

                        // Update progress
                        SyncProgress = (int)((i + 1) * 100 / recordsToSync.Count);
                    }
                    catch (Exception ex)
                    {
                        _loggerService.Error($"Error syncing record {record.Id}: {ex.Message}", ex);
                    }
                }

                CurrentSyncStatus = $"✓ Sincronização concluída: {successCount}/{recordsToSync.Count} registros";
                LastSyncTime = DateTime.UtcNow;
                SyncProgress = 100;

                _loggerService.Info($"Sync completed: {successCount} records uploaded successfully");

                // Reload available records after sync
                SelectedRecords.Clear();
                LoadAvailableRecords();
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Erro durante sincronização: {ex.Message}";
                CurrentSyncStatus = "✗ Erro na sincronização";
                _loggerService.Error($"Sync failed: {ex.Message}", ex);
            }
            finally
            {
                IsSyncing = false;
            }
        }

        /// <summary>
        /// Cancels ongoing synchronization.
        /// </summary>
        private void CancelSync()
        {
            IsSyncing = false;
            CurrentSyncStatus = "Sincronização cancelada";
            _loggerService.Info("Sync cancelled by user");
        }

        /// <summary>
        /// Checks if sync reminder should be shown based on record count and last sync time.
        /// </summary>
        private void CheckSyncReminder()
        {
            var recordCount = AvailableRecords.Count;
            var daysSinceLastSync = LastSyncTime.HasValue
                ? (DateTime.UtcNow - LastSyncTime.Value).TotalDays
                : 999;

            // Show reminder if:
            // - More than 500 records locally
            // - OR more than 7 days since last sync
            if (recordCount > 500)
            {
                ShowSyncReminder = true;
                SyncReminderMessage = $"Você tem {recordCount} registros locais. Recomenda-se sincronizar com MongoDB para fazer backup.";
            }
            else if (daysSinceLastSync > 7)
            {
                ShowSyncReminder = true;
                SyncReminderMessage = "Mais de 7 dias desde a última sincronização. Recomenda-se sincronizar agora.";
            }
            else
            {
                ShowSyncReminder = false;
            }
        }

        /// <summary>
        /// Dismisses the sync reminder.
        /// </summary>
        private void DismissSyncReminder()
        {
            ShowSyncReminder = false;
            _loggerService.Info("Sync reminder dismissed by user");
        }

        #endregion
    }
}
