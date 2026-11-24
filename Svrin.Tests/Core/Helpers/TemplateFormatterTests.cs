using Svrin.Core.Helpers;
using Xunit;

namespace Svrin.Tests.Core.Helpers
{
    public class TemplateFormatterTests
    {
        private const string Template = "{Prefix}-{Уровень}-{N:00}";
        private const string Prefix = "WW";
        private const string Level = "Level 2";

        [Theory]
        [InlineData("WW-Level 2-27")]
        [InlineData("WW-Level 2-01")]
        [InlineData("WW-Level 2-99")]
        [InlineData("WW-Level 2-10")]
        public void ValidateValue_ValidValue_ReturnsNull(string value)
        {
            var result = TemplateFormatter.ValidateValue(
                Template, value, Prefix, Level);

            Assert.Null(result);
        }

        [Theory]
        [InlineData("{Prefix}-{Уровень}-{N:000}", "WW-Level 2-001", "WW", "Level 2")]
        [InlineData("{Prefix}-{Уровень}-{N:000}", "WW-Level 2-027", "WW", "Level 2")]
        [InlineData("{Prefix}-{Уровень}-{N:000}", "WW-Level 2-999", "WW", "Level 2")]
        public void ValidateValue_CustomPadding_ReturnsNull(
            string template,
            string value,
            string prefix,
            string level)
        {
            var result = TemplateFormatter.ValidateValue(
                template, value, prefix, level);

            Assert.Null(result);
        }

        [Fact]
        public void ValidateValue_NullValue_ReturnsError()
        {
            var result = TemplateFormatter.ValidateValue(
                Template, null!, Prefix, Level);

            Assert.NotNull(result);
        }

        [Fact]
        public void ValidateValue_EmptyValue_ReturnsError()
        {
            var result = TemplateFormatter.ValidateValue(
                Template, string.Empty, Prefix, Level);

            Assert.NotNull(result);
        }
    }
}
