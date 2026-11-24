using Autodesk.Revit.UI;
using Svrin.Core.Properties;

namespace Svrin.App.Revit.Helpers
{
    internal static class RevitMessage
    {
        public static void Show(string title, string content, string instruction, string? expandedContent = null) =>
            ShowDialog(
                title,
                content,
                instruction,
                expandedContent);

        public static void ShowTimeResult(TimeSpan time) =>
            ShowDialog(
                Resources.Report,
                $"{Resources.TimeResult}: {time}",
                null,
                null);

        public static void ShowError(string content, string instruction, string expandedContent) =>
            ShowDialog(
                Resources.Error,
                content,
                instruction,
                expandedContent);

        public static void ShowUnexpectedError(Exception ex) =>
            ShowDialog(
                Resources.Error,
                ex.Message,
                Resources.UnexpectedException,
                ex.ToString());

        private static void ShowDialog(string title, string content, string? instruction, string? expandedContent = null)
        {
            new TaskDialog(title)
            {
                MainContent = content,
                TitleAutoPrefix = false,
                MainInstruction = instruction,
                ExpandedContent = expandedContent
            }.Show();
        }
    }
}
