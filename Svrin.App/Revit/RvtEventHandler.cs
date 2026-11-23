using Autodesk.Revit.UI;

namespace Svrin.App.Revit
{
    public abstract class RvtEventHandler<T1> : IExternalEventHandler
    {
        private readonly object _locker;

        private T1? _argsT1;

        private readonly ExternalEvent _revitEvent;

        public RvtEventHandler()
        {
            _revitEvent = ExternalEvent.Create(this);
            _locker = new object();
        }

        public void Execute(UIApplication app)
        {
            T1? argsT1;
            lock (_locker)
            {
                argsT1 = _argsT1;
                _argsT1 = default;
            }
            Execute(app, argsT1);
        }

        public void Raise(T1 argsT1)
        {
            lock (_locker)
            {
                _argsT1 = argsT1;
            }
            _revitEvent.Raise();
        }
        public string GetName() => nameof(RvtEventHandler<>);

        public abstract void Execute(UIApplication app, T1? argsT1);
    }

    public abstract class RvtEventHandler<T1, T2> : IExternalEventHandler
    {
        private readonly object _locker;

        private T1? _argsT1;
        private T2? _argsT2;

        private readonly ExternalEvent _revitEvent;

        public RvtEventHandler()
        {
            _revitEvent = ExternalEvent.Create(this);
            _locker = new object();
        }

        public void Execute(UIApplication app)
        {
            T1? argsT1;
            T2? argsT2;
            lock (_locker)
            {
                argsT1 = _argsT1;
                _argsT1 = default;
                argsT2 = _argsT2;
                _argsT2 = default;
            }
            Execute(app, argsT1, argsT2);
        }

        public void Raise(T1? argsT1, T2? argsT2)
        {
            lock (_locker)
            {
                _argsT1 = argsT1;
                _argsT2 = argsT2;
            }
            _revitEvent.Raise();
        }

        public string GetName() => nameof(RvtEventHandler<,>);

        public abstract void Execute(UIApplication app, T1? argsT1, T2? argsT2);
    }
}
