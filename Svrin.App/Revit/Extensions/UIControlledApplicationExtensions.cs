using Autodesk.Revit.UI;

namespace Svrin.App.Revit.Extensions
{
    internal static class UIControlledApplicationExtensions
    {
        internal static bool TryCreateRibbonTab(this UIControlledApplication application, string tab)
        {
            bool ret = false;
            try
            {
                application.CreateRibbonTab(tab);
                ret = true;
            }
            catch
            { }

            return ret;
        }

        internal static bool TryCreateRibbonPanel(this UIControlledApplication application, string tab, string panelName, out RibbonPanel? panel)
        {
            try
            {
                application.CreateRibbonTab(tab);
                panel = application.CreateRibbonPanel(tab, panelName);
            }
            catch
            {
                panel = null;
            }

            return panel != null;
        }
    }
}
