using System;
using EtnoPapers.Core.Services;

namespace EtnoPapers.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the home/dashboard page.
    /// Shows welcome information and quick start actions.
    /// </summary>
    public class HomeViewModel : ViewModelBase
    {
        private readonly DataStorageService _storageService;
        private int _recordCount = 0;
        private string _welcomeMessage = "Bem-vindo ao EtnoPapers";

        public HomeViewModel()
        {
            _storageService = new DataStorageService();
            LoadData();
        }

        public int RecordCount
        {
            get => _recordCount;
            set => SetProperty(ref _recordCount, value);
        }

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        public string ApplicationVersion => "2.1.0";

        private void LoadData()
        {
            try
            {
                _storageService.Initialize();
                RecordCount = _storageService.Count();
                WelcomeMessage = $"Bem-vindo! Você tem {RecordCount} registros locais.";
            }
            catch (Exception ex)
            {
                WelcomeMessage = "Erro ao carregar dados.";
            }
        }
    }
}
