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

            DialogResult = true;
            Close();
        }
    }
}
