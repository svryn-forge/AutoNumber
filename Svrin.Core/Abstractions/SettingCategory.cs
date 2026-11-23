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
            set => OnPropertyChanged(ref padding, value);
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
