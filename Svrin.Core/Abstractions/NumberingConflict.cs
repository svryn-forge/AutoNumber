namespace Svrin.Core.Abstractions
{
    public class NumberingConflict
    {
        public ElementFromModel? Source { get; set; }

        public string NewValue { get; set; } = string.Empty;

        public long ConflictingElementId { get; set; }

        public string ConflictingElementName { get; set; } = string.Empty;

        public string CurrentValue { get; set; } = string.Empty;
    }
}
