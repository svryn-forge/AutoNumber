namespace Svrin.Core.Helpers
{
    public static class NumberFormat
    {
        public static string? FromPadding(int? padding)
        {
            if (!padding.HasValue || padding.Value < 1)
                return null;

            padding = Math.Max(padding.Value, 1);
            return $"D{padding}";
        }
    }
}
