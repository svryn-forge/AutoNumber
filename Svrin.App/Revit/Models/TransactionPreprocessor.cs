using Autodesk.Revit.DB;

namespace Svrin.App.Revit.Models
{
    internal class TransactionPreprocessor : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            failuresAccessor.DeleteAllWarnings();

            FailureProcessingResult result;
            if (failuresAccessor.GetFailureMessages().Count > 0)
            {
                result = FailureProcessingResult.ProceedWithRollBack;
            }
            else
            {
                result = FailureProcessingResult.Continue;
            }

            return result;
        }
    }
}
