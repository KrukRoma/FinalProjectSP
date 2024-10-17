using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ForbiddenWordsSearchApp
{
    public class SearchManager
    {
        private List<string> forbiddenWords;
        private string outputFolderPath;
        private TextBlock resultTextBlock;
        private ProgressBar progressBar;
        private TextBlock progressInfo;
        private CancellationTokenSource cts;
        private int totalFiles = 0;
        private int processedFiles = 0;

        public SearchManager(List<string> forbiddenWords, string outputFolderPath, TextBlock resultTextBlock, ProgressBar progressBar, TextBlock progressInfo)
        {
            this.forbiddenWords = forbiddenWords;
            this.outputFolderPath = outputFolderPath;
            this.resultTextBlock = resultTextBlock;
            this.progressBar = progressBar;
            this.progressInfo = progressInfo;
        }

        public void StartSearch()
        {
            cts = new CancellationTokenSource();
            processedFiles = 0;
            resultTextBlock.Text = "";

            Task.Run(() => SearchFiles(cts.Token));
        }

        public void StopSearch()
        {
            cts?.Cancel();
        }

        public void PauseSearch()
        {
            cts?.Cancel();
        }

        public void ResumeSearch()
        {
            StartSearch();
        }

        private async Task SearchFiles(CancellationToken token)
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
            totalFiles = drives.SelectMany(drive => Directory.GetFiles(drive.RootDirectory.FullName, "*.*", SearchOption.AllDirectories)).Count();
            progressBar.Dispatcher.Invoke(() => {
                progressBar.Maximum = totalFiles;
                progressInfo.Text = $"0 з {totalFiles} файлів оброблено"; // Додайте початковий текст
            });

            foreach (var drive in drives)
            {
                await SearchInDirectory(drive.RootDirectory, token);
            }

            resultTextBlock.Dispatcher.Invoke(() =>
            {
                resultTextBlock.Text += "\nПошук завершено.";
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

            var fileProcessor = new FileProcessor(file, forbiddenWords, outputFolderPath, resultTextBlock, progressBar, progressInfo);
            await fileProcessor.Process(token);
        }

        private void GenerateReport()
        {
            var reportGenerator = new ReportGenerator(outputFolderPath);
            reportGenerator.GenerateReport(new Dictionary<string, int>(), totalFiles, processedFiles);
        }

    }
}
