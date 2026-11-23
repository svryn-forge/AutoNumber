using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using Svrin.App.MainPanel;
using Svrin.App.Revit.Extensions;
using Svrin.App.Revit.Helpers;
using Svrin.Core.Properties;
using Svrin.Wpf.Abstractions;
using System.Windows;

namespace Svrin.App
{
    public class ExternalApplication : IExternalApplication
    {
        internal static IServiceProvider Services { get; private set; } = null!;

        private static readonly HashSet<int> _supportedVersions =
        [
            2022, 2024, 2025
        ];

        public Result OnShutdown(UIControlledApplication application)
        {
            foreach (var window in Services.GetServices<ISingletonWindow>())
            {

                try
                {
                    window?.Close();
                }
                catch
                {

                }
            }

            (Services as IDisposable)?.Dispose();
            return Result.Succeeded;
        }
       
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                if (Application.Current is null)
                    _ = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };

                IServiceCollection services = new ServiceCollection();

                services.AddSingleton(application);
                
                services.AddRevit(application);

                Services = services.BuildServiceProvider();

                CreateRibbon(application, _supportedVersions);
            }
            catch (Exception ex)
            {
                RevitMessage.ShowError(
                    ex.Message,
                    Resources.UnexpectedException,
                    ex.ToString());

                return Result.Failed;
            }

            return Result.Succeeded;
        }

        public static void CreateRibbon(UIControlledApplication application, HashSet<int> supportedVersions)
        {
            var app = application.ControlledApplication;

            if (int.TryParse(app.VersionNumber, out int rvtVersion) && supportedVersions.Contains(rvtVersion))
            {
                MainPanelBuilder.CreateCommonPanel(application);
            }
            else
            {
                RevitMessage.Show(
                    Resources.Error,
                    Resources.UnsupportedVersionContext,
                    Resources.UnsupportedVersionInstruction);
            }
        }
    }
}
