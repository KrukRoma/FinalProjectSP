using System.Collections.Generic;
using System.IO;

namespace ForbiddenWordsSearchApp
{
    public class ReportGenerator
    {
        private string outputFolderPath;

        public ReportGenerator(string outputFolderPath)
        {
            this.outputFolderPath = outputFolderPath;
        }

        public void GenerateReport(Dictionary<string, int> wordCount, int totalFiles, int processedFiles)
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
    }
}
