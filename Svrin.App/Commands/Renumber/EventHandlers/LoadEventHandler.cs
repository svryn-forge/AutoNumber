using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Svrin.App.Revit;
using Svrin.App.Revit.Extensions;
using Svrin.App.Revit.Helpers;
using Svrin.Core.Abstractions;
using Svrin.Core.Providers;
using Svrin.Wpf.ViewModels;
using System.Diagnostics;

namespace Svrin.App.Commands.Renumber.EventHandlers
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class LoadEventHandler : RvtEventHandler<NumberingVM>
    {
        public override void Execute(UIApplication app, NumberingVM? vm)
        {
            if (vm == null)
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

                vm.Elements.Clear();

                var settings = JsonSetting<ElementNumberingSettings>.Open();
                vm.Settings = settings;
                vm.CurrentDocument = doc.Title;

                ElementNumberingSettings.LoadDefault(settings);

                var selectedIds = uidoc.Selection.GetElementIds();
                if (selectedIds.Count != 0)
                {
                    var selectedElements = selectedIds.Select(x => doc.GetElement(x)).ToList();
                    foreach (var e in selectedElements)
                    {
                        if (!e.IsValid())
                            continue;

                        if (e.Category is Category c)
                        {
                            var cId = c.Id.GetValue();

                            var s = settings.CategorySet.FirstOrDefault(x => x.Id != cId);
                            if (s == null)
                                continue;

                            CreateItem(doc, e, s, c.Id.GetValue(), vm);
                        }
                    }
                }
                else
                {
                    foreach (var s in settings.CategorySet)
                    {
                        var bic = (BuiltInCategory)s.Id;
                        if (!Enum.IsDefined(typeof(BuiltInCategory), bic))
                            continue;

                        foreach (var e in new FilteredElementCollector(doc)
                            .OfCategory(bic)
                            .WhereElementIsNotElementType())
                        {
                            if (!e.IsValid())
                                continue;

                            if (e.Category is Category c)
                            {
                                CreateItem(doc, e, s, c.Id.GetValue(), vm);
                            }
                        }
                    }
                }

                if (vm.Elements.Count == 0)
                {
                    return;
                }

                var byCategory = vm.Elements.GroupBy(e => e.CategoryId);
                foreach (var group in byCategory)
                {
                    var setting = settings.CategorySet.FirstOrDefault(x => x.Id == group.Key);
                    if (setting == null)
                        continue;

                    var sorted = group
                        .OrderBy(e => e.LevelElevation)      // уровни: снизу вверх
                        .ThenByDescending(e => e.Y)          // Y: "сверху вниз" на плане
                        .ThenBy(e => e.X)                    // X: "слева направо" на плане
                        .ThenBy(e => e.Id)                   // по Id элемента
                        .ToList();

                    if (sorted.Count == 0)
                        continue;

                    int number = 1;
                    double? currentLevel = null;

                    foreach (var t in sorted)
                    {
                        if (currentLevel != t.LevelElevation)
                        {
                            number = 1;
                            currentLevel = t.LevelElevation;
                        }

                        t.Number = number++;
                        t.Settings = setting;
                    }
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

        private static void CreateItem(Document doc, Element e, SettingCategory s, long categoryId, NumberingVM vm)
        {
            XYZ locationPoint;
            if (e.Location is LocationPoint lp)
                locationPoint = lp.Point;
            else if (e.Location is LocationCurve lc)
                locationPoint = lc.Curve.Evaluate(.5, false);
            else
                return;

            if (locationPoint == null)
                return;

            var level = doc.GetElement(e.LevelId);
            string levelName;
            double levelElevation;
            if (level.IsValid() && level.get_Parameter(BuiltInParameter.LEVEL_ELEV) is Parameter lvlElev && lvlElev.HasValue)
            {
                levelName = level.Name;
                levelElevation = lvlElev.AsDouble();
            }
            else
            { 
                return;
            }

            var parameterValue = string.Empty;
            var settingParameter = s.ParameterName;
            if (!string.IsNullOrWhiteSpace(settingParameter))
            {
                parameterValue = e.GetParameterValue(settingParameter);
            }

            var em = new ElementFromModel()
            {
                IsChecked = false,

                CategoryId = categoryId,

                CurrentValue = parameterValue,
                Level = levelName,
                LevelElevation = levelElevation,

                Id = e.Id.GetValue(),
                Name = e.Name,
                UniqueId = e.UniqueId,

                X = locationPoint.X,
                Y = locationPoint.Y,
                Z = locationPoint.Z
            };

            vm.Elements.Add(em);
        }

        public new string GetName() => nameof(LoadEventHandler);
    }
}
