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
            DOITextBox.Text = record.DOI ?? "";
            ResumoTextBox.Text = record.Resumo ?? "";

            // Load Communities as JSON
            if (record.Comunidades != null && record.Comunidades.Count > 0)
            {
                ComunidadesTextBox.Text = Newtonsoft.Json.JsonConvert.SerializeObject(record.Comunidades, Newtonsoft.Json.Formatting.Indented);
            }
            else
            {
                ComunidadesTextBox.Text = "[]";
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

            EditedRecord.DOI = DOITextBox.Text;
            EditedRecord.Resumo = ResumoTextBox.Text;

            // Save Communities from JSON
            if (!string.IsNullOrWhiteSpace(ComunidadesTextBox.Text))
            {
                try
                {
                    var comunidades = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Community>>(ComunidadesTextBox.Text);
                    if (comunidades != null)
                        EditedRecord.Comunidades = comunidades;
                    else
                        EditedRecord.Comunidades = new List<Community>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao processar JSON das comunidades: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                EditedRecord.Comunidades = new List<Community>();
            }

            EditedRecord.DataUltimaAtualizacao = DateTime.UtcNow;

            DialogResult = true;
            Close();
        }
    }
}
