using Svrin.Wpf.Abstractions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Svrin.Wpf.Services
{
    public sealed class SingletonWindow<T>(
        Func<T> windowFactory,
        Func<IntPtr>? getHWND = null)
        : ISingletonWindow
        where T : Window
    {
        private readonly object _threadLock = new();
        private Dispatcher? _dispatcher;

        private Dispatcher Dispatcher =>
            _dispatcher ??= Application.Current?.Dispatcher
                ?? System.Windows.Threading.Dispatcher.CurrentDispatcher;
        private readonly Func<T> _windowFactory = windowFactory
                ?? throw new ArgumentNullException(nameof(windowFactory));
        private readonly Func<IntPtr>? _getHWND = getHWND;

        private T? _window;
        private IntPtr _owner;

        public T? Window
        {
            get
            {
                RunOnUI(CreateWindowIfNeeded);
                return _window;
            }
        }

        public void Show()
        {
            RunOnUI(() =>
            {
                CreateWindowIfNeeded();

                if (_window is null)
                    return;

                if (!_window.IsVisible)
                    _window.Show();

                if (_window.WindowState == WindowState.Minimized)
                    _window.WindowState = WindowState.Normal;

                _window.Activate();
            });
        }

        public void Close()
        {
            RunOnUI(() => _window?.Close());
        }

        public void Focus()
        {
            RunOnUI(() =>
            {
                if (_window is null)
                    return;

                if (_window.WindowState == WindowState.Minimized)
                    _window.WindowState = WindowState.Normal;

                _window.Activate();
            });
        }

        private void CreateWindowIfNeeded()
        {
            if (_window is not null)
                return;

            lock (_threadLock)
            {
                if (_window is not null)
                    return;

                _window = _windowFactory();

                _window.Closed += ClosingEvent;

                if (_getHWND is not null)
                    _owner = _getHWND();

                if (_owner != IntPtr.Zero)
                    _ = new WindowInteropHelper(_window) { Owner = _owner };
            }
        }

        private void RunOnUI(Action action)
        {
            if (Dispatcher.CheckAccess())
                action();
            else
                Dispatcher.Invoke(action);
        }

        private void ClosingEvent(object? sender, EventArgs e)
        {
            if (_window is null)
                return;

            _window.Closed -= ClosingEvent;
            _window = null;
        }
    }
}
