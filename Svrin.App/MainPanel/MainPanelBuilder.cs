using Autodesk.Revit.UI;
using Svrin.App.Commands.Renumber;
using Svrin.App.Revit.Extensions;
using Svrin.App.Revit.Helpers;
using Svrin.Core.Properties;

namespace Svrin.App.MainPanel
{
    internal static class MainPanelBuilder
    {
        public static void CreateCommonPanel(UIControlledApplication application)
        {
            if (application.TryCreateRibbonPanel(
                Resources.CommonTabName,
                Resources.CommonPanelName,
                out var panel))
            {
                panel!.AddButton<RenumberCommand>(
                    Resources.NumberingButtonName,
                    Resources.NumberingButtonText,
                    Resources.NumberingButtonTooltip,
                    Resources.NumberingButtonDiscription,
                    Resources.defaultIcon_32x32);
            }
            else
            {
                RevitMessage.Show(
                    Resources.Error,
                    Resources.PanelAlreadyExistContext,
                    Resources.PanelAlreadyExistInstruction);
            }
        }
    }
}
