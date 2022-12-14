using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleDI
{

    public interface IFieldInjector
    {
        public Type Type { get; }
        public void Inject(object obj, object value);
    }

    public class InjectableField<T, TArg> : IFieldInjector
    {

        public Type Type { get; }
        public Func<T, TArg, TArg> SetValue;

        public InjectableField(Type fieldType, Func<T, TArg, TArg> setValue)
        {
            this.Type = fieldType;
            this.SetValue = setValue;
        }

        public void Inject(object obj, object value)
        {
            SetValue((T)obj, (TArg)value);
        }
    }

    public class ObjectInjector
    {
        private List<IFieldInjector> _fieldInjectors = new List<IFieldInjector>();
        
        private static Type s_injectAttributeType = typeof(InjectAttribute);
        private static Type s_injectableFieldType = typeof(InjectableField<,>);

        public ObjectInjector(Type type)
        {
            var fields = type.GetRuntimeFields().ToArray();

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var attr = field.GetCustomAttribute(s_injectAttributeType);

                if (attr != null)
                {
                    var setDelegate = DIUtils.CreateSetFieldFunction(type, field);

                    var injectorType = s_injectableFieldType.MakeGenericType(type, field.FieldType);

                    var fieldInjector = (IFieldInjector)Activator.CreateInstance(injectorType, new object[] { field.FieldType, setDelegate }); 
                    _fieldInjectors.Add(fieldInjector);
                }
            }
        }

        public void Inject(object obj, DIContainer container)
        {
            for (int i = 0; i < _fieldInjectors.Count; i++)
            {
                var injectable = _fieldInjectors[i];
                
                var instance = container.Resolve(injectable.Type);

                injectable.Inject(obj, instance);
            }
        }
    }
}