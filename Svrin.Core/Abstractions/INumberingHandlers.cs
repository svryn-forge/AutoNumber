namespace Svrin.Core.Abstractions
{
    public interface INumberingHandlers
    {
        void Load(object obj);
        void Apply(object obj);
        void ChangeSettings(object obj);
    }
}
