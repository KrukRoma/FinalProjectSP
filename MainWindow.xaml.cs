using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ForbiddenWordsSearchApp
{
    public partial class MainWindow : Window
    {
        private List<string> forbiddenWords;
        private string outputFolderPath;
        private CancellationTokenSource cts;
        private Task searchTask;
        private int totalFiles = 0;
        private int processedFiles = 0;
        private Dictionary<string, int> wordCount;
        private object lockObject = new object();

        public MainWindow()
        {
            InitializeComponent();
            forbiddenWords = new List<string>();
            wordCount = new Dictionary<string, int>();
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

            StartSearch();
        }

        private void StartSearch()
        {
            cts = new CancellationTokenSource();
            PauseButton.IsEnabled = true;
            StopButton.IsEnabled = true;
            StartButton.IsEnabled = false;
            processedFiles = 0;
            wordCount.Clear();
            ResultTextBlock.Text = "";

            searchTask = Task.Run(() => SearchFiles(cts.Token));
        }

        private async Task SearchFiles(CancellationToken token)
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
            totalFiles = drives.SelectMany(drive => Directory.GetFiles(drive.RootDirectory.FullName, "*.*", SearchOption.AllDirectories)).Count();
            SearchProgressBar.Dispatcher.Invoke(() => {
                SearchProgressBar.Maximum = totalFiles;
                ProgressInfo.Text = $"0 з {totalFiles} файлів оброблено"; 
            });

            foreach (var drive in drives)
            {
                await SearchInDirectory(drive.RootDirectory, token);
            }

            Dispatcher.Invoke(() =>
            {
                ResultTextBlock.Text += "\nПошук завершено.";
                StartButton.IsEnabled = true;
                PauseButton.IsEnabled = false;
                ResumeButton.IsEnabled = false;
                StopButton.IsEnabled = false;
            });

            GenerateReport();
        }

        private async Task SearchInDirectory(DirectoryInfo directory, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            try
            {
                foreach (var file in directory.GetFiles())
                {
                    if (token.IsCancellationRequested) return;
                    await ProcessFile(file, token);
                }

                foreach (var subDirectory in directory.GetDirectories())
                {
                    if (token.IsCancellationRequested) return;
                    await SearchInDirectory(subDirectory, token);
                }
            }
            catch (UnauthorizedAccessException)
            {

            }
        }

        private async Task ProcessFile(FileInfo file, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            try
            {
                var content = await File.ReadAllTextAsync(file.FullName);
                bool hasForbiddenWords = false;
                foreach (var word in forbiddenWords)
                {
                    if (content.Contains(word))
                    {
                        hasForbiddenWords = true;
                        lock (lockObject)
                        {
                            if (!wordCount.ContainsKey(word))
                            {
                                wordCount[word] = 0;
                            }
                            wordCount[word]++;
                        }
                    }
                }

                if (hasForbiddenWords)
                {
                    var newContent = forbiddenWords.Aggregate(content, (current, word) => current.Replace(word, "*******"));
                    var newFilePath = Path.Combine(outputFolderPath, file.Name);
                    await File.WriteAllTextAsync(newFilePath, newContent);

                    Dispatcher.Invoke(() =>
                    {
                        ResultTextBlock.Text += $"\nФайл {file.FullName} містить заборонені слова.";
                    });
                }

                Dispatcher.Invoke(() =>
                {
                    processedFiles++;
                    SearchProgressBar.Value = processedFiles;
                    ProgressInfo.Text = $"{processedFiles} з {totalFiles} файлів оброблено"; 
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    ResultTextBlock.Text += $"\nПомилка при обробці файлу {file.FullName}: {ex.Message}";
                });
            }
        }

        private void GenerateReport()
        {
            var reportFilePath = Path.Combine(outputFolderPath, "Report.txt");
            using (var writer = new StreamWriter(reportFilePath))
            {
                writer.WriteLine("Звіт про заборонені слова");
                writer.WriteLine("=========================");
                writer.WriteLine($"Всього оброблено файлів: {totalFiles}");
                writer.WriteLine($"Файлів із забороненими словами: {processedFiles}");
                writer.WriteLine("Кількість заборонених слів:");

                foreach (var word in wordCount.OrderByDescending(w => w.Value).Take(10))
                {
                    writer.WriteLine($"{word.Key}: {word.Value} входжень");
                }
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            ResumeButton.IsEnabled = true;
            PauseButton.IsEnabled = false;
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            StartSearch();
            ResumeButton.IsEnabled = false;
            PauseButton.IsEnabled = true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            StartButton.IsEnabled = true;
            PauseButton.IsEnabled = false;
            ResumeButton.IsEnabled = false;
            StopButton.IsEnabled = false;
        }
    }
}
