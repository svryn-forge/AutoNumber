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
        public static TransactionStatus StartsTransactionGroup<T1, T2, T3>(Document doc, string name, Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3,
            out string? error)
        {
            TransactionStatus status = TransactionStatus.Uninitialized;
            error = null;
            using (var t = new TransactionGroup(doc, name))
            {
                try
                {
                    t.Start();

                    action(param1, param2, param3);

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
        public static TransactionStatus StartsTransaction<T1, T2>(Document doc, string name, Action<T1, T2> action, T1 param1, T2 param2,
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
                    action(param1, param2);
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
        public static TransactionStatus StartsTransaction<T1, T2, T3, T4>(Document doc, string name, Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4,
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
                    action(param1, param2, param3, param4);
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

        public static TResult? StartsTransaction<TResult>(Document doc, string name, Func<TResult> func, out TransactionStatus status,
            out string? error,
            IFailuresPreprocessor? failuresPreprocessor = null)
        {
            TResult? result = default;
            error = null;
            status = TransactionStatus.Uninitialized;
            using (var t = new Transaction(doc, name))
            {
                try
                {
                    AddFailuresPreprocessor(t, failuresPreprocessor);
                    t.Start();
                    result = func();
                    t.Commit();
                    status = result == null ? TransactionStatus.Error : t.GetStatus();
                }
                catch (Exception ex)
                {
                    if (t.HasStarted())
                        t.RollBack();

                    error = ex.Message;
                    status = TransactionStatus.RolledBack;
                }
            }
            return result;
        }
        public static TResult? StartsTransaction<T, TResult>(Document doc, string name, Func<T, TResult> func, T param,
            out TransactionStatus status,
            out string? error,
            IFailuresPreprocessor? failuresPreprocessor = null)
        {
            TResult? result = default;
            error = null;
            status = TransactionStatus.Uninitialized;
            using (var t = new Transaction(doc, name))
            {
                try
                {
                    AddFailuresPreprocessor(t, failuresPreprocessor);
                    t.Start();
                    result = func(param);
                    t.Commit();
                    status = result == null ? TransactionStatus.Error : t.GetStatus();
                }
                catch (Exception ex)
                {
                    if (t.HasStarted())
                        t.RollBack();

                    error = ex.Message;
                    status = TransactionStatus.RolledBack;
                }
            }
            return result;
        }
        public static TResult? StartsTransaction<T1, T2, TResult>(Document doc, string name, Func<T1, T2, TResult> func, T1 param1, T2 param2,
            out TransactionStatus status,
            out string? error,
            IFailuresPreprocessor? failuresPreprocessor = null)
        {
            TResult? result = default;
            error = null;
            status = TransactionStatus.Uninitialized;
            using (var t = new Transaction(doc, name))
            {
                try
                {
                    AddFailuresPreprocessor(t, failuresPreprocessor);
                    t.Start();
                    result = func(param1, param2);
                    t.Commit();
                    status = result == null ? TransactionStatus.Error : t.GetStatus();
                }
                catch (Exception ex)
                {
                    if (t.HasStarted())
                        t.RollBack();

                    status = TransactionStatus.RolledBack;
                    error = ex.Message;
                }
            }
            return result;
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
