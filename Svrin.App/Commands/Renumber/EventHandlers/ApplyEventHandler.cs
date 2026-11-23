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
        private NumberingResult? _result;

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

                _result = new NumberingResult()
                {
                    Total = fromModels.Count
                };

                var preprocessor = new TransactionPreprocessor();
                var status = TransactionsUtil.StartsTransaction(
                    doc,
                    "Заполнение параметра",
                    TransactionStart,
                    fromModels,
                    doc,
                    out var error,
                    preprocessor);

                if (string.IsNullOrWhiteSpace(error))
                {
                    if (status != TransactionStatus.Committed)
                    {
                        _result.Details.Add($"Транзакция завершилась со статусом {status}.");
                    }
                    if (preprocessor.Warnings.Count > 0)
                    {
                        _result.Details.Add("Preprocessor Warnings");
                        _result.Details.AddRange(preprocessor.Warnings);
                        _result.Details.Add(Environment.NewLine);
                    }

                    NumberingResultLogger.ShowResult(_result, doc.Application.VersionNumber);
                }
                else
                {
                    RevitMessage.ShowError(ErrorText, error!, null!);
                }
            }
            catch (Exception ex)
            {
                hasErrors = true;
                RevitMessage.ShowUnexceptedError(ex);
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
        
        private void TransactionStart(IReadOnlyCollection<ElementFromModel> fromModels, Document doc)
        {
            if (_result == null) return;

            foreach (var item in fromModels)
            {
                var element = doc.GetElement(item.Id.GetElementId());
                if (!element.IsValid())
                {
                    _result.Skipped++;
                    _result.Details.Add($"{item.Name} (ID: {item.Id}) — элемент не найден.");
                    continue;
                }

                if (!string.Equals(item.UniqueId, element.UniqueId, StringComparison.InvariantCulture))
                {
                    _result.Skipped++;
                    _result.Details.Add($"{item.Name} (ID: {item.Id}) — элемент был заменён.");
                    continue;
                }

                var settings = item.Settings;
                if (settings == null)
                {
                    _result.Skipped++;
                    _result.Details.Add($"{item.Name} — без настроек.");
                    continue;
                }
                var parameterName = settings.ParameterName;
                if (string.IsNullOrWhiteSpace(parameterName))
                {
                    _result.Skipped++;
                    _result.Details.Add($"{item.Name} — параметр в настройках не назначен.");
                    continue;
                }
                var parameter = element.LookupParameter(parameterName);
                if (parameter == null)
                {
                    _result.Skipped++;
                    _result.Details.Add($"{item.Name} — параметр не найден.");
                    continue;
                }

                if (parameter.IsReadOnly)
                {
                    _result.Skipped++;
                    _result.Details.Add($"{item.Name} — параметр доступен только для чтения.");
                    continue;
                }

                if (parameter.StorageType != StorageType.String)
                {
                    _result.Skipped++;
                    _result.Details.Add($"{item.Name} — неподдерживаемый тип параметра.");
                    continue;
                }

                if (parameter.Set(item.NewValue))
                {
                    item.CurrentValue = item.NewValue;
                    _result.Success++;
                }
                else
                {
                    _result.Skipped++;
                    _result.Details.Add($"{item.Name} (ID: {item.Id}) — не удалось изменить значение параметра.");
                }
            }
        }
    }
}
