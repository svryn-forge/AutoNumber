namespace Svrin.Core.Abstractions
{
    public sealed class SearchFieldOption(SearchField field, string title)
    {
        public SearchField Field { get; } = field;
        public string Title { get; } = title;
    }
}
