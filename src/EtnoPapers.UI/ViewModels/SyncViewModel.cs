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
        private bool _canStartSync = false;

        public SyncViewModel()
        {
            try
            {
                _loggerService = new LoggerService();
                _loggerService.Info("SyncViewModel constructor started");

                _loggerService.Info("Creating DataStorageService...");
                _storageService = new DataStorageService();
                _loggerService.Info("DataStorageService created");

                _loggerService.Info("Creating MongoDBSyncService...");
                _syncService = new MongoDBSyncService();
                _loggerService.Info("MongoDBSyncService created");

                _loggerService.Info("Creating ConfigurationService...");
                _configService = new ConfigurationService();
                _loggerService.Info("ConfigurationService created");

                _loggerService.Info("Creating ObservableCollections...");
                _availableRecords = new ObservableCollection<ArticleRecord>();
                _selectedRecords = new ObservableCollection<ArticleRecord>();

                // Monitor SelectedRecords changes to update CanStartSync
                _selectedRecords.CollectionChanged += (s, e) =>
                {
                    UpdateCanStartSync();
                };

                _loggerService.Info("ObservableCollections created");

                _loggerService.Info("Creating commands...");
                LoadRecordsCommand = new RelayCommand(_ => LoadAvailableRecords());
                TestConnectionCommand = new AsyncRelayCommand(_ => TestMongoDBConnection());
                // StartSyncCommand: sem predicate - deixar o IsEnabled binding no XAML controlar
                StartSyncCommand = new AsyncRelayCommand(_ => StartSync());
                CancelSyncCommand = new RelayCommand(_ => CancelSync(), _ => IsSyncing);
                DismissSyncReminderCommand = new RelayCommand(_ => DismissSyncReminder());
                _loggerService.Info("Commands created");

                _loggerService.Info("SyncViewModel initialized successfully");
            }
            catch (Exception ex)
            {
                _loggerService.Error($"CRITICAL ERROR in SyncViewModel constructor: {ex.GetType().Name}: {ex.Message}", ex);
                _loggerService.Error($"Stack trace: {ex.StackTrace}");
                throw;
            }
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
            set
            {
                if (SetProperty(ref _isSyncing, value))
                {
                    UpdateCanStartSync();
                }
            }
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
            set
            {
                if (SetProperty(ref _isMongoDBConnected, value))
                {
                    UpdateCanStartSync();
                }
            }
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

        public bool CanStartSync
        {
            get => _canStartSync;
            set => SetProperty(ref _canStartSync, value);
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
                _loggerService.Info("LoadAvailableRecords starting...");

                try
                {
                    _loggerService.Info("Calling _storageService.LoadAll()...");
                    var allRecords = _storageService.LoadAll();
                    _loggerService.Info($"LoadAll returned {allRecords?.Count ?? 0} records");

                    _loggerService.Info("Clearing AvailableRecords...");
                    AvailableRecords.Clear();

                    if (allRecords != null)
                    {
                        _loggerService.Info($"Adding {allRecords.Count} records to AvailableRecords...");
                        foreach (var record in allRecords)
                        {
                            if (record == null)
                            {
                                _loggerService.Warn("Skipping null record");
                                continue;
                            }
                            AvailableRecords.Add(record);
                        }
                    }

                    _loggerService.Info($"Checking sync reminder with {AvailableRecords.Count} records...");
                    CheckSyncReminder();

                    // Atualizar estado do botão Sincronizar
                    UpdateCanStartSync();

                    _loggerService.Info($"LoadAvailableRecords completed successfully. Loaded {AvailableRecords.Count} records");
                }
                catch (Exception storageEx)
                {
                    _loggerService.Error($"CRITICAL ERROR in LoadAvailableRecords (storage): {storageEx.GetType().Name}: {storageEx.Message}", storageEx);
                    HasError = true;
                    ErrorMessage = $"Erro ao carregar registros: {storageEx.GetType().Name} - {storageEx.Message}";
                    throw;
                }
            }
            catch (Exception ex)
            {
                _loggerService.Error($"CRITICAL ERROR in LoadAvailableRecords: {ex.GetType().Name}: {ex.Message}", ex);
                _loggerService.Error($"Stack trace: {ex.StackTrace}");
                HasError = true;
                ErrorMessage = $"Erro ao carregar registros: {ex.Message}";
            }
        }

        /// <summary>
        /// Tests MongoDB connection using current URI configuration.
        /// </summary>
        public async Task TestMongoDBConnection()
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

                try
                {
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

                    // Atualizar estado do botão após testar conexão
                    UpdateCanStartSync();
                }
                catch (Exception serviceEx)
                {
                    IsMongoDBConnected = false;
                    CurrentSyncStatus = "✗ Erro na conexão";
                    HasError = true;
                    ErrorMessage = $"Erro ao testar conexão: {serviceEx.Message}";
                    _loggerService.Error($"Error in MongoDBSyncService.TestConnectionAsync: {serviceEx.Message}", serviceEx);
                }
            }
            catch (Exception ex)
            {
                IsMongoDBConnected = false;
                CurrentSyncStatus = "✗ Erro na conexão";
                HasError = true;
                ErrorMessage = $"Erro inesperado ao testar conexão: {ex.Message}";
                _loggerService.Error($"Unexpected error testing MongoDB connection: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Starts synchronization of selected records to MongoDB.
        /// </summary>
        public async Task StartSync()
        {
            _loggerService.Info("================================");
            _loggerService.Info("StartSync method CALLED");
            _loggerService.Info($"  - SelectedRecords.Count={SelectedRecords.Count}");
            _loggerService.Info($"  - IsMongoDBConnected={IsMongoDBConnected}");
            _loggerService.Info($"  - IsSyncing={IsSyncing}");

            if (SelectedRecords.Count == 0 || !IsMongoDBConnected)
            {
                _loggerService.Warn("StartSync called but conditions not met: SelectedRecordsCount=" + SelectedRecords.Count + ", IsConnected=" + IsMongoDBConnected);
                System.Windows.MessageBox.Show("Não é possível sincronizar:\n- Registros selecionados: " + SelectedRecords.Count + "\n- MongoDB conectado: " + IsMongoDBConnected, "Sincronização Bloqueada");
                return;
            }

            _loggerService.Info("Conditions met. Proceeding with synchronization...");

            IsSyncing = true;
            SyncProgress = 0;
            HasError = false;
            ErrorMessage = "";

            _loggerService.Info("SET IsSyncing = true");
            _loggerService.Info("Starting synchronization...");
            CurrentSyncStatus = "Iniciando sincronização...";

            try
            {
                var config = _configService.LoadConfiguration();

                if (string.IsNullOrWhiteSpace(config?.MongodbUri))
                {
                    throw new InvalidOperationException("URI do MongoDB não configurado");
                }

                // Make a copy to avoid modification during iteration
                var recordsToSync = new List<ArticleRecord>(SelectedRecords);
                _loggerService.Info($"Preparing to sync {recordsToSync.Count} records");

                int successCount = 0;

                for (int i = 0; i < recordsToSync.Count; i++)
                {
                    try
                    {
                        var record = recordsToSync[i];
                        if (record == null)
                        {
                            _loggerService.Warn($"Skipping null record at index {i}");
                            continue;
                        }

                        CurrentSyncStatus = $"Sincronizando: {i + 1}/{recordsToSync.Count} - {record.Titulo}";
                        _loggerService.Debug($"Uploading record: {record.Id}");

                        // Upload to MongoDB asynchronously
                        if (await _syncService.UploadRecordAsync(record))
                        {
                            _loggerService.Debug($"Successfully uploaded record: {record.Id}");
                            // Delete local copy on success
                            _storageService.Delete(record.Id);
                            successCount++;
                        }
                        else
                        {
                            _loggerService.Warn($"Failed to upload record: {record.Id}");
                        }

                        // Update progress
                        SyncProgress = (int)((i + 1) * 100 / recordsToSync.Count);
                    }
                    catch (Exception ex)
                    {
                        _loggerService.Error($"Error syncing record at index {i}: {ex.Message}", ex);
                        // Continue with next record even if one fails
                    }
                }

                CurrentSyncStatus = $"✓ Sincronização concluída: {successCount}/{recordsToSync.Count} registros";
                LastSyncTime = DateTime.UtcNow;
                SyncProgress = 100;

                _loggerService.Info($"Sync completed: {successCount}/{recordsToSync.Count} records uploaded successfully");

                // Mostrar messageBox de sucesso
                System.Windows.MessageBox.Show($"Sincronização concluída com sucesso!\n\n{successCount} de {recordsToSync.Count} registros enviados para MongoDB.", "Sincronização OK");

                // Reload available records after sync
                try
                {
                    SelectedRecords.Clear();
                    LoadAvailableRecords();
                }
                catch (Exception ex)
                {
                    _loggerService.Error($"Error reloading records after sync: {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Erro durante sincronização: {ex.Message}";
                CurrentSyncStatus = "✗ Erro na sincronização";
                _loggerService.Error($"Sync failed with exception: {ex.GetType().Name}: {ex.Message}", ex);

                // Mostrar messageBox de erro
                System.Windows.MessageBox.Show($"Erro na sincronização:\n\n{ex.Message}", "Erro na Sincronização");
            }
            finally
            {
                IsSyncing = false;
                _loggerService.Info("Sync operation completed (IsSyncing=false)");
                _loggerService.Info("================================");
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

        /// <summary>
        /// Updates the CanStartSync property based on current conditions.
        /// Enables the button only if registros are selected AND MongoDB is connected.
        /// </summary>
        private void UpdateCanStartSync()
        {
            bool hasSelectedRecords = SelectedRecords.Count > 0;
            bool isConnected = IsMongoDBConnected;
            bool notSyncing = !IsSyncing;

            bool canStart = hasSelectedRecords && isConnected && notSyncing;

            _loggerService.Info($"UpdateCanStartSync called: canStart={canStart}");
            _loggerService.Info($"  - SelectedRecords.Count={SelectedRecords.Count} (hasSelected={hasSelectedRecords})");
            _loggerService.Info($"  - IsMongoDBConnected={IsMongoDBConnected} (isConnected={isConnected})");
            _loggerService.Info($"  - IsSyncing={IsSyncing} (notSyncing={notSyncing})");

            if (CanStartSync != canStart)
            {
                CanStartSync = canStart;
                _loggerService.Info($"CanStartSync property updated to: {canStart}");
            }
            else
            {
                _loggerService.Debug($"CanStartSync value unchanged: {canStart}");
            }
        }

        #endregion
    }
}
