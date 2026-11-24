using NSubstitute;
using Svrin.Core.Abstractions;
using Svrin.Wpf.Abstractions;
using Svrin.Wpf.ViewModels;
using Xunit;

namespace Svrin.Tests.Wpf.ViewModels
{
    public class NumberingVMTests
    {
        private static NumberingVM CreateVm(
            IWindowService? ws = null,
            INumberingHandlers? handlers = null) =>
            new(ws ?? Substitute.For<IWindowService>(),
                handlers ?? Substitute.For<INumberingHandlers>());

        private static ElementFromModel El(long id, long categoryId, string name = "") => new()
        {
            Id = id,
            CategoryId = categoryId,
            Name = name
        };

        /// <summary>
        /// Конструктор
        /// </summary>
        [Fact]
        public void Constructor_InitializesSearchFields()
        {
            var vm = CreateVm();

            Assert.Equal(5, vm.SearchFields.Count);
            Assert.Equal(SearchField.Id, vm.SearchFields[0].Field);
            Assert.Equal("Id", vm.SearchFields[0].Title);
        }

        [Fact]
        public void Constructor_SelectedSearchField_IsFirst()
        {
            var vm = CreateVm();
            Assert.Same(vm.SearchFields[0], vm.SelectedSearchField);
        }

        [Fact]
        public void Constructor_Commands_NotNull()
        {
            var vm = CreateVm();

            Assert.NotNull(vm.CloseCommand);
            Assert.NotNull(vm.ApplyCommand);
            Assert.NotNull(vm.LoadCommand);
            Assert.NotNull(vm.OpenSettingsCommand);
        }

        /// <summary>
        /// Команды: моки
        /// </summary>
        [Fact]
        public void CloseCommand_CallsWindowServiceClose()
        {
            var ws = Substitute.For<IWindowService>();
            var vm = CreateVm(ws);

            vm.CloseCommand.Execute(null);

            ws.Received(1).Close();
        }

        [Fact]
        public void ApplyCommand_CallsHandlersApplyWithThis()
        {
            var handlers = Substitute.For<INumberingHandlers>();
            var vm = CreateVm(handlers: handlers);

            vm.ApplyCommand.Execute(null);

            handlers.Received(1).Apply(vm);
        }

        [Fact]
        public void LoadCommand_CallsHandlersLoadWithThis()
        {
            var handlers = Substitute.For<INumberingHandlers>();
            var vm = CreateVm(handlers: handlers);

            vm.LoadCommand.Execute(null);

            handlers.Received(1).Load(vm);
        }

        [Fact]
        public void OpenSettingsCommand_CallsHandlersChangeSettings()
        {
            var handlers = Substitute.For<INumberingHandlers>();
            var vm = CreateVm(handlers: handlers);

            vm.OpenSettingsCommand.Execute(null);

            handlers.Received(1).ChangeSettings(vm);
        }

        /// <summary>
        /// Settings.set
        /// </summary>
        [Fact]
        public void Settings_SetNull_ClearsElements()
        {
            var vm = CreateVm();
            vm.Elements.Add(El(1, 100));

            vm.Settings = null;

            Assert.Empty(vm.Elements);
        }

        [Fact]
        public void Settings_Set_AssignsMatchingCategory()
        {
            var vm = CreateVm();
            var el = El(1, 100);
            vm.Elements.Add(el);

            var category = new SettingCategory { Id = 100, Name = "Стены" };
            var settings = new ElementNumberingSettings();
            settings.CategorySet.Add(category);

            vm.Settings = settings;

            Assert.Same(category, el.Settings);
        }

        [Fact]
        public void Settings_Set_RemovesElementsWithoutMatchingCategory()
        {
            var vm = CreateVm();
            vm.Elements.Add(El(1, 100));
            vm.Elements.Add(El(2, 200));
            vm.Elements.Add(El(3, 999));   // нет категории

            var settings = new ElementNumberingSettings();
            settings.CategorySet.Add(new SettingCategory { Id = 100 });
            settings.CategorySet.Add(new SettingCategory { Id = 200 });

            vm.Settings = settings;

            Assert.Equal(2, vm.Elements.Count);
            Assert.DoesNotContain(vm.Elements, e => e.Id == 3);
        }

        [Fact]
        public void Settings_Set_AllMatched_NoRemoval()
        {
            var vm = CreateVm();
            vm.Elements.Add(El(1, 100));
            vm.Elements.Add(El(2, 100));

            var settings = new ElementNumberingSettings();
            settings.CategorySet.Add(new SettingCategory { Id = 100 });

            vm.Settings = settings;

            Assert.Equal(2, vm.Elements.Count);
        }

        [Fact]
        public void Settings_Set_SameValue_DoesNotRaisePropertyChanged()
        {
            var vm = CreateVm();
            var settings = new ElementNumberingSettings();
            vm.Settings = settings;

            var raised = 0;
            vm.PropertyChanged += (s, e) => raised++;

            vm.Settings = settings;   // та же ссылка

            Assert.Equal(0, raised);
        }

        /// <summary>
        /// ShowTimeReport
        /// </summary>
        [Fact]
        public void ShowTimeReport_RaisesPropertyChanged()
        {
            var vm = CreateVm();
            string? name = null;
            vm.PropertyChanged += (s, e) => name = e.PropertyName;

            vm.ShowTimeReport = true;

            Assert.Equal(nameof(NumberingVM.ShowTimeReport), name);
        }

        /// <summary>
        /// SelectedSearchField
        /// </summary>
        [Fact]
        public void SelectedSearchField_Change_RaisesPropertyChanged()
        {
            var vm = CreateVm();
            string? name = null;
            vm.PropertyChanged += (s, e) => name = e.PropertyName;

            vm.SelectedSearchField = vm.SearchFields[1];

            Assert.Equal(nameof(NumberingVM.SelectedSearchField), name);
        }
    }
}
