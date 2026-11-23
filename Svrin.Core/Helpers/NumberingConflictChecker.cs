using Svrin.Core.Abstractions;

namespace Svrin.Core.Helpers
{
    public static class NumberingConflictChecker
    {
        public static List<NumberingConflict> FindDuplicateNewValues(IEnumerable<ElementFromModel> elements)
        {
            var list = elements.ToList();

            var conflicts = new List<NumberingConflict>();

            foreach (var group in list
                .Where(e => !string.IsNullOrWhiteSpace(e.NewValue) && !e.HasErrors)
                .GroupBy(e => e.NewValue, StringComparer.InvariantCultureIgnoreCase)
                .Where(g => g.Count() > 1))
            {
                var items = group.ToList();
                foreach (var source in items)
                {
                    foreach (var target in items)
                    {
                        if (ReferenceEquals(source, target)) continue;

                        conflicts.Add(new NumberingConflict
                        {
                            Source = source,
                            NewValue = source.NewValue,
                            ConflictingElementId = target.Id,
                            ConflictingElementName = target.Name,
                            CurrentValue = target.CurrentValue
                        });
                    }
                }
            }

            return conflicts;
        }
    }
}
