using Svrin.Core.Abstractions;
using Xunit;

namespace Svrin.Tests.Core.Abstractions
{
    public class BaseViewModelTests
    {
        // Наследник с публичной обёрткой над protected-методами
        private class TestViewModel : BaseViewModel
        {
            private int _number;
            private string? _text;

            public int Number
            {
                get => _number;
                set => OnPropertyChanged(ref _number, value);
            }

            public string? Text
            {
                get => _text;
                set => OnPropertyChanged(ref _text, value);
            }

            // Обёртки для проверки возврата и явного имени свойства
            public bool SetNumber(ref int field, int value) => OnPropertyChanged(ref field, value);

            public bool SetNumberNamed(ref int field, int value, string name)
                => OnPropertyChanged(ref field, value, name);

            public void Raise(string name) => OnPropertyChanged(name);
        }

        // OnPropertyChanged<T> (ref-версия)

        [Fact]
        public void OnPropertyChanged_NewValue_UpdatesFieldAndRaisesEvent()
        {
            // Arrange
            var vm = new TestViewModel();
            var raised = new List<string>();
            vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            vm.Number = 42;

            // Assert
            Assert.Equal(42, vm.Number);            // поле обновилось
            Assert.Single(raised);
            Assert.Equal(nameof(TestViewModel.Number), raised[0]);
        }

        [Fact]
        public void OnPropertyChanged_SameValue_DoesNotRaiseEventAndReturnsFalse()
        {
            var vm = new TestViewModel();
            var raised = 0;
            vm.PropertyChanged += (_, _) => raised++;

            // Первый set — изменение
            vm.Number = 7;
            Assert.Equal(1, raised);

            // Второй set того же значения — без события
            vm.Number = 7;
            Assert.Equal(1, raised);   // не выросло
        }

        [Fact]
        public void OnPropertyChanged_ReturnsTrueOnChangeFalseOnSame()
        {
            var vm = new TestViewModel();
            var field = 1;

            Assert.True(vm.SetNumber(ref field, 2));
            Assert.Equal(2, field);

            Assert.False(vm.SetNumber(ref field, 2));
            Assert.Equal(2, field);   // не изменилось
        }

        [Fact]
        public void OnPropertyChanged_ExplicitName_UsedInEvent()
        {
            var vm = new TestViewModel();
            string? raisedName = null;
            vm.PropertyChanged += (_, e) => raisedName = e.PropertyName;

            var field = 0;
            vm.SetNumberNamed(ref field, 1, "CustomName");

            Assert.Equal("CustomName", raisedName);
        }

        [Fact]
        public void OnPropertyChanged_ReferenceType_SameReference_NoEvent()
        {
            var vm = new TestViewModel();
            var text = "hello";
            vm.Text = text;

            var raised = 0;
            vm.PropertyChanged += (_, _) => raised++;

            vm.Text = text;   // тот же экземпляр строки
            Assert.Equal(0, raised);
        }

        [Fact]
        public void OnPropertyChanged_ReferenceType_DifferentValue_Raises()
        {
            var vm = new TestViewModel
            {
                Text = "a"
            };

            var raised = 0;
            vm.PropertyChanged += (_, _) => raised++;

            vm.Text = "b";   // разные строки
            Assert.Equal(1, raised);
        }

        [Fact]
        public void OnPropertyChanged_NullToValue_Raises()
        {
            var vm = new TestViewModel();
            var raised = new List<string>();
            vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            vm.Text = "x";   // было null → стало "x"
            Assert.Single(raised);
            Assert.Equal(nameof(TestViewModel.Text), raised[0]);
        }

        [Fact]
        public void OnPropertyChanged_ValueToNull_Raises()
        {
            var vm = new TestViewModel
            {
                Text = "x"
            };
            var raised = 0;
            vm.PropertyChanged += (_, _) => raised++;

            vm.Text = null;
            Assert.Equal(1, raised);
        }

        [Fact]
        public void OnPropertyChanged_NoSubscribers_DoesNotThrow()
        {
            // PropertyChanged == null — не должно быть NRE
            var vm = new TestViewModel
            {
                Number = 1   // без подписчиков
            };
            Assert.Equal(1, vm.Number);
        }

        // OnPropertyChanged(string) (без сравнения)

        [Fact]
        public void OnPropertyChanged_ByName_AlwaysRaises()
        {
            var vm = new TestViewModel();
            var raised = new List<string>();
            vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            vm.Raise("Foo");
            vm.Raise("Foo");   // даже повторно — событие поднимается

            Assert.Equal(new[] { "Foo", "Foo" }, raised);
        }

        [Fact]
        public void OnPropertyChanged_ByName_NoSubscribers_DoesNotThrow()
        {
            var vm = new TestViewModel();
            vm.Raise("Bar");   // без подписчиков
        }

        // Сравнение через Equals (object)

        [Fact]
        public void OnPropertyChanged_UsesObjectEquals_NotReferenceEquals()
        {
            // Для value-типов Equals — по значению
            var vm = new TestViewModel();
            var field = 1;

            Assert.False(vm.SetNumber(ref field, 1));   // равны по значению → false
        }

        private class CustomEquatable
        {
            public int Value { get; set; }
            public override bool Equals(object? obj) =>
                obj is CustomEquatable other && other.Value == Value;
            public override int GetHashCode() => Value.GetHashCode();
        }

        private class CustomEquatableViewModel : BaseViewModel
        {
            private CustomEquatable? _item;
            public CustomEquatable? Item
            {
                get => _item;
                set => OnPropertyChanged(ref _item, value);
            }
        }

        [Fact]
        public void OnPropertyChanged_CustomEquals_TreatedAsEqual()
        {
            var vm = new CustomEquatableViewModel
            {
                Item = new CustomEquatable { Value = 1 }
            };

            var raised = 0;
            vm.PropertyChanged += (_, _) => raised++;

            // Другой экземпляр, но Equals == true
            vm.Item = new CustomEquatable { Value = 1 };

            Assert.Equal(0, raised);   // событие не поднято — Equals сработал
        }
    }
}
