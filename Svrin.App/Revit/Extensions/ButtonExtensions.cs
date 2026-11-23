using Autodesk.Revit.UI;
using Svrin.Wpf.Helpers;

namespace Svrin.App.Revit.Extensions
{
    internal static class ButtonExtensions
    {
        internal static RibbonItem AddButton<TExternalCommand>(
            this RibbonPanel panel,
            string btnName,
            string btnTxt,
            string? tooltip = default,
            string? longDescription = default,
            byte[]? largeImg = default,
            byte[]? smallImg = default,
            byte[]? toolTipImage = default,
            (ContextualHelpType, string) contextualHelp = default)
            where TExternalCommand : IExternalCommand
        {
            return AddButton<PushButtonData, TExternalCommand>(panel, btnName, btnTxt, tooltip, longDescription, largeImg, smallImg, toolTipImage, contextualHelp);
        }

        internal static RibbonItem AddButton<TPushButtonData, TExternalCommand>(
            this RibbonPanel panel,
            string btnName,
            string btnTxt,
            string? tooltip = default,
            string? longDescription = default,
            byte[]? largeImg = default,
            byte[]? smallImg = default,
            byte[]? toolTipImage = default,
            (ContextualHelpType, string) contextualHelp = default)
            where TExternalCommand : IExternalCommand
            where TPushButtonData : PushButtonData
        {
            return panel.AddItem(CreateButton<TPushButtonData, TExternalCommand>(btnName, btnTxt, tooltip, longDescription, largeImg, smallImg, toolTipImage, contextualHelp));
        }

        internal static PushButtonData CreateButton<TPushButtonData, TExternalCommand>(
            string btnName,
            string btnTxt,
            string? tooltip = default,
            string? longDescription = default,
            byte[]? largeImg = default,
            byte[]? smallImg = default,
            byte[]? toolTipImage = default,
            (ContextualHelpType, string) contextualHelp = default)
            where TExternalCommand : IExternalCommand
            where TPushButtonData : PushButtonData
        {
            Type command = typeof(TExternalCommand);
            TPushButtonData buttonData = (TPushButtonData)Activator
                .CreateInstance(typeof(TPushButtonData), btnName, btnTxt, command.Assembly.Location, command.FullName)!;

            if (!string.IsNullOrWhiteSpace(tooltip))
                buttonData.ToolTip = tooltip;

            if (ImageHelper.TryGetBitmapSource(largeImg, 32, out var largeSource))
                buttonData.LargeImage = largeSource;

            smallImg ??= largeImg;
            if (ImageHelper.TryGetBitmapSource(smallImg, 16, out var smallSource))
                buttonData.Image = smallSource;

            if (ImageHelper.TryGetBitmapSource(toolTipImage, out var tooltipSource))
                buttonData.ToolTipImage = tooltipSource;

            if (contextualHelp.Item1 != ContextualHelpType.None && !string.IsNullOrEmpty(contextualHelp.Item2))
                buttonData.SetContextualHelp(
                    new ContextualHelp(contextualHelp.Item1, contextualHelp.Item2));

            if (!string.IsNullOrWhiteSpace(longDescription))
                buttonData.LongDescription = longDescription;

            if (typeof(IExternalCommandAvailability).IsAssignableFrom(command))
                buttonData.AvailabilityClassName = command.FullName;

            return buttonData;
        }
    }
}
