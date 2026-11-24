using Svrin.Core.Abstractions;
using Svrin.Core.Helpers;
using Xunit;

namespace Svrin.Tests.Core.Helpers
{
    public class NumberFormatTests
    {
        [Theory]
        [InlineData(0, null)]
        [InlineData(1, "D1")]
        [InlineData(3, "D3")]
        public void FromPadding_ReturnsExpectedFormat(int? padding, string? expected)
        {
            var result = NumberFormat.FromPadding(padding);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void TemplateFormatter_ValidTemplate_ReturnsNull()
        {
            // Arrange — подготовка
            var template = "{Prefix}-{Уровень}-{N:000}";
            var value = "А-1-001";

            // Act — действие
            var error = TemplateFormatter.ValidateValue(template, value);

            // Assert — проверка
            Assert.Null(error);  // null = ошибок нет
        }

        [Fact]
        public void FindDuplicateNewValues_TwoSameValues_ReturnsOneConflict()
        {
            // Arrange
            var elements = new List<ElementFromModel>
            {
                new () { Id = 1, Name = "Стена А", NewValue = "А-1-001" },
                new () { Id = 2, Name = "Стена Б", NewValue = "А-1-001" },
                new () { Id = 3, Name = "Стена В", NewValue = "А-1-002" }
            };

            // Act
            var conflicts = NumberingConflictChecker.FindDuplicateNewValues(elements);

            // Assert
            Assert.Equal("А-1-001", conflicts[0].NewValue);
        }
    }
}
