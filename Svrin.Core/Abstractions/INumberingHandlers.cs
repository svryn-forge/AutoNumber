namespace Svrin.Core.Abstractions
{
    public interface INumberingHandlers
    {
        void Load(object obj);
        void Apply(string? title, IReadOnlyCollection<ElementFromModel>? elements);
        void ChangeSettings(object obj);
    }
}
