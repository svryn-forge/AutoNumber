using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Svrin.App.Revit.Extensions;

namespace Svrin.App.Revit.Models
{
    internal class CategorySelectionFilter : ISelectionFilter
    {
        public long[] AllowedCategoryIds { get; } = [];

        public bool AllowElement(Element elem)
        {
            return elem.IsValid() && elem.Category is Category c && AllowedCategoryIds.Contains(c.Id.GetValue());
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
