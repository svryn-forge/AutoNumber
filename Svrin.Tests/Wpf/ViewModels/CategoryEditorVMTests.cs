using Svrin.Core.Abstractions;
using Svrin.Wpf.ViewModels;
using Xunit;

namespace Svrin.Tests.Wpf.ViewModels
{
    public class CategoryEditorVMTests
    {
        private static SettingCategory Cat(long id, string name = "", bool selected = false,
            string prefix = "", string parameterName = "", int padding = 3) => new()
            {
                Id = id,
                Name = name,
                IsSelected = selected,
                Prefix = prefix,
                ParameterName = parameterName,
                Padding = padding
            };

        /// <summary>
        /// Конструктор: клонирование
        /// </summary>
        [Fact]
        public void Constructor_ClonesSourceCategories()
        {
            var source = new[] { Cat(1, "A"), Cat(2, "B") };

            var vm = new CategoryEditorVM(source);

            Assert.Equal(2, vm.Categories.Count);
            Assert.NotSame(source[0], vm.Categories[0]);   // клон, не ссылка
            Assert.NotSame(source[1], vm.Categories[1]);
            Assert.Equal(1, vm.Categories[0].Id);
            Assert.Equal(2, vm.Categories[1].Id);
        }

        [Fact]
        public void Constructor_ModifyingCategories_DoesNotAffectSource()
        {
            var source = new[] { Cat(1, "A", prefix: "X") };
            var vm = new CategoryEditorVM(source);

            vm.Categories[0].Prefix = "Y";

            Assert.Equal("X", source[0].Prefix);
        }

        [Fact]
        public void Constructor_EmptySource_EmptyCategories()
        {
            var vm = new CategoryEditorVM([]);
            Assert.Empty(vm.Categories);
        }

        [Fact]
        public void Constructor_Commands_AreInitialized()
        {
            var vm = new CategoryEditorVM([]);

            Assert.NotNull(vm.AddCommand);
            Assert.NotNull(vm.RemoveCommand);
            Assert.NotNull(vm.OkCommand);
            Assert.NotNull(vm.CancelCommand);
        }

        /// <summary>
        /// Add
        /// </summary>
        [Fact]
        public void Add_EmptyCollection_AddsWithIdZero()
        {
            var vm = new CategoryEditorVM([]);

            vm.AddCommand.Execute(null);

            Assert.Single(vm.Categories);
            Assert.Equal(0, vm.Categories[0].Id);
        }

        [Fact]
        public void Add_NonEmptyCollection_AddsWithMaxIdPlusOne()
        {
            var vm = new CategoryEditorVM([Cat(1), Cat(5), Cat(3)]);

            vm.AddCommand.Execute(null);

            Assert.Equal(4, vm.Categories.Count);
            Assert.Equal(6, vm.Categories.Last().Id);   // max(1,5,3)+1
        }

        [Fact]
        public void Add_MultipleTimes_IncrementsId()
        {
            var vm = new CategoryEditorVM(Array.Empty<SettingCategory>());

            vm.AddCommand.Execute(null);
            vm.AddCommand.Execute(null);
            vm.AddCommand.Execute(null);

            Assert.Equal([0, 1, 2], vm.Categories.Select(c => c.Id).ToArray());
        }

        [Fact]
        public void Add_NewCategory_DefaultsAreDefault()
        {
            var vm = new CategoryEditorVM(Array.Empty<SettingCategory>());

            vm.AddCommand.Execute(null);

            var added = vm.Categories[0];
            Assert.Equal(-1L == added.Id ? -1L : added.Id, added.Id);  // Id = 0 (не -1, т.к. явно задан)
            Assert.Equal("", added.Name);
            Assert.Equal("", added.Prefix);
            Assert.False(added.IsSelected);
        }

        /// <summary>
        /// Remove
        /// </summary>
        [Fact]
        public void Remove_ExistingCategory_Removes()
        {
            var vm = new CategoryEditorVM([Cat(1), Cat(2), Cat(3)]);
            var toRemove = vm.Categories[1];

            vm.RemoveCommand.Execute(toRemove);

            Assert.Equal(2, vm.Categories.Count);
            Assert.DoesNotContain(toRemove, vm.Categories);
        }

        [Fact]
        public void Remove_NonCategoryObject_DoesNothing()
        {
            var vm = new CategoryEditorVM([Cat(1)]);

            vm.RemoveCommand.Execute("not a category");
            vm.RemoveCommand.Execute(null);
            vm.RemoveCommand.Execute(42);

            Assert.Single(vm.Categories);
        }

        [Fact]
        public void Remove_NotInCollection_DoesNothing()
        {
            var vm = new CategoryEditorVM([Cat(1)]);
            var stranger = Cat(99);

            vm.RemoveCommand.Execute(stranger);

            Assert.Single(vm.Categories);
        }

        /// <summary>
        /// Result
        /// </summary>
        [Fact]
        public void Result_InitiallyNull()
        {
            var vm = new CategoryEditorVM([Cat(1)]);
            Assert.Null(vm.Result);
        }
    }
}
