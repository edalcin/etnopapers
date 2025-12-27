using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EtnoPapers.Core.Models;
using EtnoPapers.Core.Services;
using EtnoPapers.UI.Commands;

namespace EtnoPapers.UI.ViewModels
{
    /// <summary>
    /// ViewModel for managing (viewing, filtering, editing, deleting) local records.
    /// Provides CRUD operations and filtering for the RecordsPage.
    /// </summary>
    public class RecordsViewModel : ViewModelBase
    {
        private readonly DataStorageService _storageService;
        private readonly ValidationService _validationService;
        private readonly LoggerService _loggerService;

        private ObservableCollection<ArticleRecord> _records;
        private ObservableCollection<ArticleRecord> _filteredRecords;
        private string _searchText = "";
        private int? _yearFilterMin;
        private int? _yearFilterMax;
        private string _authorFilter = "";
        private string _biomeFilter = "";
        private bool _isLoading = false;
        private bool _hasError = false;
        private string _errorMessage = "";

        public RecordsViewModel()
        {
            _storageService = new DataStorageService();
            _validationService = new ValidationService();
            _loggerService = new LoggerService();

            _records = new ObservableCollection<ArticleRecord>();
            _filteredRecords = new ObservableCollection<ArticleRecord>();

            LoadRecordsCommand = new RelayCommand(_ => LoadRecords());
            DeleteRecordsCommand = new RelayCommand(_ => DeleteSelectedRecords(), _ => !IsLoading && SelectedRecords.Count > 0);
            ApplyFiltersCommand = new RelayCommand(_ => ApplyFilters());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());

            _loggerService.Info("RecordsViewModel initialized");
        }

        #region Properties

        public ObservableCollection<ArticleRecord> Records
        {
            get => _records;
            set => SetProperty(ref _records, value);
        }

        public ObservableCollection<ArticleRecord> FilteredRecords
        {
            get => _filteredRecords;
            set => SetProperty(ref _filteredRecords, value);
        }

        public ObservableCollection<ArticleRecord> SelectedRecords { get; } = new ObservableCollection<ArticleRecord>();

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public int? YearFilterMin
        {
            get => _yearFilterMin;
            set => SetProperty(ref _yearFilterMin, value);
        }

        public int? YearFilterMax
        {
            get => _yearFilterMax;
            set => SetProperty(ref _yearFilterMax, value);
        }

        public string AuthorFilter
        {
            get => _authorFilter;
            set => SetProperty(ref _authorFilter, value);
        }

