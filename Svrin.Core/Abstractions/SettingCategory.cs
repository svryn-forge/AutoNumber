using System.Text;

namespace Svrin.Core.Abstractions
{
    public class SettingCategory : BaseViewModel
    {
        private bool _isSelected;
        private string _prefix = string.Empty;
        private string _parameterName = string.Empty;
        private string _template = "{Prefix}-{Уровень}-{N:000}";
        private int _padding = 3;

        public bool IsSelected
        {
            get => _isSelected;
            set => OnPropertyChanged(ref _isSelected, value);
        }

        public long Id { get; set; } = -1L;
        public string Name { get; set; } = string.Empty;

        public string Prefix
        {
            get => _prefix;
            set => OnPropertyChanged(ref _prefix, value);
        }

        public string ParameterName
        {
            get => _parameterName;
            set => OnPropertyChanged(ref _parameterName, value);
        }

        public int Padding
        {
            get => _padding;
            set
            {
                if (value < 1)
                    value = 1;

                if (value > 9)
                    value = 9;

                if (!OnPropertyChanged(ref _padding, value))
                    return;

                StringBuilder sb = new StringBuilder("{Prefix}-{Уровень}-{N:");
                for (int i = 0; i < value; i++)
                    sb.Append('0');

                sb.Append('}');

                Template = sb.ToString();
            }
        }

        public string Template
        {
            get => _template;
            private set => OnPropertyChanged(ref _template, value);
        }

        public SettingCategory Clone() => new()
        {
            Id = Id,
            IsSelected = IsSelected,
            Name = Name,
            ParameterName = ParameterName,
            Prefix = Prefix,
            Padding = Padding
        };
    }
}
