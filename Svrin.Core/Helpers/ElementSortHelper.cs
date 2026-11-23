using Svrin.Core.Abstractions;

namespace Svrin.Core.Helpers
{
    public static class ElementSortHelper
    {
        public static IReadOnlyList<ElementFromModel> SortForNumbering(IEnumerable<ElementFromModel> elements, double rowTolerance = 3.28083989501312) // 1 м
        {
            var byLevel = elements
                .GroupBy(e => e.LevelElevation)
                .OrderBy(g => g.Key);

            var result = new List<ElementFromModel>();

            foreach (var levelGroup in byLevel)
            {
                var sortedByY = levelGroup
                    .OrderByDescending(e => e.Y)
                    .ThenBy(e => e.X)
                    .ThenBy(e => e.Id)
                    .ToList();

                var rows = new List<List<ElementFromModel>>();
                foreach (var el in sortedByY)
                {
                    if (rows.Count == 0)
                    {
                        rows.Add([el]);
                        continue;
                    }

                    var currentRow = rows.Last();
                    var rowY = currentRow[0].Y;

                    if (Math.Abs(rowY - el.Y) <= rowTolerance)
                        currentRow.Add(el);
                    else
                        rows.Add([el]);
                }

                foreach (var row in rows)
                    result.AddRange(row.OrderBy(e => e.X).ThenBy(e => e.Id));
            }

            return result;
        }
    }
}
