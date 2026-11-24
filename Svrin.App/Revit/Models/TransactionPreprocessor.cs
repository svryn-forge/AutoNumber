using Autodesk.Revit.DB;
using Svrin.App.Revit.Extensions;

namespace Svrin.App.Revit.Models
{
    internal class TransactionPreprocessor : IFailuresPreprocessor
    {
        public List<string> Warnings { get; } = new List<string>();

        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            var failures = failuresAccessor.GetFailureMessages();

            foreach (var failure in failures)
            {
                if (failure.GetSeverity() != FailureSeverity.Warning)
                    continue;

                var description = failure.GetDescriptionText();

                var elementIds = failure
                    .GetFailingElementIds()
                    .Select(id => id.GetValue().ToString());

                Warnings.Add($"{description}. ID элементов: {string.Join(", ", elementIds)}");

                failuresAccessor.DeleteWarning(failure);
            }

            return FailureProcessingResult.Continue;
        }
    }
}
