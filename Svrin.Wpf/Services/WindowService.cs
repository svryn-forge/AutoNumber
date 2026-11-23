using Svrin.Wpf.Abstractions;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace Svrin.Wpf.Services
{
    public class WindowService : IWindowService
    {
        public WindowService(Window window, Action<Window>? onClose = null)
        {
            _window = window;
            _dispatcher = window.Dispatcher;
            _window.Closed += OnWindowClosed;
            _onClose = onClose;
        }

        private readonly Action<Window>? _onClose;
        private readonly Window _window;
        private readonly Dispatcher _dispatcher;

        public object? DataContext
        {
            get => _window?.DataContext;
            set
            {
                _window?.DataContext = value;
            }
        }

        public void ShowDialog() => _window?.ShowDialog();

        public void Close(object o) => this.Close();
        public void Close() => _window?.Close();

        public static TView Initialize<TView, TVM>(
            Func<TView> viewCreationFunc,
            Func<WindowService, TVM> vmCreationFunc)
            where TView : Window
        {
            var view = viewCreationFunc();
            var service = new WindowService(view);

            view.DataContext = vmCreationFunc(service);

            return view;
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            try
            {
                _onClose?.Invoke(_window);

                if (DataContext is IDisposable disposable)
                    disposable.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                _window.Closed -= OnWindowClosed;
            }
        }

        public void RunOnUI(Action action)
        {
            if (_dispatcher == null || action == null)
                return;

            if (_dispatcher.CheckAccess())
                action();
            else
                _dispatcher.Invoke(action);
        }
    }
}
