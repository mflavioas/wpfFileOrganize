using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace wpfFileOrganize
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public int QtdMaxImage { get; set; }

        private string SelecionarDiretorio()
        {
            CommonOpenFileDialog dialog = new();
            dialog.IsFolderPicker = true;

            // Exibir o diálogo e verificar se o usuário clicou em "OK"
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (dialog.FileName != null)
                    return dialog.FileName;
            }
            return string.Empty;
        }

        private void btnLocOrigem_Click(object sender, RoutedEventArgs e)
        {
            txtDirOrigem.Text = SelecionarDiretorio();
        }

        private void btnLocDestino_Click(object sender, RoutedEventArgs e)
        {
            txtDirDestino.Text = SelecionarDiretorio();
        }

        private void AddPainelDuplicados(List<string> duplicados)
        {
            gridDuplicados.RowDefinitions.Add(new RowDefinition());
            int NrLinhaGrid = gridDuplicados.RowDefinitions.Count - 1;
            //Criação do controle das labels de informação
            Grid grid = new()
            {
                Margin = new Thickness(0, 5, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            RichTextBox richTextBox = new RichTextBox();
            foreach (string item in duplicados)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                richTextBox.AppendText(string.Concat(item, Environment.NewLine));
                #region Imagem
                Image image = new()
                {
                    Margin = new Thickness(0, 5, 0, 0),
                    Height = 100,
                    Width = 100,
                    IsEnabled = false,
                    ToolTip = item
                };
                string imagePath = item;
                image.Source = new BitmapImage(new Uri(imagePath));
                grid.Children.Add(image);
                Grid.SetColumn(image, grid.ColumnDefinitions.Count - 1);
                #endregion
            }
            if (QtdMaxImage < grid.ColumnDefinitions.Count) { QtdMaxImage = grid.ColumnDefinitions.Count; }
            gridDuplicados.Children.Add(grid);
            gridDuplicados.Children.Add(richTextBox);
            Grid.SetColumn(grid, 0);
            Grid.SetRow(grid, NrLinhaGrid);

            Grid.SetColumn(richTextBox, 1);
            Grid.SetRow(richTextBox, NrLinhaGrid);
        }
        private void btnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (gridDuplicados.Children.Count > 0) { gridDuplicados.Children.RemoveAt(0); }
            DateTime dInicio = DateTime.Now;
            Dictionary<string, List<string>> fileHashes =
                Organizador.OrganizeImages(txtDirOrigem.Text, txtDirDestino.Text, cmbxTipoArquivo.SelectedIndex,
                 chkMover.IsChecked == true, ref pgbar);
            foreach (var pair in fileHashes)
            {
                AddPainelDuplicados(pair.Value);
            }
            gridDuplicados.ColumnDefinitions[0].Width = new GridLength(QtdMaxImage * 100);
            MessageBox.Show($"Processo concluido. Inicio: {dInicio:hh:mm:ss} - Fim: {DateTime.Now:hh:mm:ss}");
            pgbar.Value = 0;
        }
    }
}
