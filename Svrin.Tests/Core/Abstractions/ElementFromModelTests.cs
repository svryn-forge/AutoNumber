using Svrin.Core.Abstractions;
using Xunit;

namespace Svrin.Tests.Core.Abstractions
{
    public class ElementFromModelTests
    {
        private static ElementFromModel CreateElement() => new()
        {
            Id = 1,
            Name = "Стена",
            UniqueId = "uid",
            CategoryId = 100,
            Level = "1",
            LevelElevation = 0.0
        };

        private static SettingCategory CreateSettings(
            string prefix = "А",
            string parameterName = "Номер",
            int padding = 3,
            string template = "{Prefix}-{Уровень}-{N:000}") => new()
            {
                Id = 1,
                Name = "Категория",
                Prefix = prefix,
                ParameterName = parameterName,
                Padding = padding,
            };

        // UpdateNewValue через Settings.set

        [Fact]
        public void Settings_Set_BuildsNewValueFromPrefixLevelNumber()
        {
            var el = CreateElement();
            el.Number = 7;
            el.Settings = CreateSettings(prefix: "А", padding: 3);

            Assert.Equal("А-1-007", el.NewValue);
        }

        [Fact]
        public void Settings_Set_NoPrefix_OmitsPrefixAndLeadingSeparator()
        {
            var el = CreateElement();
            el.Number = 5;
            el.Settings = CreateSettings(prefix: "", padding: 2);

            // Prefix пуст → AddValue пропускает, AddSeparator после него не ставит '-'
            Assert.Equal("1-05", el.NewValue);
        }

        [Fact]
        public void Settings_Set_OnlyNumber()
        {
            var el = CreateElement();
            el.Level = "";
            el.Number = 42;
            el.Settings = CreateSettings(prefix: "", padding: 4);

            Assert.Equal("0042", el.NewValue);
        }

        [Fact]
        public void Settings_Set_Null_DoesNotUpdateNewValue()
        {
            var el = CreateElement();
            el.NewValue = "existing";
            el.Settings = null;

            Assert.Equal("existing", el.NewValue);   // set → return до UpdateNewValue
        }

        [Fact]
        public void Settings_Set_Again_RebuildsNewValue()
        {
            var el = CreateElement();
            el.Number = 1;
            el.Settings = CreateSettings(prefix: "А", padding: 1);
            Assert.Equal("А-1-1", el.NewValue);

            el.Settings = CreateSettings(prefix: "Б", padding: 2);
            Assert.Equal("Б-1-01", el.NewValue);
        }

        [Fact]
        public void Number_Change_DoesNotRebuildNewValue()
        {
            // Фиксируем текущее поведение: сеттер Number не вызывает UpdateNewValue
            var el = CreateElement();
            el.Number = 1;
            el.Settings = CreateSettings(prefix: "А", padding: 1);
            var before = el.NewValue;

            el.Number = 2;

            Assert.Equal(before, el.NewValue);   // не пересчиталось
        }

        // NewValueError / HasErrors / GetErrors

        [Fact]
        public void NewValueError_NullSettings_ReturnsNull()
        {
            var el = CreateElement();
            el.NewValue = "anything";

            Assert.Null(el.NewValueError);
            Assert.False(el.HasErrors);
        }

        [Fact]
        public void NewValueError_ValidValue_ReturnsNull()
        {
            var el = CreateElement();
            el.Settings = CreateSettings();   // шаблон {Prefix}-{Уровень}-{N:000}
            el.NewValue = "А-1-001";

            Assert.Null(el.NewValueError);
            Assert.False(el.HasErrors);
        }

        [Fact]
        public void NewValueError_InvalidValue_ReturnsMessage()
        {
            var el = CreateElement();
            el.Settings = CreateSettings();
            el.NewValue = "невалидно";

            Assert.NotNull(el.NewValueError);
            Assert.True(el.HasErrors);
        }

        [Fact]
        public void HasErrors_TrueWhenNewValueErrorNotNull()
        {
            var el = CreateElement();
            el.Settings = CreateSettings();
            el.NewValue = "x";   // не проходит шаблон

            Assert.True(el.HasErrors);
        }

        [Fact]
        public void GetErrors_NewValue_WithError_ReturnsErrorArray()
        {
            var el = CreateElement();
            el.Settings = CreateSettings();
            el.NewValue = "x";

            var errors = el.GetErrors(nameof(ElementFromModel.NewValue)).Cast<string>().ToArray();

            Assert.Single(errors);
            Assert.Equal(el.NewValueError, errors[0]);
        }

