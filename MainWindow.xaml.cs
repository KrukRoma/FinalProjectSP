using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ForbiddenWordsSearchApp
{
    public partial class MainWindow : Window
    {
        private List<string> forbiddenWords;
        private string outputFolderPath;
        private SearchManager searchManager;

        public MainWindow()
        {
            InitializeComponent();
            forbiddenWords = new List<string>();
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true 
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                outputFolderPath = dialog.FileName; 
                SelectedFolderPath.Text = outputFolderPath;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ForbiddenWordsInput.Text) || string.IsNullOrWhiteSpace(outputFolderPath))
            {
                MessageBox.Show("Будь ласка, введіть заборонені слова і виберіть папку для виводу.");
                return;
            }

            forbiddenWords = ForbiddenWordsInput.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(w => w.Trim()).ToList();

            if (forbiddenWords.Count == 0)
            {
                MessageBox.Show("Будь ласка, введіть хоча б одне заборонене слово.");
                return;
            }

            MessageBox.Show("Запускаємо пошук..."); 

            searchManager = new SearchManager(forbiddenWords, outputFolderPath, ResultTextBlock, SearchProgressBar, ProgressInfo);
            searchManager.StartSearch();
        }


        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            searchManager?.StopSearch();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            searchManager?.PauseSearch();
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            searchManager?.ResumeSearch();
        }
    }
}
