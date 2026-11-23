using Autodesk.Revit.DB;

namespace Svrin.App.Revit.Extensions
{
    internal static class ElementIdExtensions
    {
        public static long GetValue(this ElementId id)
        {
            if (id == null)
                return -1L;

#if R2022
            return id.IntegerValue;
#else
            return id.Value;
#endif
        }

        public static ElementId GetElementId(this long value)
        {
#if R2022
            return new ElementId((int)value);
#else
            return new ElementId(value);
#endif
        }

        public static ElementId GetElementId(this int value)
        {
#if R2022
            return new ElementId(value);
#else
            return new ElementId((long)value);
#endif
        }
    }
}
