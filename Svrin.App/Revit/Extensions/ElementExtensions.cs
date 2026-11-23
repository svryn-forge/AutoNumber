using Autodesk.Revit.DB;

namespace Svrin.App.Revit.Extensions
{
    internal static class ElementExtensions
    {
        public static string GetParameterValue(this Element e, string parameterName)
        {
            Parameter parameter = e.LookupParameter(parameterName);
            if (parameter != null && parameter.HasValue)
            {
                if (parameter.StorageType == StorageType.String)    // актуально для версий до 2024
                {
                    return parameter.AsString();
                }
                else
                {
                    return parameter.AsValueString();
                }
            }
            return string.Empty;
        }
    }
}
