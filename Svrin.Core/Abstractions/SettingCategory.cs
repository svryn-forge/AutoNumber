using System.Text;

namespace Svrin.Core.Abstractions
{
    public class SettingCategory : BaseViewModel
    {
        private bool isSelected;
        private string prefix = string.Empty;
        private string parameterName = string.Empty;
        private int padding = 3;

        public bool IsSelected
        {
            get => isSelected;
            set => OnPropertyChanged(ref isSelected, value);
        }

        public long Id { get; set; } = -1L;
        public string Name { get; set; } = string.Empty;

        public string Prefix
        {
            get => prefix;
            set => OnPropertyChanged(ref prefix, value);
        }

        public string ParameterName
        {
            get => parameterName;
            set => OnPropertyChanged(ref parameterName, value);
        }

        public int Padding
        {
            get => padding;
            set
            {
                if (value < 1)
                    value = 1;

                if (!OnPropertyChanged(ref padding, value))
                    return;

                StringBuilder sb = new StringBuilder("{Prefix}-{Уровень}-{N:");
                for (int i = 0; i < value; i++)
                    sb.Append('0');

                sb.Append('}');

                Template = sb.ToString();
            }
        }

        private string _template = "{Prefix}-{Уровень}-{N:000}";

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
