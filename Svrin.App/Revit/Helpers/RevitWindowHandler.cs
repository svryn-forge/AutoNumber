using Autodesk.Windows;

namespace Svrin.App.Revit.Helpers
{
    internal static class RevitWindowHandler
    {
        public static IntPtr GetApplicationWindow() => ComponentManager.ApplicationWindow;
    }
}
