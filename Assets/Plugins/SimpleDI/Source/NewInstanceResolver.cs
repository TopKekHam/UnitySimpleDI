using System;

namespace SimpleDI
{
    public class NewInstanceResolver : IDIResolver
    {
        private Type instanceType;
        private DIContainer container;

        private Func<object> newMethod;

        public NewInstanceResolver(Type instanceType, DIContainer container)
        {
            this.instanceType = instanceType;
            this.container = container;

            BuildMethod();
        }

        void BuildMethod()
        {
            var constructor = DIUtils.GetInjectableConstructor(instanceType);

            if (constructor == null)
            {
                constructor = DIUtils.GetEmptyConstructor(instanceType);
            }

            if (constructor == null)
            {
                throw new Exception($"Injectable or empty constructor not found, couldn't not create instance of: {instanceType.Name}");
            }

            newMethod = DIUtils.CreateConstructorFucntion(constructor, container);
        }

        public object Resolve()
        {
            var obj = newMethod();

            container.InjectObjectFields(obj);

            return obj;
        }
    }
}