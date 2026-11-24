using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Svrin.App.Revit;
using Svrin.App.Revit.Extensions;
using Svrin.App.Revit.Helpers;
using Svrin.App.Revit.Models;
using Svrin.Core.Abstractions;
using Svrin.Core.Helpers;
using Svrin.Wpf.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Svrin.App.Commands.Renumber.EventHandlers
{
    internal class ApplyEventHandler : RvtEventHandler<NumberingVM?>
    {
        const string ErrorText = "В процессе заполнения параметров возникла ошибка.";
        
        public override void Execute(UIApplication app, NumberingVM? vm)
        {
            if (vm == null)
                return;
            string? documentTitle = vm.CurrentDocument;
            if (string.IsNullOrWhiteSpace(documentTitle))
                return;

            var collection = vm.Elements;
            if (collection == null || collection.Count == 0)
                return;

            bool hasErrors = false;
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                if (!app.IsValid())
                    return;

                var uidoc = app.ActiveUIDocument;
                if (!uidoc.IsValid())
                    return;

                var doc = uidoc.Document;
                if (!doc.IsValid())
                    return;

                var title = doc.Title;

                if (!string.Equals(
                    title,
                    documentTitle,
                    StringComparison.InvariantCulture))
                    return;

                var conflicts = NumberingConflictChecker.FindDuplicateNewValues(collection);

                IReadOnlyCollection<ElementFromModel> fromModels;
                if (conflicts.Any())
                {
                    var shouldContinue = NumberingConflictLogger.ShowConflicts(conflicts);

                    if (!shouldContinue)
                    {
                        return;
                    }    

                    var conflictingIds = new HashSet<long>(
                        conflicts
                        .Where(x => x.Source != null)
                        .Select(x => x.Source!.Id));

                    fromModels = new ObservableCollection<ElementFromModel>(collection
                        .Where(x => !conflictingIds.Contains(x.Id)));

                    if (fromModels.Count == 0)
                        return;
                }
                else
                {
                    fromModels = collection;
                }

                var result = new NumberingResult()
                {
                    Total = fromModels.Count
                };

                var preprocessor = new TransactionPreprocessor();
                var status = TransactionsUtil.StartsTransaction(
                    doc,
                    "Заполнение параметра",
                    TransactionStart,
                    fromModels,
                    result,
                    doc,
                    out var error,
                    preprocessor);

                if (string.IsNullOrWhiteSpace(error))
                {
                    if (status != TransactionStatus.Committed)
                    {
                        result.Details.Add($"Транзакция завершилась со статусом {status}.");
                    }
                    if (preprocessor.Warnings.Count > 0)
                    {
                        result.Details.Add("Preprocessor Warnings");
                        result.Details.AddRange(preprocessor.Warnings);
                        result.Details.Add(Environment.NewLine);
                    }

                    NumberingResultLogger.ShowResult(result, doc.Application.VersionNumber);
                }
                else
                {
                    RevitMessage.ShowError(ErrorText, error!, null!);
                }
            }
            catch (Exception ex)
            {
                hasErrors = true;
                RevitMessage.ShowUnexpectedError(ex);
            }
            finally
            {
                stopwatch.Stop();
                if (!hasErrors && vm.ShowTimeReport)
                {
                    RevitMessage.ShowTimeResult(stopwatch.Elapsed);
                }
            }
        }
        
        private void TransactionStart(IReadOnlyCollection<ElementFromModel> fromModels, NumberingResult result, Document doc)
        {
            if (result == null) return;

            foreach (var item in fromModels)
            {
                var element = doc.GetElement(item.Id.GetElementId());
                if (!element.IsValid())
                {
                    result.Skipped++;
                    result.Details.Add($"{item.Name} (ID: {item.Id}) — элемент не найден.");
                    continue;
                }

                if (!string.Equals(item.UniqueId, element.UniqueId, StringComparison.InvariantCulture))
                {
                    result.Skipped++;
                    result.Details.Add($"{item.Name} (ID: {item.Id}) — элемент был заменён.");
                    continue;
                }

                var settings = item.Settings;
                if (settings == null)
                {
                    result.Skipped++;
                    result.Details.Add($"{item.Name} — без настроек.");
                    continue;
                }
                var parameterName = settings.ParameterName;
                if (string.IsNullOrWhiteSpace(parameterName))
                {
                    result.Skipped++;
                    result.Details.Add($"{item.Name} — параметр в настройках не назначен.");
                    continue;
                }
                var parameter = element.LookupParameter(parameterName);
                if (parameter == null)
                {
                    result.Skipped++;
                    result.Details.Add($"{item.Name} — параметр не найден.");
                    continue;
                }

                if (parameter.IsReadOnly)
                {
                    result.Skipped++;
                    result.Details.Add($"{item.Name} — параметр доступен только для чтения.");
                    continue;
                }

                if (parameter.StorageType != StorageType.String)
                {
                    result.Skipped++;
                    result.Details.Add($"{item.Name} — неподдерживаемый тип параметра.");
                    continue;
                }

                if (parameter.Set(item.NewValue))
                {
                    item.CurrentValue = item.NewValue;
                    result.Success++;
                }
                else
                {
                    result.Skipped++;
                    result.Details.Add($"{item.Name} (ID: {item.Id}) — не удалось изменить значение параметра.");
                }
            }
        }
    }
}
