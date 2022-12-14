using System;

namespace SimpleDI
{
    public class NewInstanceResolver : IDIResolver
    {
        private Type _instanceType;
        private DIContainer _container;
        private Func<object> _newMethod;

        public NewInstanceResolver(Type instanceType, DIContainer container)
        {
            this._instanceType = instanceType;
            this._container = container;

            BuildMethod();
        }

        void BuildMethod()
        {
            var constructor = DIUtils.GetInjectableConstructor(_instanceType);

            if (constructor == null)
            {
                constructor = DIUtils.GetEmptyConstructor(_instanceType);
            }

            if (constructor == null)
            {
                throw new Exception($"Injectable or empty constructor not found, couldn't not create instance of: {_instanceType.Name}");
            }

            _newMethod = DIUtils.CreateConstructorFucntion(constructor, _container);
        }

        public object Resolve()
        {
            var obj = _newMethod();

            _container.InjectObjectFields(obj);

            return obj;
        }
    }
}