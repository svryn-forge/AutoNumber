using Autodesk.Revit.UI;

namespace Svrin.App.Revit.Helpers
{
    internal static class RevitMessage
    {
        public static void Show(string title, string content, string instruction, string? expandedContent = null)
        {
            new TaskDialog(title)
            {
                MainContent = content,
                TitleAutoPrefix = false,
                MainInstruction = instruction,
                ExpandedContent = expandedContent
            }.Show();
        }

        public static void ShowError(string content, string instruction, string expandedContent)
        {
            new TaskDialog(Core.Properties.Resources.Error)
            {
                MainContent = content,
                TitleAutoPrefix = false,
                MainInstruction = instruction,
                ExpandedContent = expandedContent
            }.Show();
        }
        
    }
}