        public string BiomeFilter
        {
            get => _biomeFilter;
            set => SetProperty(ref _biomeFilter, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public int RecordCount => FilteredRecords.Count;

        #endregion

        #region Commands

        public ICommand LoadRecordsCommand { get; }
        public ICommand DeleteRecordsCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads all records from storage.
        /// </summary>
        public void LoadRecords()
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = "";

            try
            {
                var loadedRecords = _storageService.LoadAll();
                Records.Clear();
                foreach (var record in loadedRecords)
                {
                    Records.Add(record);
                }

                FilteredRecords.Clear();
                foreach (var record in Records)
                {
                    FilteredRecords.Add(record);
                }

                _loggerService.Info($"Loaded {Records.Count} records from storage");
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Erro ao carregar registros: {ex.Message}";
                _loggerService.Error($"Failed to load records: {ex.Message}", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Edits an existing record.
        /// </summary>
        public void EditRecord(ArticleRecord record)
        {
            if (record == null)
                return;

            try
            {
                if (!_validationService.IsValidForSaving(record))
                {
                    ErrorMessage = "Os dados do registro não passam na validação. Verifique os campos obrigatórios.";
                    _loggerService.Warn("Record failed validation during edit");
                    return;
                }

                if (_storageService.Update(record))
                {
                    _loggerService.Info($"Record updated: {record.Id}");
                    LoadRecords(); // Reload to reflect changes
                }
                else
                {
                    ErrorMessage = "Erro ao atualizar o registro.";
                    _loggerService.Error("Failed to update record");
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Erro ao editar registro: {ex.Message}";
                _loggerService.Error($"Error editing record: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Saves a new record created from scratch.
        /// </summary>
        public bool SaveNewRecord(ArticleRecord record)
        {
            if (record == null)
                return false;

            try
            {
                if (!_validationService.IsValidForSaving(record))
                {
                    ErrorMessage = "Os dados do novo registro não passam na validação. Verifique os campos obrigatórios.";
                    _loggerService.Warn("New record failed validation");
                    return false;
                }

                if (_storageService.Create(record))
                {
                    _loggerService.Info($"New record created: {record.Id}");
                    LoadRecords();
                    return true;
                }
                else
                {
                    ErrorMessage = "Erro ao salvar o novo registro.";
                    _loggerService.Error("Failed to create record");
                    return false;
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Erro ao criar registro: {ex.Message}";
                _loggerService.Error($"Error creating record: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Deletes a single record.
        /// </summary>
        public bool DeleteRecord(string recordId)
        {
            if (string.IsNullOrEmpty(recordId))
                return false;

            try
            {
                if (_storageService.Delete(recordId))
                {
                    _loggerService.Info($"Record deleted: {recordId}");
                    LoadRecords();
                    return true;
                }
                else
                {
                    ErrorMessage = "Erro ao deletar o registro.";
                    _loggerService.Error("Failed to delete record");
                    return false;
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Erro ao deletar registro: {ex.Message}";
                _loggerService.Error($"Error deleting record: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Deletes multiple selected records.
        /// </summary>
        private void DeleteSelectedRecords()
        {
            if (SelectedRecords.Count == 0)
                return;

            int deletedCount = 0;
            foreach (var record in SelectedRecords.ToList())
            {
                if (DeleteRecord(record.Id))
                    deletedCount++;
            }

            SelectedRecords.Clear();
            _loggerService.Info($"Deleted {deletedCount} records");
        }

        /// <summary>
        /// Applies filters to the record list based on search and filter criteria.
        /// </summary>
        public void ApplyFilters()
        {
            FilteredRecords.Clear();

            var filtered = Records.AsEnumerable();

            // Search text filter (searches title and authors)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLowerInvariant();
                filtered = filtered.Where(r =>
                    (!string.IsNullOrEmpty(r.Titulo) && r.Titulo.ToLowerInvariant().Contains(searchLower)) ||
                    (r.Autores != null && r.Autores.Any(a => a.ToLowerInvariant().Contains(searchLower)))
                );
            }

            // Year range filter
            if (YearFilterMin.HasValue)
                filtered = filtered.Where(r => r.Ano >= YearFilterMin.Value);

            if (YearFilterMax.HasValue)
                filtered = filtered.Where(r => r.Ano <= YearFilterMax.Value);

            // Author filter (contains)
            if (!string.IsNullOrWhiteSpace(AuthorFilter))
            {
                var authorLower = AuthorFilter.ToLowerInvariant();
                filtered = filtered.Where(r =>
                    r.Autores != null && r.Autores.Any(a => a.ToLowerInvariant().Contains(authorLower))
                );
            }

            // Biome filter removed - field no longer exists in ArticleRecord

            foreach (var record in filtered)
            {
                FilteredRecords.Add(record);
            }

            OnPropertyChanged(nameof(RecordCount));
            _loggerService.Info($"Applied filters; {FilteredRecords.Count} records after filtering");
        }

        /// <summary>
        /// Clears all filters and shows all records.
        /// </summary>
        private void ClearFilters()
        {
            SearchText = "";
            YearFilterMin = null;
            YearFilterMax = null;
            AuthorFilter = "";
            BiomeFilter = "";

            FilteredRecords.Clear();
            foreach (var record in Records)
            {
                FilteredRecords.Add(record);
            }

            OnPropertyChanged(nameof(RecordCount));
            _loggerService.Info("Filters cleared; all records shown");
        }

        #endregion
    }
}
