using Svrin.Core.Abstractions;
using Svrin.Wpf.Commands;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Svrin.Wpf.ViewModels
{
    public class CategoryEditorVM : BaseViewModel
    {
        public ObservableCollection<SettingCategory> Categories { get; }
        private readonly List<SettingCategory> _originals;

        public CategoryEditorVM(IEnumerable<SettingCategory> source)
        {
            _originals = source.Select(c => c.Clone()).ToList();
            Categories = Categories = new ObservableCollection<SettingCategory>(_originals.Select(c => c.Clone()));

            AddCommand = new RelayCommand(Add);
            RemoveCommand = new RelayCommand(Remove);
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Close);
        }

        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>Результат после ОК — новые категории.</summary>
        public List<SettingCategory>? Result { get; private set; }

        private void Add(object? _)
        {
            var nextId = Categories.Count == 0 ? 0 : Categories.Max(c => c.Id) + 1;
            Categories.Add(new SettingCategory { Id = nextId });
        }

        private void Remove(object? obj)
        {
            if (obj is SettingCategory c)
                Categories.Remove(c);
        }

        private void Close(object? obj)
        {
            if (obj is Window w)
                w.Close();
        }

        private void Ok(object? obj)
        {
            if (obj is not Window w)
                return;

            var temp = Categories.Where(x => x.IsSelected).Select(c => c.Clone()).ToList();
            if (!Validate(temp, out var error))
            {
                MessageBox.Show(error, "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Result = temp;
            w!.DialogResult = true;
        }

        private static bool Validate(IEnumerable<SettingCategory> categories, out string error)
        {
            error = string.Empty;

            if (categories.Any(c => string.IsNullOrWhiteSpace(c.ParameterName)))
            { error = "У всех категорий должно быть заполнено ParameterName."; return false; }

            if (categories.Any(c => c.Padding < 0))
            { error = "Padding не может быть отрицательным."; return false; }

            return true;
        }
    }
}
