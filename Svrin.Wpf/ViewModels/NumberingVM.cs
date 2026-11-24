using Svrin.Core.Abstractions;
using Svrin.Core.Properties;
using Svrin.Wpf.Abstractions;
using Svrin.Wpf.Commands;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Svrin.Wpf.ViewModels
{
    public class NumberingVM : BaseViewModel
    {
        private readonly IWindowService _windowService;
        private readonly INumberingHandlers _handlers;
        private string? _searchText;
        private SearchFieldOption _selectedSearchField;
        private CancellationTokenSource? _searchCts;
        private bool _showTimeReport;
        private ElementNumberingSettings? _settings;

        public ICommand CloseCommand => new RelayCommand((_) => _windowService?.Close());
        public ICommand ApplyCommand => new RelayCommand((_) => _handlers.Apply(this));
        public ICommand LoadCommand => new RelayCommand((_) => _handlers.Load(this));
        public ICommand OpenSettingsCommand => new RelayCommand(OpenSettings);
        public IReadOnlyList<SearchFieldOption> SearchFields { get; }

        public string? CurrentDocument { get; set; }
        public bool ShowTimeReport { get => _showTimeReport; set => OnPropertyChanged(ref _showTimeReport, value); }
        public ElementNumberingSettings? Settings
        {
            get => _settings;
            set
            {
                if (value == null)
                {
                    Elements.Clear();
                }

                if (!OnPropertyChanged(ref _settings, value))
                    return;


                for (int i = 0; i < Elements.Count; i++)
                {
                    ElementFromModel? e = Elements[i];
                    var s = _settings.CategorySet.FirstOrDefault(x => x.Id == e.CategoryId);
                    if (s == null)
                    {
                        Elements.Remove(e);
                        i--;
                        continue;
                    }

                    e.Settings = s;
                }
            }
        }
        public ObservableCollection<ElementFromModel> Elements { get; } = [];

        public SearchFieldOption SelectedSearchField
        {
            get => _selectedSearchField;
            set
            {
                if (OnPropertyChanged(ref _selectedSearchField, value))
                    ApplyFilter();
            }
        }
        
        public string? SearchText
        {
            get => _searchText;
            set
            {
                if (OnPropertyChanged(ref _searchText, value))
                {
                    bool hasErrors = false;
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    try
                    {
                        ScheduleFilter();
                    }
                    catch
                    {
                        hasErrors = true;
                    }
                    finally
                    {
                        stopwatch.Stop();
                        if (!hasErrors && ShowTimeReport)
                        {
                            MessageBox.Show($"{Resources.TimeResult}: {stopwatch.Elapsed}", Resources.Report);
                        }
                    }
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
                    SearchField.NewValue => Contains(e.NewValue, value),
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
