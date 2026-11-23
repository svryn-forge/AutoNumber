using Svrin.Core.Abstractions;
using Svrin.Wpf.Services;
using Svrin.Wpf.ViewModels;
using System.Windows;

namespace Svrin.Wpf.View
{
    public partial class MainWindow : Window
    {
        private readonly NumberingVM _vm;

        public MainWindow(INumberingHandlers numberingHandlers)
        {
            var ws = new WindowService(this);
            _vm = new NumberingVM(ws, numberingHandlers);
            DataContext = _vm;

            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;
            _vm?.LoadCommand.Execute(null);
        }
    }
}
