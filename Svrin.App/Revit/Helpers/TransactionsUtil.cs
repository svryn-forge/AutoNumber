using Autodesk.Revit.DB;

namespace Svrin.App.Revit.Helpers
{
    internal static class TransactionsUtil
    {
        public static TransactionStatus StartsTransactionGroup(Document doc, string name, Action action, out string? error)
        {
            TransactionStatus status = TransactionStatus.Uninitialized;
            error = null;
            using (var t = new TransactionGroup(doc, name))
            {
                try
                {
                    t.Start();

                    action();

                    t.Assimilate();
                }
                catch (Exception ex)
                {
                    if (t.HasStarted())
                        t.RollBack();

                    error = ex.Message;
                }

                status = t.GetStatus();
            }

            return status;
        }
        public static TransactionStatus StartsTransactionGroup<T>(Document doc, string name, Action<T> action, T param, out string? error)
        {
            TransactionStatus status = TransactionStatus.Uninitialized;
            error = null;
            using (var t = new TransactionGroup(doc, name))
            {
                try
                {
                    t.Start();

                    action(param);

                    t.Assimilate();
                }
                catch (Exception ex)
                {
                    if (t.HasStarted())
                        t.RollBack();

                    error = ex.Message;
                }

                status = t.GetStatus();
            }

            return status;
        }
        public static TransactionStatus StartsTransactionGroup<T1, T2>(Document doc, string name, Action<T1, T2> action, T1 param1, T2 param2, out string? error)
        {
            TransactionStatus status = TransactionStatus.Uninitialized;
            error = null;
            using (var t = new TransactionGroup(doc, name))
            {
                try
                {
                    t.Start();

                    action(param1, param2);

                    t.Assimilate();
                }
                catch (Exception ex)
                {
                    if (t.HasStarted())
                        t.RollBack();

                    error = ex.Message;
                }

                status = t.GetStatus();
            }

            return status;
        }

        public static TransactionStatus StartsTransaction(Document doc, string name, Action action,
            out string? error,
            IFailuresPreprocessor? failuresPreprocessor = null)
        {
            TransactionStatus status = TransactionStatus.Uninitialized;
            error = null;
            using (var t = new Transaction(doc, name))
            {
                try
                {
                    AddFailuresPreprocessor(t, failuresPreprocessor);
                    t.Start();
                    action();
                    t.Commit();
                }
                catch (Exception ex)
                {
                    if (t.HasStarted())
                        t.RollBack();

                    error = ex.Message;
                }

                status = t.GetStatus();
            }
            return status;
        }
        public static TransactionStatus StartsTransaction<T>(Document doc, string name, Action<T> action, T param,
            out string? error,
            IFailuresPreprocessor? failuresPreprocessor = null)
        {
            TransactionStatus status = TransactionStatus.Uninitialized;
            error = null;
            using (var t = new Transaction(doc, name))
            {
                try
                {
                    AddFailuresPreprocessor(t, failuresPreprocessor);
                    t.Start();
                    action(param);
                    t.Commit();
                }
                catch (Exception ex)
                {
                    if (t.HasStarted())
                        t.RollBack();

                    error = ex.Message;
                }

                status = t.GetStatus();
            }
            return status;
        }
        public static TransactionStatus StartsTransaction<T1, T2, T3>(Document doc, string name, Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3,
            out string? error,
            IFailuresPreprocessor? failuresPreprocessor = null)
        {
            TransactionStatus status = TransactionStatus.Uninitialized;
            error = null;
            using (var t = new Transaction(doc, name))
            {
                try
                {
                    AddFailuresPreprocessor(t, failuresPreprocessor);
                    t.Start();
                    action(param1, param2, param3);
                    t.Commit();
                }
                catch (Exception ex)
                {
                    if (t.HasStarted())
                        t.RollBack();

                    error = ex.Message;
                }

                status = t.GetStatus();
            }
            return status;
        }

        private static void AddFailuresPreprocessor(Transaction? t, IFailuresPreprocessor? failuresPreprocessor)
        {
            if (failuresPreprocessor == null)
                return;
            if (t == null || !t.IsValidObject || t.HasStarted())
                return;

            t.SetFailureHandlingOptions(
                t.GetFailureHandlingOptions()
                    .SetFailuresPreprocessor(failuresPreprocessor));
        }
    }
}
