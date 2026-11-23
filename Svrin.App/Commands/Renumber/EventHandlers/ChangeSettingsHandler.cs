using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Svrin.App.Revit;
using Svrin.App.Revit.Extensions;
using Svrin.App.Revit.Helpers;
using Svrin.Core.Abstractions;
using Svrin.Core.Providers;
using Svrin.Wpf.View;
using Svrin.Wpf.ViewModels;
using System.Diagnostics;

namespace Svrin.App.Commands.Renumber.EventHandlers
{
    internal class ChangeSettingsHandler : RvtEventHandler<NumberingVM>
    {
        public override void Execute(UIApplication app, NumberingVM? vm)
        {
            if (vm == null)
                return;
            var vmSettings = vm.Settings;
            if (vmSettings == null)
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

                var categories = doc.Settings.Categories.OfType<Category>().Where(c => c.AllowsBoundParameters).Select(x => new SettingCategory()
                {
                    Id = x.Id.GetValue(),
                    Name = x.Name
                });

                var settings = JsonSetting<ElementNumberingSettings>.Open();
                HashSet<long> existingIds = [];

                Dictionary<long, SettingCategory> d = [];

                settings.CategorySet.Clear();

                foreach (var s in vmSettings.CategorySet)
                {
                    s.IsSelected = true;
                    d.Add(s.Id, s);
                }

                foreach (var cat in categories)
                {
                    if (!d.TryGetValue(cat.Id, out var s))
                    {
                        s = cat;
                    }

                    settings.CategorySet.Add(s);
                }

                var editorVm = new CategoryEditorVM(settings.CategorySet);

                var window = new CategoryEditorWindow()
                {
                    DataContext = editorVm
                };

                if (window.ShowDialog() == true && editorVm.Result != null)
                {
                    settings.CategorySet.Clear();
                    settings.CategorySet.AddRange(editorVm.Result.Where(x => x.IsSelected));

                    JsonSetting<ElementNumberingSettings>.Save(settings);

                    vm.Settings = settings;
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
    }
}
