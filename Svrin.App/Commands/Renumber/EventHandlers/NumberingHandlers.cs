using Svrin.Core.Abstractions;
using Svrin.Wpf.ViewModels;

namespace Svrin.App.Commands.Renumber.EventHandlers
{
    internal class NumberingHandlers : INumberingHandlers
    {
        private readonly LoadEventHandler _loadEventHandler;
        private readonly ApplyEventHandler _aplyEventHandler;
        private readonly ChangeSettingsHandler _changeSettings;
        public NumberingHandlers()
        {
            _loadEventHandler = new LoadEventHandler();
            _aplyEventHandler = new ApplyEventHandler();
            _changeSettings = new ChangeSettingsHandler();
        }

        public void Load(object obj)
        {
            if (obj is NumberingVM vm)
            {
                _loadEventHandler.Raise(vm);
            }
        }

        public void Apply(string? title, IReadOnlyCollection<ElementFromModel>? elements)
        {
            _aplyEventHandler.Raise(title, elements);
        }

        public void ChangeSettings(object obj)
        {
            if (obj is NumberingVM vm)
            {
                _changeSettings.Raise(vm);
            }
        }
    }
}
