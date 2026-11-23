namespace Svrin.Wpf.Abstractions
{
    public interface IWindowService
    {
        void ShowDialog();
        void Close();
        void Close(object o);
        void RunOnUI(Action action);
    }
}