        [Fact]
        public void GetErrors_NewValue_NoError_ReturnsEmpty()
        {
            var el = CreateElement();
            el.Settings = CreateSettings();
            el.NewValue = "А-1-001";

            var errors = el.GetErrors(nameof(ElementFromModel.NewValue)).Cast<string>().ToArray();

            Assert.Empty(errors);
        }

        [Theory]
        [InlineData("Id")]
        [InlineData("Name")]
        [InlineData("CurrentValue")]
        [InlineData(null)]
        [InlineData("Unknown")]
        public void GetErrors_OtherProperty_ReturnsEmpty(string? propertyName)
        {
            var el = CreateElement();
            el.Settings = CreateSettings();
            el.NewValue = "x";   // есть ошибка, но запрос не про NewValue

            var errors = el.GetErrors(propertyName).Cast<string>().ToArray();

            Assert.Empty(errors);
        }

        // События

        [Fact]
        public void NewValue_Set_Changed_RaisesPropertyChangedAndErrorsChanged()
        {
            var el = CreateElement();
            el.Settings = CreateSettings();

            var propertyNames = new List<string?>();
            var errorsChanged = 0;
            el.PropertyChanged += (s, e) => propertyNames.Add(e.PropertyName);
            el.ErrorsChanged += (s, e) => errorsChanged++;

            el.NewValue = "x";

            Assert.Contains(nameof(ElementFromModel.NewValue), propertyNames);
            Assert.Contains(nameof(ElementFromModel.NewValueError), propertyNames);
            Assert.Contains(nameof(ElementFromModel.HasErrors), propertyNames);
            Assert.Equal(1, errorsChanged);
        }

        [Fact]
        public void NewValue_Set_SameValue_DoesNotRaiseEvents()
        {
            var el = CreateElement();
            el.NewValue = "x";

            var raised = 0;
            el.ErrorsChanged += (s, e) => raised++;

            el.NewValue = "x";   // то же значение

            Assert.Equal(0, raised);
        }

        [Fact]
        public void ErrorsChanged_PropertyName_IsNewValue()
        {
            var el = CreateElement();
            el.Settings = CreateSettings();

            string? name = null;
            el.ErrorsChanged += (s, e) => name = e.PropertyName;

            el.NewValue = "x";

            Assert.Equal(nameof(ElementFromModel.NewValue), name);
        }

        // IsEquals

        [Fact]
        public void IsEquals_SameValues_True()
        {
            var el = CreateElement();
            el.CurrentValue = "A-1-001";
            el.NewValue = "A-1-001";

            Assert.True(el.IsEquals);
        }

        [Fact]
        public void IsEquals_DifferentCase_True()
        {
            var el = CreateElement();
            el.CurrentValue = "a-1-001";
            el.NewValue = "A-1-001";

            Assert.True(el.IsEquals);   // InvariantCultureIgnoreCase
        }

        [Fact]
        public void IsEquals_DifferentValues_False()
        {
            var el = CreateElement();
            el.CurrentValue = "A-1-001";
            el.NewValue = "A-1-002";

            Assert.False(el.IsEquals);
        }

        [Fact]
        public void IsEquals_BothEmpty_True()
        {
            var el = CreateElement();
            // CurrentValue = "", NewValue = "" по умолчанию
            Assert.True(el.IsEquals);
        }

        // ToString

        [Fact]
        public void ToString_UsesInvariantOrCurrentCulture_BeAware()
        {
            var el = CreateElement();
            el.Id = 42;
            el.X = 1.5;
            el.Y = 2.5;
            el.Z = 3.5;
            el.Level = "L1";

            var s = el.ToString();

            // ToString() интерполирует double через CurrentCulture.
            // В ru-RU будет "1,5", в en-US — "1.5".
            // Тест фиксирует, что формат зависит от культуры:
            Assert.Contains("42", s);
            Assert.Contains("L1", s);
            // Проверьте, что X/Y/Z присутствуют в каком-то виде:
            Assert.Matches(@"1[.,]5", s);
        }

        // IsChecked / PropertyChanged базовых свойств

        [Fact]
        public void IsChecked_RaisesPropertyChanged()
        {
            var el = CreateElement();
            string? name = null;
            el.PropertyChanged += (s, e) => name = e.PropertyName;

            el.IsChecked = true;

            Assert.Equal(nameof(ElementFromModel.IsChecked), name);
            Assert.True(el.IsChecked);
        }

        [Fact]
        public void CurrentValue_RaisesPropertyChanged()
        {
            var el = CreateElement();
            string? name = null;
            el.PropertyChanged += (s, e) => name = e.PropertyName;

            el.CurrentValue = "x";

            Assert.Equal(nameof(ElementFromModel.CurrentValue), name);
        }
    }
}
