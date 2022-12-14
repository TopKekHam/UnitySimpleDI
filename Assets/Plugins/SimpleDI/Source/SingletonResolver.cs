using System;

namespace SimpleDI
{
    public class SingletonResolver : IDIResolver
    {
        private object _instance;
        private bool _injected;
        private DIContainer _container;
        private NewInstanceResolver _resolver;
        
        public SingletonResolver(Type type, DIContainer container)
        {
            _resolver = new NewInstanceResolver(type, container);
            _injected = false;
        }

        public SingletonResolver(object instance, DIContainer container, bool injected = false)
        {
            this._instance = instance;
            Load(container, injected);
        }

        void Load(DIContainer container, bool injected)
        {
            this._container = container;
            this._injected = injected;
        }

        public object Resolve()
        {
            if (_instance == null)
            {
                _instance = _resolver.Resolve();
                _injected = true;
            }
            
            if(_injected == false)
            {
                _container.InjectObjectFields(_instance);
                _injected = true;
            }

            return _instance;
        }
    }
}
