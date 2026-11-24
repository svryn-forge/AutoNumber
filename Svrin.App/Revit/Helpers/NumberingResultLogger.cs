using Autodesk.Revit.UI;
using Svrin.Core.Abstractions;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Svrin.App.Revit.Helpers
{
    internal static class NumberingResultLogger
    {
        public static void ShowResult(NumberingResult result, string version)
        {
            var logDirectory = AppPaths.LogDirectory;

            Directory.CreateDirectory(logDirectory);

            var fileName = $"NumberingResult_{version}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            var filePath = Path.Combine(logDirectory, fileName);

            var lines = new List<string>
            {
                "РЕЗУЛЬТАТ НУМЕРАЦИИ",
                $"Дата: {DateTime.Now:dd.MM.yyyy HH:mm:ss}",
                new string('-', 50),
                $"Всего элементов: {result.Total}",
                $"Успешно изменено: {result.Success}",
                $"Пропущено: {result.Skipped}",
                new string('-', 50),
                "ПОДРОБНОСТИ",
                string.Empty
            };

            lines.AddRange(result.Details);

            File.WriteAllLines(filePath, lines, Encoding.UTF8);

            var dialog = new TaskDialog("Результат нумерации")
            {
                MainInstruction = "Операция завершена",
                MainContent =
                    $"Всего элементов: {result.Total}\n" +
                    $"Успешно изменено: {result.Success}\n" +
                    $"Пропущено: {result.Skipped}\n" +
                    $"Отчёт сохранён:\n{filePath}",
                CommonButtons = TaskDialogCommonButtons.Yes |
                                TaskDialogCommonButtons.No,
                DefaultButton = TaskDialogResult.Yes
            };

            dialog.MainInstruction = "Открыть отчёт?";

            if (dialog.Show() == TaskDialogResult.Yes)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
        }
    }
}
