using Autodesk.Revit.DB;

namespace Svrin.App.Revit.Models
{
    public class WorksharingFailurePreprocessor : IFailuresPreprocessor
    {
        public List<ElementId> LockedElements { get; } = [];

        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            var failures = failuresAccessor.GetFailureMessages();

            bool hasWorksharingFailure = false;

            foreach (FailureMessageAccessor failure in failures)
            {
                FailureDefinitionId id = failure.GetFailureDefinitionId();

                if (id == BuiltInFailures.EditingFailures.CannotEditElements ||
                    id == BuiltInFailures.EditingFailures.CannotEditEditingElements ||
                    id == BuiltInFailures.EditingFailures.OwnedByOther)
                {
                    hasWorksharingFailure = true;

                    foreach (ElementId elementId in failure.GetFailingElementIds())
                    {
                        if (!LockedElements.Contains(elementId))
                            LockedElements.Add(elementId);
                    }
                }
            }

            if (hasWorksharingFailure)
            {
                // убрать failure и откатить транзакцию без стандартного окна Revit
                FailureHandlingOptions options = failuresAccessor.GetFailureHandlingOptions();
                options.SetClearAfterRollback(true);

                failuresAccessor.SetFailureHandlingOptions(options);

                return FailureProcessingResult.ProceedWithRollBack;
            }

            return FailureProcessingResult.Continue;
        }
    }
}
