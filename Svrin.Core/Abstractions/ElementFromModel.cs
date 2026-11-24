using Svrin.Core.Helpers;
using System.Collections;
using System.ComponentModel;
using System.Text;

namespace Svrin.Core.Abstractions
{
    public class ElementFromModel : BaseViewModel, INotifyDataErrorInfo
    {
        private readonly StringBuilder _stringBuilder = new();
        private string _newValue = string.Empty;
        private bool _isChecked;
        private int _number;
        private string _currentValue = string.Empty;
        private SettingCategory? _settings;

        public bool IsChecked { get => _isChecked; set => OnPropertyChanged(ref _isChecked, value); }
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string UniqueId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public double LevelElevation { get; set; }

        public SettingCategory? Settings
        {
            get => _settings;
            set
            {
                OnPropertyChanged(ref _settings, value);
                if (_settings == null)
                    return;

                UpdateNewValue();
            }
        }

        public int Number
        {
            get => _number;
            set => OnPropertyChanged(ref _number, value);
        }

        public string CurrentValue { get => _currentValue; set => OnPropertyChanged(ref _currentValue, value); }
        public string NewValue
        {
            get => _newValue;
            set
            {
                if (!OnPropertyChanged(ref _newValue, value))
                    return;

                OnPropertyChanged(nameof(NewValueError));
                OnPropertyChanged(nameof(HasErrors));

                ErrorsChanged?.Invoke(
                    this,
                    new DataErrorsChangedEventArgs(nameof(NewValue)));
            }
        }

        public bool IsEquals => string.Equals(CurrentValue, NewValue, StringComparison.InvariantCultureIgnoreCase);

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public bool HasErrors => !string.IsNullOrEmpty(NewValueError);
        public string? NewValueError => _settings == null || string.IsNullOrWhiteSpace(_settings.Template)
            ? null
            : TemplateFormatter.ValidateValue(_settings.Template!, NewValue, _settings.Prefix, Level);

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable GetErrors(string? propertyName)
        {
            return propertyName switch
            {
                nameof(NewValue) when NewValueError is not null
                    => new[] { NewValueError },

                _ => Array.Empty<string>()
            };
        }

        private void UpdateNewValue()
        {
            if (_settings == null)
                return;

            _stringBuilder.Clear();

            AddValue(_settings.Prefix);
            AddSeparator();
            AddValue(Level);
            AddSeparator();

            var format = NumberFormat.FromPadding(_settings.Padding);
            AddValue(Number.ToString(format));
            
            NewValue = _stringBuilder.ToString();

            void AddValue(string value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _stringBuilder.Append(value);
            }

            void AddSeparator()
            {
                if (_stringBuilder.Length > 0)
                    _stringBuilder.Append('-');
            }
        }

        // Сделал для проверки значений при отладке
        public override string ToString() => $"{Id}_{X}_{Y}_{Z}_{Level}";
    }
}
