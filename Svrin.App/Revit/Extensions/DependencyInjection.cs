using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using Svrin.App.Commands.Renumber.EventHandlers;
using Svrin.App.Revit.Helpers;
using Svrin.Core.Abstractions;
using Svrin.Wpf.Abstractions;
using Svrin.Wpf.Services;
using Svrin.Wpf.View;
using System.Windows;

namespace Svrin.App.Revit.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRevit(
            this IServiceCollection services,
            UIControlledApplication application)
        {
            services.AddSingleton(application);
            services.AddSingleton<INumberingHandlers, NumberingHandlers>();

            services.AddTransient<MainWindow>();
            services.AddSingletonWindow<MainWindow>();

            return services;
        }

        private static IServiceCollection AddSingletonWindow<T>(
            this IServiceCollection services)
            where T : Window
        {
            services.AddSingleton(sp =>
                new SingletonWindow<T>(
                    () => sp.GetRequiredService<T>(),
                    RevitWindowHandler.GetApplicationWindow));

            services.AddSingleton<ISingletonWindow>(
                sp => sp.GetRequiredService<SingletonWindow<T>>());

            return services;
        }
    }
}
