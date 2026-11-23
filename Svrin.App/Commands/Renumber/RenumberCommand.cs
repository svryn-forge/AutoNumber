using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using Svrin.App.Revit.Extensions;
using Svrin.App.Revit.Helpers;
using Svrin.Wpf.Services;
using Svrin.Wpf.View;

namespace Svrin.App.Commands.Renumber
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class RenumberCommand : IExternalCommand, IExternalCommandAvailability
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                ExternalApplication.Services
                    .GetRequiredService<SingletonWindow<MainWindow>>()
                    .Show();
            }
            catch (Exception ex)
            {
                RevitMessage.ShowUnexceptedError(ex);
                return Result.Failed;
            }

            return Result.Succeeded;
        }
       
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet _)
        {
            if (!applicationData.IsValid())
                return false;

            var uiDoc = applicationData.ActiveUIDocument;
            if (!uiDoc.IsValid())
                return false;

            var doc = uiDoc.Document;
            return doc.IsValid() && !doc.IsFamilyDocument;
        }
    }
}
