using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ForbiddenWordsSearchApp
{
    public class FileProcessor
    {
        private FileInfo file;
        private List<string> forbiddenWords;
        private string outputFolderPath;
        private TextBlock resultTextBlock;
        private ProgressBar progressBar;
        private TextBlock progressInfo;
        private int processedFiles = 0;

        public FileProcessor(FileInfo file, List<string> forbiddenWords, string outputFolderPath, TextBlock resultTextBlock, ProgressBar progressBar, TextBlock progressInfo)
        {
            this.file = file;
            this.forbiddenWords = forbiddenWords;
            this.outputFolderPath = outputFolderPath;
            this.resultTextBlock = resultTextBlock;
            this.progressBar = progressBar;
            this.progressInfo = progressInfo;
        }

        public async Task Process(CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            try
            {
                var content = await File.ReadAllTextAsync(file.FullName);
                bool hasForbiddenWords = forbiddenWords.Any(word => content.Contains(word));

                if (hasForbiddenWords)
                {
                    var newContent = forbiddenWords.Aggregate(content, (current, word) => current.Replace(word, "*******"));
                    var newFilePath = Path.Combine(outputFolderPath, file.Name);
                    await File.WriteAllTextAsync(newFilePath, newContent);

                    resultTextBlock.Dispatcher.Invoke(() =>
                    {
                        resultTextBlock.Text += $"\nФайл {file.FullName} містить заборонені слова.";
                    });
                }

                UpdateProgress();
            }
            catch (Exception ex)
            {
                resultTextBlock.Dispatcher.Invoke(() =>
                {
                    resultTextBlock.Text += $"\nПомилка при обробці файлу {file.FullName}: {ex.Message}";
                });
            }
        }

        private void UpdateProgress()
        {
            processedFiles++;
            progressBar.Dispatcher.Invoke(() =>
            {
                progressBar.Value = processedFiles;
                progressInfo.Text = $"{processedFiles} файлів оброблено"; 
            });
        }
    }
}
