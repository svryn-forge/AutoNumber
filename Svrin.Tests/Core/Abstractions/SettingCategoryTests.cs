using Svrin.Core.Abstractions;
using Xunit;

namespace Svrin.Tests.Core.Abstractions
{
    public class SettingCategoryTests
    {
        // Дефолты

        [Fact]
        public void Defaults_AreExpected()
        {
            var c = new SettingCategory();

            Assert.Equal(-1L, c.Id);
            Assert.Equal("", c.Name);
            Assert.Equal("", c.Prefix);
            Assert.Equal("", c.ParameterName);
            Assert.False(c.IsSelected);
            Assert.Equal(3, c.Padding);
            Assert.Equal("{Prefix}-{Уровень}-{N:000}", c.Template);
        }

        // Padding → Template

        [Theory]
        [InlineData(1, "{Prefix}-{Уровень}-{N:0}")]
        [InlineData(2, "{Prefix}-{Уровень}-{N:00}")]
        [InlineData(3, "{Prefix}-{Уровень}-{N:000}")]
        [InlineData(5, "{Prefix}-{Уровень}-{N:00000}")]
        public void Padding_RebuildsTemplate(int padding, string expectedTemplate)
        {
            var c = new SettingCategory
            {
                Padding = padding
            };

            Assert.Equal(padding, c.Padding);
            Assert.Equal(expectedTemplate, c.Template);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-1, 1)]
        [InlineData(-100, 1)]
        public void Padding_LessThanOne_ClampedToOne(int input, int expected)
        {
            var c = new SettingCategory
            {
                Padding = input
            };

            Assert.Equal(expected, c.Padding);
            Assert.Equal("{Prefix}-{Уровень}-{N:0}", c.Template);
        }

        [Fact]
        public void Padding_NoUpperClamp()
        {
            var c = new SettingCategory
            {
                Padding = 10
            };

            Assert.Equal(10, c.Padding);
            Assert.Equal("{Prefix}-{Уровень}-{N:0000000000}", c.Template);
        }

        [Fact]
        public void Padding_SameValue_DoesNotRebuildTemplateOrRaiseEvent()
        {
            var c = new SettingCategory();   // Padding = 3, Template = дефолт

            var raised = 0;
            c.PropertyChanged += (s, e) => raised++;

            c.Padding = 3;

            Assert.Equal(0, raised);
            Assert.Equal("{Prefix}-{Уровень}-{N:000}", c.Template);
        }

        [Fact]
        public void Padding_Change_RaisesPropertyChangedForPaddingAndTemplate()
        {
            var c = new SettingCategory();
            var names = new List<string?>();
            c.PropertyChanged += (s, e) => names.Add(e.PropertyName);

            c.Padding = 4;

            Assert.Contains(nameof(SettingCategory.Padding), names);
            Assert.Contains(nameof(SettingCategory.Template), names);
        }

        // Prefix не влияет на Template

        [Fact]
        public void Prefix_DoesNotChangeTemplate()
        {
            var c = new SettingCategory();
            var before = c.Template;

            c.Prefix = "АБВ";

            Assert.Equal(before, c.Template);   // Template содержит литерал {Prefix}
            Assert.Equal("АБВ", c.Prefix);
        }

        [Fact]
        public void Prefix_RaisesPropertyChanged()
        {
            var c = new SettingCategory();
            string? name = null;
            c.PropertyChanged += (s, e) => name = e.PropertyName;

            c.Prefix = "X";

            Assert.Equal(nameof(SettingCategory.Prefix), name);
        }

        // IsSelected / ParameterName

        [Fact]
        public void IsSelected_RaisesPropertyChanged()
        {
            var c = new SettingCategory();
            string? name = null;
            c.PropertyChanged += (s, e) => name = e.PropertyName;

            c.IsSelected = true;

            Assert.Equal(nameof(SettingCategory.IsSelected), name);
            Assert.True(c.IsSelected);
        }

        [Fact]
        public void ParameterName_RaisesPropertyChanged()
        {
            var c = new SettingCategory();
            string? name = null;
            c.PropertyChanged += (s, e) => name = e.PropertyName;

            c.ParameterName = "Марка";

            Assert.Equal(nameof(SettingCategory.ParameterName), name);
        }

        // Template — private set

        [Fact]
        public void Template_CanOnlyBeChangedViaPadding()
        {
            // Компилятор не даст c.Template = "...", поэтому проверяем косвенно:
            // после смены Padding Template соответствует Padding.
            var c = new SettingCategory
            {
                Padding = 5
            };

            Assert.Equal("{Prefix}-{Уровень}-{N:00000}", c.Template);
        }

        // Clone

        [Fact]
        public void Clone_CopiesAllProperties()
        {
            var original = new SettingCategory
            {
                Id = 42,
                Name = "Стены",
                Prefix = "С",
                ParameterName = "Марка",
                Padding = 4,
                IsSelected = true
            };

            var clone = original.Clone();

            Assert.NotSame(original, clone);
            Assert.Equal(original.Id, clone.Id);
            Assert.Equal(original.Name, clone.Name);
            Assert.Equal(original.Prefix, clone.Prefix);
            Assert.Equal(original.ParameterName, clone.ParameterName);
            Assert.Equal(original.Padding, clone.Padding);
            Assert.Equal(original.IsSelected, clone.IsSelected);
            Assert.Equal(original.Template, clone.Template);   // Template восстановлен из Padding
        }

        [Fact]
        public void Clone_IsIndependent()
        {
            var original = new SettingCategory { Prefix = "A", Padding = 2 };
            var clone = original.Clone();

            clone.Prefix = "B";
            clone.Padding = 5;

            Assert.Equal("A", original.Prefix);
            Assert.Equal(2, original.Padding);
            Assert.Equal("{Prefix}-{Уровень}-{N:00}", original.Template);
        }

        [Fact]
        public void Clone_DefaultPadding_KeepsDefaultTemplate()
        {
            var original = new SettingCategory();   // Padding = 3
            var clone = original.Clone();

            Assert.Equal(3, clone.Padding);
            Assert.Equal("{Prefix}-{Уровень}-{N:000}", clone.Template);
        }

        [Fact]
        public void Clone_NonDefaultPadding_RebuildsTemplate()
        {
            var original = new SettingCategory { Padding = 7 };
            var clone = original.Clone();

            Assert.Equal(7, clone.Padding);
            Assert.Equal("{Prefix}-{Уровень}-{N:0000000}", clone.Template);
        }

        [Fact]
        public void Clone_ReturnsNewInstance()
        {
            var original = new SettingCategory();
            var clone = original.Clone();

            Assert.NotSame(original, clone);
        }
    }
}
