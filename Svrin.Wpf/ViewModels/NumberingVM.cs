using Svrin.Core.Abstractions;
using Svrin.Wpf.Abstractions;
using Svrin.Wpf.Commands;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Svrin.Wpf.ViewModels
{
    public enum SearchField
    {
        Id,
        Name,
        Level,
        CurrentValue,
        NewValue
    }

    public sealed class SearchFieldOption(SearchField field, string title)
    {
        public SearchField Field { get; } = field;
        public string Title { get; } = title;
    }

    public class NumberingVM : BaseViewModel
    {
        private CancellationTokenSource? _searchCts;
        private readonly IWindowService _windowService;
        private readonly INumberingHandlers _handlers;

        public ICommand CloseCommand => new RelayCommand((_) => _windowService?.Close());
        public ICommand ApplyCommand => new RelayCommand((_) => _handlers.Apply(CurrentDocument, Elements));
        public ICommand LoadCommand => new RelayCommand((_) => _handlers.Load(this));
        public ICommand ClearSearchCommand => new RelayCommand((_) => SearchText = string.Empty);
        public ICommand OpenSettingsCommand => new RelayCommand(OpenSettings);
        public string? CurrentDocument { get; set; }

        public ElementNumberingSettings? Settings { get; set; }
        public ObservableCollection<ElementFromModel> Elements { get; } = [];

        public IReadOnlyList<SearchFieldOption> SearchFields { get; }

        private SearchFieldOption _selectedSearchField;
        public SearchFieldOption SelectedSearchField
        {
            get => _selectedSearchField;
            set
            {
                if (OnPropertyChanged(ref _selectedSearchField, value))
                    ApplyFilter();
            }
        }
        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set
            {
                if (OnPropertyChanged(ref _searchText, value))
                {
                    ScheduleFilter();
                }
            }
        }

        public NumberingVM(IWindowService windowService, INumberingHandlers handlers)
        {
            _windowService = windowService;
            _handlers = handlers;
            SearchFields =
            [
                new SearchFieldOption(SearchField.Id,           "Id"),
                new SearchFieldOption(SearchField.Name,         "Имя"),
                new SearchFieldOption(SearchField.Level,        "Уровень"),
                new SearchFieldOption(SearchField.CurrentValue, "Текущее значение"),
                new SearchFieldOption(SearchField.NewValue,     "Новое значение"),
            ];
            _selectedSearchField = SearchFields.First();
        }

        private void ScheduleFilter()
        {
            if (_searchCts != null)
            {
                _searchCts.Cancel();
                _searchCts.Dispose();
            }

            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            Task.Delay(300, token).ContinueWith(_ =>
            {
                if (token.IsCancellationRequested) return;

                var dispatcher = Application.Current?.Dispatcher;
                if (dispatcher == null)
                    ApplyFilter();
                else
                    dispatcher.InvokeAsync(ApplyFilter);
            }, token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
        }

        private void ApplyFilter()
        {
            var view = CollectionViewSource.GetDefaultView(Elements);

            var text = _searchText;
            if (string.IsNullOrWhiteSpace(text))
            {
                view.Filter = null;
                return;
            }

            var value = text!.Trim();
            var field = _selectedSearchField.Field;

            view.Filter = obj =>
            {
                if (obj is not ElementFromModel e) return false;

                return field switch
                {
                    SearchField.Id => Contains(e.Id.ToString(), value),
                    SearchField.Name => Contains(e.Name, value),
                    SearchField.Level => Contains(e.Level, value),
                    SearchField.CurrentValue => Contains(e.CurrentValue, value),
                    SearchField.NewValue
                        => Contains(e.Prefix, value)
                        || Contains(e.Level, value)
                        || Contains(e.Number, value),
                    _ => false
                };

                static bool Contains(string? src, string q) => !string.IsNullOrEmpty(src) && src!.Contains(q);
            };
        }

        private void OpenSettings(object? o)
        {
            _handlers.ChangeSettings(this);
        }
    }
}
