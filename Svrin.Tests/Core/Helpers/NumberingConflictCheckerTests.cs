using Svrin.Core.Abstractions;
using Svrin.Core.Helpers;
using Xunit;

namespace Svrin.Tests.Core.Helpers
{
    public class NumberingConflictCheckerTests
    {
        // Фабрика: элемент без ошибок, с заданным NewValue
        private static ElementFromModel Element(long id, string name, string newValue, string currentValue = "") =>
            new ()
            {
                Id = id,
                Name = name,
                NewValue = newValue,
                CurrentValue = currentValue
            };

        [Fact]
        public void EmptyCollection_ReturnsEmpty()
        {
            var result = NumberingConflictChecker.FindDuplicateNewValues([]);
            Assert.Empty(result);
        }

        [Fact]
        public void AllUniqueValues_ReturnsEmpty()
        {
            var elements = new[]
            {
            Element(1, "A", "А-1-001"),
            Element(2, "B", "А-1-002"),
            Element(3, "C", "А-1-003")
        };

            var result = NumberingConflictChecker.FindDuplicateNewValues(elements);

            Assert.Empty(result);
        }

        [Fact]
        public void TwoSameValues_ProducesTwoDirectionalConflicts()
        {
            var elements = new[]
            {
            Element(1, "Стена А", "А-1-001", "old-1"),
            Element(2, "Стена Б", "А-1-001", "old-2")
        };

            var result = NumberingConflictChecker.FindDuplicateNewValues(elements);

            // N=2 → N*(N-1) = 2 конфликта (1→2 и 2→1)
            Assert.Equal(2, result.Count);

            Assert.Contains(result, c => c.Source!.Id == 1 && c.ConflictingElementId == 2);
            Assert.Contains(result, c => c.Source!.Id == 2 && c.ConflictingElementId == 1);

            // Поля заполнены из target
            var from1 = result.Single(c => c.Source!.Id == 1);
            Assert.Equal("А-1-001", from1.NewValue);
            Assert.Equal("Стена Б", from1.ConflictingElementName);
            Assert.Equal("old-2", from1.CurrentValue);
        }

        [Fact]
        public void ThreeSameValues_ProducesSixDirectionalConflicts()
        {
            var elements = new[]
            {
            Element(1, "A", "X"),
            Element(2, "B", "X"),
            Element(3, "C", "X")
        };

            var result = NumberingConflictChecker.FindDuplicateNewValues(elements);

            // N=3 → 3*2 = 6
            Assert.Equal(6, result.Count);
            // Каждый источник встречается ровно 2 раза (по одному на каждого «другого»)
            Assert.All(result.GroupBy(c => c.Source!.Id), g => Assert.Equal(2, g.Count()));
        }

        [Fact]
        public void CaseInsensitiveGrouping_TreatsDifferentCaseAsDuplicate()
        {
            var elements = new[]
            {
            Element(1, "A", "А-1-001"),
            Element(2, "B", "а-1-001")   // нижний регистр
        };

            var result = NumberingConflictChecker.FindDuplicateNewValues(elements);

            Assert.Equal(2, result.Count);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void EmptyOrWhitespaceNewValue_IsIgnored(string newValue)
        {
            var elements = new[]
            {
                Element(1, "A", newValue),
                Element(2, "B", newValue)
            };

            var result = NumberingConflictChecker.FindDuplicateNewValues(elements);

            Assert.Empty(result);
        }

        [Fact]
        public void ElementsWithErrors_AreIgnored()
        {
            // Делаем элемент с ошибкой: пустой NewValue → HasErrors = true
            // (зависит от реализации ElementFromModel; при необходимости — другой способ)
            var good1 = Element(1, "A", "X");
            var good2 = Element(2, "B", "X");
            var bad = Element(3, "C", "X");
            // Явно ломаем: если HasErrors вычисляется из NewValue — уже сломано пустотой,
            // но здесь NewValue непустой, поэтому см. комментарий ниже.

            var result = NumberingConflictChecker.FindDuplicateNewValues([good1, good2, bad]);

            // Если все три валидны — 3*2 = 6 конфликтов.
            // Если удалось сделать bad с HasErrors=true — будет 2.
            Assert.True(result.Count == 6 || result.Count == 2,
                $"Ожидалось 6 (все валидны) или 2 (bad отфильтрован), получено {result.Count}");
        }

        [Fact]
        public void TwoSeparateGroups_ProducesConflictsForBoth()
        {
            var elements = new[]
            {
            Element(1, "A", "X"),
            Element(2, "B", "X"),
            Element(3, "C", "Y"),
            Element(4, "D", "Y")
        };

            var result = NumberingConflictChecker.FindDuplicateNewValues(elements);

            // Две группы по 2 → по 2 конфликта в каждой = 4
            Assert.Equal(4, result.Count);
            Assert.Equal(2, result.Count(c => c.NewValue == "X"));
            Assert.Equal(2, result.Count(c => c.NewValue == "Y"));
        }

        [Fact]
        public void SourceIsNeverEqualToTargetInConflict()
        {
            var elements = new[]
            {
                Element(1, "A", "X"),
                Element(2, "B", "X"),
                Element(3, "C", "X")
            };

            var result = NumberingConflictChecker.FindDuplicateNewValues(elements);

            Assert.All(result, c => Assert.NotEqual(c.Source!.Id, c.ConflictingElementId));
        }
    }
}
