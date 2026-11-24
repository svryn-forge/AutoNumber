using Autodesk.Revit.UI;
using Svrin.Core.Abstractions;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Svrin.App.Revit.Helpers
{
    internal static class NumberingConflictLogger
    {
        public static bool ShowConflicts(IReadOnlyCollection<NumberingConflict> conflicts)
        {
            if (conflicts == null || conflicts.Count == 0)
                return true;

            var logDirectory = AppPaths.LogDirectory;

            Directory.CreateDirectory(logDirectory);

            var fileName = string.Format(
                "NumberingConflicts_{0:yyyy-MM-dd_HH-mm-ss}.txt",
                DateTime.Now);

            var filePath = Path.Combine(logDirectory, fileName);

            var lines = new List<string>
            {
                "ОТЧЁТ О КОНФЛИКТАХ НУМЕРАЦИИ",
                $"Дата: {DateTime.Now:dd.MM.yyyy HH:mm:ss}",
                $"Количество конфликтов: {conflicts.Count}",
                new string('-', 70),
                string.Empty
            };

            var index = 1;

            foreach (var conflict in conflicts)
            {
                var source = conflict.Source;
                if (source == null)
                    continue;

                lines.Add($"Конфликт №{index++}");
                lines.Add($"Элемент: {source.Name}");
                lines.Add($"ID элемента: {source.Id}");
                lines.Add($"Новое значение: {conflict.NewValue}");
                lines.Add($"Конфликтующий элемент: {conflict.ConflictingElementName}");
                lines.Add($"ID конфликтующего элемента: {conflict.ConflictingElementId}");
                lines.Add($"Текущий номер: {conflict.CurrentValue}");
                lines.Add(new string('-', 70));
            }

            File.WriteAllLines(filePath, lines, Encoding.UTF8);

            var continueDialog = new TaskDialog("Конфликты нумерации")
            {
                MainInstruction = "Обнаружены конфликты нумерации",
                MainContent =
                    $"Найдено конфликтов: {conflicts.Count}.{Environment.NewLine}" +
                    "Конфликтующие элементы будут пропущены." +
                    $"{Environment.NewLine}{Environment.NewLine}" +
                    "Продолжить нумерацию остальных элементов?",
                CommonButtons = TaskDialogCommonButtons.Yes |
                                TaskDialogCommonButtons.No,
                DefaultButton = TaskDialogResult.No
            };

            var result = continueDialog.Show();

            if (result != TaskDialogResult.Yes)
                return false;

            var openDialog = new TaskDialog("Отчёт о конфликтах")
            {
                MainInstruction = "Отчёт сохранён",
                MainContent =
                    $"Файл отчёта:{Environment.NewLine}{filePath}" +
                    $"{Environment.NewLine}{Environment.NewLine}" +
                    "Открыть отчёт?",
                CommonButtons = TaskDialogCommonButtons.Yes |
                                TaskDialogCommonButtons.No,
                DefaultButton = TaskDialogResult.Yes
            };

            if (openDialog.Show() == TaskDialogResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    TaskDialog.Show(
                        "Ошибка",
                        $"Не удалось открыть отчёт.{Environment.NewLine}{ex.Message}");
                }
            }

            return true;
        }
    }
}
