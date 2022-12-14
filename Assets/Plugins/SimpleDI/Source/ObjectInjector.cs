using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleDI
{

    public interface IFieldInjector
    {
        public Type type { get; }
        public void Inject(object obj, object value);
    }

    public class InjectableField<T, TArg> : IFieldInjector
    {

        public Type type { get; }
        public Func<T, TArg, TArg> setValue;

        public InjectableField(Type fieldType, Func<T, TArg, TArg> setValue)
        {
            this.type = fieldType;
            this.setValue = setValue;
        }

        public void Inject(object obj, object value)
        {
            setValue((T)obj, (TArg)value);
        }
    }

    public class ObjectInjector
    {
        private List<IFieldInjector> fieldsToInject = new List<IFieldInjector>();

        private static Type InjectAttributeType = typeof(InjectAttribute);
        private static Type InjectableFieldType = typeof(InjectableField<,>);

        public ObjectInjector(Type type)
        {
            var fields = type.GetRuntimeFields().ToArray();

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var attr = field.GetCustomAttribute(InjectAttributeType);

                if (attr != null)
                {
                    var setDelegate = DIUtils.CreateSetFieldFunction(type, field);

                    var injectorType = InjectableFieldType.MakeGenericType(type, field.FieldType);

                    var fieldInjector = (IFieldInjector)Activator.CreateInstance(injectorType, new object[] { field.FieldType, setDelegate }); 
                    fieldsToInject.Add(fieldInjector);
                }
            }
        }

        public void Inject(object obj, DIContainer container)
        {
            for (int i = 0; i < fieldsToInject.Count; i++)
            {
                var injectable = fieldsToInject[i];
                
                var instance = container.Resolve(injectable.type);

                injectable.Inject(obj, instance);
            }
        }
    }
}