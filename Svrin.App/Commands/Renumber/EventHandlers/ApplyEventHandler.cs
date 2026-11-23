using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Svrin.App.Revit;
using Svrin.App.Revit.Extensions;
using Svrin.App.Revit.Helpers;
using Svrin.App.Revit.Models;
using Svrin.Core.Abstractions;

namespace Svrin.App.Commands.Renumber.EventHandlers
{
    internal class ApplyEventHandler : RvtEventHandler<string, IReadOnlyCollection<ElementFromModel>>
    {
        public override void Execute(UIApplication app, string? documentTitle, IReadOnlyCollection<ElementFromModel>? collection)
        {
            if (string.IsNullOrWhiteSpace(documentTitle))
                return;
            if (collection == null || collection.Count == 0)
                return;
            if (!app.IsValid())
                return;

            var uidoc = app.ActiveUIDocument;
            var doc = uidoc.Document;
            var title = doc.Title;

            if (!string.Equals(title, documentTitle, StringComparison.InvariantCulture))
            {
                return;
            }

            TransactionsUtil.StartsTransactionGroup(doc, "StartsTransactionGroup", TransactionGroupAction, collection, doc, out var error);

            if (!string.IsNullOrWhiteSpace(error))
            {
            }
        }

        private static void TransactionGroupAction(IReadOnlyCollection<ElementFromModel> fromModels, Document doc)
        {
            IFailuresPreprocessor preprocessor = doc.IsWorkshared
                ? new WorksharingFailurePreprocessor()
                : new TransactionPreprocessor();

            foreach (var item in fromModels)
            {
                var element = doc.GetElement(item.Id.GetElementId());
                if (!element.IsValid())
                {
                    continue;
                }

                if (!string.Equals(item.UniqueId, element.UniqueId, StringComparison.InvariantCulture))
                {
                    continue;
                }

                var parameter = element.LookupParameter(item.Parameter);
                if (parameter == null || parameter.IsReadOnly || parameter.StorageType != StorageType.String)
                {
                    continue;
                }

                var status = TransactionsUtil.StartsTransaction(
                    doc,
                    "StartsTransaction",
                    () => parameter.Set(item.NewValue),
                    out var error,
                    preprocessor);

                if (!string.IsNullOrWhiteSpace(error))
                {
                    continue;
                }

                if (status == TransactionStatus.Committed)
                {

                }
                else
                {

                }
            }
        }
    }
}
