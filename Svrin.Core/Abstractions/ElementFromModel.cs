using Svrin.Core.Helpers;
using System.Text;

namespace Svrin.Core.Abstractions
{
    public class ElementFromModel : BaseViewModel
    {
        private readonly StringBuilder _stringBuilder = new();
        private bool isChecked;
        private string prefix = string.Empty;
        private string number = string.Empty;
        private string template = "Д-{Уровень}-{N:000}";
        public string Template
        {
            get => template;
            set
            {
                if (!OnPropertyChanged(ref template, value)) return;
                OnPropertyChanged(nameof(NewValue));
                OnPropertyChanged(nameof(TemplateError));
            }
        }
        public string? TemplateError => TemplateFormatter.ValidateTemplate(Template, Level, Number, Prefix);

        public bool IsChecked { get => isChecked; set => OnPropertyChanged(ref isChecked, value); }
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string UniqueId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public double LevelElevation { get; set; }
        public string Prefix
        {
            get => prefix;
            set
            {
                if (!OnPropertyChanged(ref prefix, value))
                    return;
                UpdateNewValue();
            }
        }
        public string Number
        {
            get => number;
            set
            {
                if (!OnPropertyChanged(ref number, value))
                    return;
                UpdateNewValue();
            }
        }

        public string CurrentValue { get; set; } = string.Empty;

        private string? _newValueCache;
        public string NewValue
        {
            get
            {
                if (_newValueCache != null) return _newValueCache;

                _stringBuilder.Clear();

                AddValue(Prefix);
                AddSeparator();
                AddValue(Level);
                AddSeparator();
                AddValue(Number);

                return _newValueCache = _stringBuilder.ToString();

                void AddValue(string value)
                {
                    if (string.IsNullOrWhiteSpace(value)) return;
                    _stringBuilder.Append(value);
                }
                void AddSeparator()
                {
                    if (_stringBuilder.Length > 0)
                        _stringBuilder.Append('-');
                }
            }
        }

        private void UpdateNewValue()
        {
            _newValueCache = null;
            OnPropertyChanged(nameof(NewValue));
        }

        public bool IsEquals => string.Equals(CurrentValue, NewValue, StringComparison.InvariantCultureIgnoreCase);

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public string Parameter { get; set; } = string.Empty;

        public override string ToString() => $"{Id}_{X}_{Y}_{Z}_{Level}";
    }
}
