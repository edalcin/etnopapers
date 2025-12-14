using System;
using System.Collections.Generic;
using System.Windows;
using EtnoPapers.Core.Models;

namespace EtnoPapers.UI.Views
{
    /// <summary>
    /// EditRecordDialog - Modal dialog for editing an existing record
    /// </summary>
    public partial class EditRecordDialog : Window
    {
        public ArticleRecord? EditedRecord { get; set; }

        public EditRecordDialog(ArticleRecord record)
        {
            InitializeComponent();
            EditedRecord = record;
            LoadRecordData(record);
            DisplayExtractionTime(record);
        }

        private void LoadRecordData(ArticleRecord record)
        {
            TituloTextBox.Text = record.Titulo ?? "";
            AutoresTextBox.Text = string.Join(Environment.NewLine, record.Autores ?? new List<string>());
            AnoTextBox.Text = record.Ano?.ToString() ?? "";
            PaisTextBox.Text = record.Pais ?? "";
            EstadoTextBox.Text = record.Estado ?? "";
            MunicipioTextBox.Text = record.Municipio ?? "";
            BiomaTextBox.Text = record.Bioma ?? "";
            ResumoTextBox.Text = record.Resumo ?? "";
            LocalTextBox.Text = record.Local ?? "";
            MetodologiaTextBox.Text = record.Metodologia ?? "";

            // Load Community data
            if (record.Comunidade != null)
            {
                ComunidadeNomeTextBox.Text = record.Comunidade.Nome ?? "";
                ComunidadePovoTextBox.Text = record.Comunidade.Povo ?? "";
                ComunidadeLocalizacaoTextBox.Text = record.Comunidade.Localizacao ?? "";
            }

            // Load Plant Species as JSON
            if (record.Especies != null && record.Especies.Count > 0)
            {
                EspeciesTextBox.Text = Newtonsoft.Json.JsonConvert.SerializeObject(record.Especies, Newtonsoft.Json.Formatting.Indented);
            }
        }

        private void DisplayExtractionTime(ArticleRecord record)
        {
            if (record.TempoExtracao.HasValue)
            {
                double segundos = record.TempoExtracao.Value;
                string timeDisplay;

                if (segundos < 60)
                {
                    timeDisplay = $"{segundos:F1}s";
                }
                else if (segundos < 3600)
                {
                    int minutos = (int)(segundos / 60);
                    int segs = (int)(segundos % 60);
                    timeDisplay = $"{minutos}m {segs}s";
                }
                else
                {
                    int horas = (int)(segundos / 3600);
                    int minutos = (int)((segundos % 3600) / 60);
                    timeDisplay = $"{horas}h {minutos}m";
                }

                ExtractionTimeTextBlock.Text = timeDisplay;
            }
            else
            {
                ExtractionTimeTextBlock.Text = "N/A";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditedRecord == null)
                return;

            // Update record from UI
            EditedRecord.Titulo = TituloTextBox.Text;
            EditedRecord.Autores = string.IsNullOrWhiteSpace(AutoresTextBox.Text)
                ? new List<string>()
                : new List<string>(AutoresTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

            if (int.TryParse(AnoTextBox.Text, out var ano))
                EditedRecord.Ano = ano;

            EditedRecord.Pais = PaisTextBox.Text;
            EditedRecord.Estado = EstadoTextBox.Text;
            EditedRecord.Municipio = MunicipioTextBox.Text;
            EditedRecord.Bioma = BiomaTextBox.Text;
            EditedRecord.Resumo = ResumoTextBox.Text;
            EditedRecord.Local = LocalTextBox.Text;
            EditedRecord.Metodologia = MetodologiaTextBox.Text;

            // Save Community data
            if (!string.IsNullOrWhiteSpace(ComunidadeNomeTextBox.Text) ||
                !string.IsNullOrWhiteSpace(ComunidadePovoTextBox.Text) ||
                !string.IsNullOrWhiteSpace(ComunidadeLocalizacaoTextBox.Text))
            {
                if (EditedRecord.Comunidade == null)
                    EditedRecord.Comunidade = new EtnoPapers.Core.Models.Community();

                EditedRecord.Comunidade.Nome = ComunidadeNomeTextBox.Text;
                EditedRecord.Comunidade.Povo = ComunidadePovoTextBox.Text;
                EditedRecord.Comunidade.Localizacao = ComunidadeLocalizacaoTextBox.Text;
            }

            // Save Plant Species from JSON
            if (!string.IsNullOrWhiteSpace(EspeciesTextBox.Text))
            {
                try
                {
                    var especies = Newtonsoft.Json.JsonConvert.DeserializeObject<List<EtnoPapers.Core.Models.PlantSpecies>>(EspeciesTextBox.Text);
                    if (especies != null)
                        EditedRecord.Especies = especies;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao processar JSON das espécies: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            DialogResult = true;
            Close();
        }
    }
}
