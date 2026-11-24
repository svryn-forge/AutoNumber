using Svrin.Core.Helpers;
using Xunit;

namespace Svrin.Tests.Core.Helpers
{
    public class TemplateFormatterTests
    {
        private const string DefaultTemplate = "{Prefix}-{Уровень}-{N:000}";

        /// <summary>
        /// Валидные значения
        /// </summary>
        /// <param name="value"></param>
        [Theory]
        [InlineData("A-1-001")]
        [InlineData("Б-Уровень_1-123")]
        [InlineData("Z-этаж 2-999")]
        public void DefaultTemplate_ValidValues_ReturnsNull(string value)
        {
            Assert.Null(TemplateFormatter.ValidateValue(DefaultTemplate, value));
        }

        [Fact]
        public void EnglishPlaceholders_Work()
        {
            Assert.Null(TemplateFormatter.ValidateValue("{Prefix}-{Level}-{Number:000}", "A-1-001"));
        }

        /// <summary>
        /// {Prefix} → ровно одна буква
        /// </summary>
        [Fact]
        public void Prefix_TwoLetters_IsInvalid()
        {
            // \p{L} без + → ровно 1 буква
            Assert.NotNull(TemplateFormatter.ValidateValue("{Prefix}-{N}", "AB-1"));
        }

        [Fact]
        public void Prefix_Digit_IsInvalid()
        {
            Assert.NotNull(TemplateFormatter.ValidateValue("{Prefix}-{N}", "1-1"));
        }

        [Theory]
        [InlineData("A-1")]
        [InlineData("я-1")]  // кириллица
        [InlineData("Ω-1")]  // греческая
        public void Prefix_OneLetter_IsValid(string value)
        {
            Assert.Null(TemplateFormatter.ValidateValue("{Prefix}-{N}", value));
        }

        /// <summary>
        /// {Уровень} / {Level} → .+?
        /// </summary>
        /// <param name="value"></param>
        [Theory]
        [InlineData("A-1-001")]
        [InlineData("A-Уровень 1-001")]
        [InlineData("A-этаж_2-001")]
        [InlineData("A-x-001")]         // даже одна буква как уровень
        public void Level_AnyNonEmpty_IsValid(string value)
        {
            Assert.Null(TemplateFormatter.ValidateValue(DefaultTemplate, value));
        }

        [Fact]
        public void Level_Empty_IsInvalid()
        {
            // .+? требует минимум 1 символ
            Assert.NotNull(TemplateFormatter.ValidateValue(DefaultTemplate, "A--001"));
        }

        /// <summary>
        /// {N:000} → ровно N цифр
        /// </summary>
        /// <param name="value"></param>
        [Theory]
        [InlineData("A-1-1")]     // 1 цифра при :000 → невалидно
        [InlineData("A-1-12")]    // 2 цифры → невалидно
        [InlineData("A-1-1234")]  // 4 цифры → невалидно
        public void Padding000_WrongLength_IsInvalid(string value)
        {
            Assert.NotNull(TemplateFormatter.ValidateValue(DefaultTemplate, value));
        }

        [Theory]
        [InlineData("A-1-000")]
        [InlineData("A-1-999")]
        public void Padding000_ThreeDigits_IsValid(string value)
        {
            Assert.Null(TemplateFormatter.ValidateValue(DefaultTemplate, value));
        }

        [Theory]
        [InlineData("{N}", "1")]
        [InlineData("{N}", "12345")]
        [InlineData("{N}", "0")]               // formatter про знак не знает — формат ок
        [InlineData("{N:0}", "1")]
        [InlineData("{N:00}", "12")]
        [InlineData("{N:000}", "123")]
        [InlineData("{N:000}", "007")]         // ведущие нули — ок
        [InlineData("{N:000}", "000")]         // все нули — тоже ок для formatter
        public void NumberFormat_Valid(string template, string value)
        {
            Assert.Null(TemplateFormatter.ValidateValue(template, value));
        }

        [Theory]
        [InlineData("{N:0}", "12")]            // \d{1}
        [InlineData("{N:0}", "")]
        [InlineData("{N:00}", "1")]            // \d{2}
        [InlineData("{N:00}", "123")]          // \d{2} — раньше проходило из-за бага
        [InlineData("{N:000}", "12")]
        [InlineData("{N:000}", "1234")]
        [InlineData("{N}", "")]                // \d+ не матчит пусто
        [InlineData("{N}", "-1")]              // минус не цифра → формат не тот
        [InlineData("{N}", "1.5")]
        [InlineData("{N}", " 1")]
        [InlineData("{N}", "1 ")]
        public void NumberFormat_Invalid(string template, string value)
        {
            Assert.Equal("Значение не соответствует шаблону.",
                TemplateFormatter.ValidateValue(template, value));
        }

        /// <summary>
        /// Литералы в шаблоне экранируются
        /// </summary>
        [Fact]
        public void LiteralDotsAreEscaped()
        {
            // "." в шаблоне — литерал, а не любой символ
            Assert.Null(TemplateFormatter.ValidateValue("{N}.{N}", "1.2"));
            Assert.NotNull(TemplateFormatter.ValidateValue("{N}.{N}", "1x2"));
        }

        [Fact]
        public void ExactMatch_Required()
        {
            // ^...$ — лишние символы запрещены
            Assert.NotNull(TemplateFormatter.ValidateValue("{N}", "1 "));
            Assert.NotNull(TemplateFormatter.ValidateValue("{N}", " 1"));
            Assert.NotNull(TemplateFormatter.ValidateValue("{N}", "a1"));
        }

        /// <summary>
        /// Неизвестный плейсхолдер (фиксируем текущее поведение)
        /// </summary>
        [Fact]
        public void UnknownPlaceholder_IsTreatedAsLiteral()
        {
            // {Foo} не распознан → экранируется как литерал "{Foo}"
            Assert.Null(TemplateFormatter.ValidateValue("{Foo}-{N}", "{Foo}-1"));
            Assert.NotNull(TemplateFormatter.ValidateValue("{Foo}-{N}", "Bar-1"));
        }

        /// <summary>
        /// null / пустые
        /// </summary>
        [Fact]
        public void NullValue_IsTreatedAsEmpty()
        {
            Assert.Null(TemplateFormatter.ValidateValue("", null!));
        }

        [Fact]
        public void EmptyTemplate_EmptyValue_IsValid()
        {
            Assert.Null(TemplateFormatter.ValidateValue("", ""));
        }

        [Fact]
        public void EmptyTemplate_NonEmptyValue_IsInvalid()
        {
            Assert.NotNull(TemplateFormatter.ValidateValue("", "x"));
        }

        /// <summary>
        /// Возвращаемое сообщение
        /// </summary>
        [Fact]
        public void InvalidValue_ReturnsSpecificMessage()
        {
            var error = TemplateFormatter.ValidateValue("{N}", "abc");
            Assert.Equal("Значение не соответствует шаблону.", error);
        }
    }
}
