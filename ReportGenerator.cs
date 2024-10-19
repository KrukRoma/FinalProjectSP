using System;
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

        public void GenerateReport(Dictionary<string, int> results, int totalFiles, int processedFiles)
        {
            string reportPath = Path.Combine(outputFolderPath, "Report.txt");

            using (var writer = new StreamWriter(reportPath))
            {
                writer.WriteLine("Звіт про пошук заборонених слів");
                writer.WriteLine($"Загальна кількість файлів: {totalFiles}");
                writer.WriteLine($"Оброблених файлів: {processedFiles}");
                writer.WriteLine("Заборонені слова:");

                foreach (var result in results)
                {
                    writer.WriteLine($"{result.Key}: {result.Value}");
                }
            }
        }
    }
}
