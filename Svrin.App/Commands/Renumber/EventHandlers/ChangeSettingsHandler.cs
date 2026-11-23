using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Svrin.App.Revit;
using Svrin.App.Revit.Extensions;
using Svrin.Core.Abstractions;
using Svrin.Core.Providers;
using Svrin.Wpf.View;
using Svrin.Wpf.ViewModels;

namespace Svrin.App.Commands.Renumber.EventHandlers
{
    internal class ChangeSettingsHandler : RvtEventHandler<NumberingVM>
    {
        public override void Execute(UIApplication app, NumberingVM? vm)
        {
            if (vm == null)
                return;
            if (!app.IsValid())
                return;

            var uidoc = app.ActiveUIDocument;
            var doc = uidoc.Document;

            var categories = doc.Settings.Categories.OfType<Category>().Where(c => c.AllowsBoundParameters).Select(x => new SettingCategory()
            { 
                Id = x.Id.GetValue(),
                Name = x.Name
            });

            var settings = JsonSetting<ElementNumberingSettings>.Open();
            HashSet<long> existingIds = [];
            foreach (var s in settings.CategorySet)
            {
                existingIds.Add(s.Id);
                s.IsSelected = true;
            }

            foreach (var cat in categories)
            {
                if (!existingIds.Contains(cat.Id))
                {
                    settings.CategorySet.Add(cat);
                }
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
    }
}
