using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Svrin.App.Revit.Extensions
{
    internal static class ValidObjectExtensions
    {
        public static bool IsValid(this UIApplication app) => app != null && app.IsValidObject;
        public static bool IsValid(this UIDocument doc) => doc != null && doc.IsValidObject;
        public static bool IsValid(this Document doc) => doc != null && doc.IsValidObject;
        public static bool IsValid(this Element e) => e != null && e.IsValidObject;
    }
}
