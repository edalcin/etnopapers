using System;
using System.Collections.Generic;
using System.Windows;
using EtnoPapers.Core.Models;

namespace EtnoPapers.UI.Views
{
    /// <summary>
    /// NewRecordDialog - Modal dialog for creating a new record from scratch
    /// </summary>
    public partial class NewRecordDialog : Window
    {
        public ArticleRecord? CreatedRecord { get; set; }

        public NewRecordDialog()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(TituloTextBox.Text))
            {
                MessageBox.Show("O título é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(AutoresTextBox.Text))
            {
                MessageBox.Show("Pelo menos um autor é obrigatório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(AnoTextBox.Text) || !int.TryParse(AnoTextBox.Text, out var ano))
            {
                MessageBox.Show("O ano é obrigatório e deve ser um número válido.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create new record
            CreatedRecord = new ArticleRecord
            {
                Id = Guid.NewGuid().ToString(),
                Titulo = TituloTextBox.Text,
                Autores = new List<string>(AutoresTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)),
                Ano = ano,
                DOI = string.IsNullOrWhiteSpace(DOITextBox.Text) ? "" : DOITextBox.Text,
                Resumo = string.IsNullOrWhiteSpace(ResumoTextBox.Text) ? "" : ResumoTextBox.Text,
                Comunidades = new List<Community>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = "pending"
            };

            DialogResult = true;
            Close();
        }
    }
}
