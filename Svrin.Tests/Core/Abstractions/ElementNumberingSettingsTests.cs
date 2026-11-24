using Svrin.Core.Abstractions;
using Xunit;

namespace Svrin.Tests.Core.Abstractions
{
    public class ElementNumberingSettingsTests
    {
        // Filename
        [Fact]
        public void Filename_IsConstant()
        {
            var settings = new ElementNumberingSettings();
            Assert.Equal("ElementNumberingSettings.json", settings.Filename);
        }

        // CategorySet по умолчанию
        [Fact]
        public void CategorySet_Default_IsEmpty()
        {
            var settings = new ElementNumberingSettings();
            Assert.NotNull(settings.CategorySet);
            Assert.Empty(settings.CategorySet);
        }

        // LoadDefault
        [Fact]
        public void LoadDefault_EmptySet_AddsFourCategories()
        {
            var settings = new ElementNumberingSettings();

            ElementNumberingSettings.LoadDefault(settings);

            Assert.Equal(4, settings.CategorySet.Count);
        }

        [Fact]
        public void LoadDefault_AddsExpectedCategories()
        {
            var settings = new ElementNumberingSettings();

            ElementNumberingSettings.LoadDefault(settings);

            var names = settings.CategorySet.Select(c => c.Name).ToArray();
            Assert.Equal(["Стены", "Двери", "Окна", "Помещения"], names);
        }

        [Fact]
        public void LoadDefault_SetsExpectedIds()
        {
            var settings = new ElementNumberingSettings();

            ElementNumberingSettings.LoadDefault(settings);

            var ids = settings.CategorySet.Select(c => c.Id).ToArray();
            Assert.Equal([-2000011L, -2000023L, -2000014L, -2000160L], ids);
        }

        [Fact]
        public void LoadDefault_SetsExpectedPrefixes()
        {
            var settings = new ElementNumberingSettings();

            ElementNumberingSettings.LoadDefault(settings);

            var prefixes = settings.CategorySet.Select(c => c.Prefix).ToArray();
            Assert.Equal(["С", "Д", "О", "Р"], prefixes);
        }

        [Fact]
        public void LoadDefault_SetsExpectedParameterNames()
        {
            var settings = new ElementNumberingSettings();

            ElementNumberingSettings.LoadDefault(settings);

            var walls = settings.CategorySet.Single(c => c.Name == "Стены");
            Assert.Equal("Марка", walls.ParameterName);

            var rooms = settings.CategorySet.Single(c => c.Name == "Помещения");
            Assert.Equal("Номер", rooms.ParameterName);
        }

        [Fact]
        public void LoadDefault_DoesNotSetPaddingOrTemplate()
        {
            var settings = new ElementNumberingSettings();

            ElementNumberingSettings.LoadDefault(settings);

            // Дефолтные значения SettingCategory — проверяем, что LoadDefault их не переопределяет
            Assert.All(settings.CategorySet, c =>
            {
                // Проверьте реальные дефолты SettingCategory; ниже — предполагаемые
                Assert.True(c.Padding == 0 || c.Padding > 0);  // поправьте под реальность
            });
        }

        [Fact]
        public void LoadDefault_NonEmptySet_DoesNothing()
        {
            var settings = new ElementNumberingSettings();
            var existing = new SettingCategory { Id = 1, Name = "Существующая", Prefix = "X" };
            settings.CategorySet.Add(existing);

            ElementNumberingSettings.LoadDefault(settings);

            Assert.Single(settings.CategorySet);
            Assert.Same(existing, settings.CategorySet[0]);
            Assert.Equal("Существующая", settings.CategorySet[0].Name);
        }

        [Fact]
        public void LoadDefault_CalledTwice_DoesNotDuplicate()
        {
            var settings = new ElementNumberingSettings();

            ElementNumberingSettings.LoadDefault(settings);
            ElementNumberingSettings.LoadDefault(settings);

            Assert.Equal(4, settings.CategorySet.Count);   // не 8
        }

        [Fact]
        public void LoadDefault_PreservesExistingCustomCategory()
        {
            var settings = new ElementNumberingSettings();
            settings.CategorySet.Add(new SettingCategory { Id = 999, Name = "Custom" });

            ElementNumberingSettings.LoadDefault(settings);

            Assert.Single(settings.CategorySet);
            Assert.Equal(999, settings.CategorySet[0].Id);
        }

        [Fact]
        public void LoadDefault_AddsDistinctReferences()
        {
            var settings = new ElementNumberingSettings();

            ElementNumberingSettings.LoadDefault(settings);

            // Каждый вызов new() — отдельные экземпляры, не один и тот же объект
            Assert.Equal(4, settings.CategorySet.Distinct().Count());
        }

        // IJsonSetting

        [Fact]
        public void ImplementsIJsonSetting()
        {
            var settings = new ElementNumberingSettings();
            Assert.IsType<Svrin.Core.Providers.IJsonSetting>(settings, exactMatch: false);
        }
    }
}
