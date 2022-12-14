using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SimpleDI
{

    [DefaultExecutionOrder(-99999)]
    public class DIContainer : MonoBehaviour
    {
        public static readonly Type s_injectAttributeType = typeof(InjectAttribute);

        public bool CreateConstructorBindingsWhileResolving = true;
        public bool CheckCircularDependenciesAtRuntime = true;
        
        private Dictionary<Type, IDIResolver> _instanceMap = new Dictionary<Type, IDIResolver>();
        private Dictionary<Type, ObjectInjector> _injectors = new Dictionary<Type, ObjectInjector>();
        private Stack<Type> _typeCheckStack = new Stack<Type>();
        
        // Injection =============================

        /// <summary>
        /// Applies DI to all gameObject in active scene.
        /// </summary>
        public void InjectActiveScene()
        {
            var scene = SceneManager.GetActiveScene();
            InjectScene(scene);
        }

        /// <summary>
        /// Applies DI to all gameObject in a scene.
        /// </summary>
        public void InjectScene(Scene scene)
        {
            var game_objects = scene.GetRootGameObjects();

            for (int i = 0; i < game_objects.Length; i++)
            {
                InjectGameObject(game_objects[i]);
            }
        }

        /// <summary>
        /// Applies DI to all gameObject and its children.
        /// </summary>
        public void InjectGameObject(GameObject gameObject)
        {
            var components = gameObject.GetComponents<MonoBehaviour>();

            for (int i = 0; i < components.Length; i++)
            {
                InjectObjectFields(components[i]);
            }

            var childCount = gameObject.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                var child = gameObject.transform.GetChild(i);

                InjectGameObject(child.gameObject);
            }
        }

        /// <summary>
        /// Applies DI to an object.
        /// </summary>
        public object InjectObjectFields(object obj)
        {
            var type = obj.GetType();
            
            ObjectInjector injector;

            if (_injectors.TryGetValue(type, out injector) == false)
            {
                injector = new ObjectInjector(type);
                _injectors.Add(type, injector);
            }

            injector.Inject(obj, this);
            
            return obj;
        }

        // Resolve =============================

        /// <summary>
        /// Resolves type.
        /// </summary>
        /// <exception cref="Exception">If type not bound throws exception.</exception>
        public T Resolve<T>()
        {
            var type = typeof(T);
            return (T)Resolve(type);
        }

        /// <summary>
        /// Resolves type.
        /// </summary>
        /// <exception cref="Exception">If type not bound throws exception.</exception>
        public object Resolve(Type type)
        {
            if (_instanceMap.TryGetValue(type, out var resolver))
            {
                if(CheckCircularDependenciesAtRuntime) PushTypeCheck(type);

                var instance = resolver.Resolve();

                if(CheckCircularDependenciesAtRuntime) PopTypeCheck(type);

                return instance;
            }
            else
            {
                if (CreateConstructorBindingsWhileResolving)
                {
                    if (type.BaseType == typeof(MulticastDelegate) || type.BaseType == typeof(Delegate))
                    {
                        var args = type.GetGenericArguments();

                        BindConstructor(args[^1], args[0..^1]);

                        return Resolve(type);
                    }
                }

                if (type.IsGenericType)
                {
                    var args = type.GetGenericArguments().Select(t => t.Name).Aggregate((src, arg) => src + ", " + arg);
                    throw new Exception($"No binding found for type: {type.Name}<{args}>");
                }
                else
                {
                    throw new Exception($"No binding found for type: {type.Name}");
                }
            }
        }

        // Circular dependency check ===========

        void PushTypeCheck(Type type)
        {

            Type prevType = null;
            _typeCheckStack.TryPeek(out prevType);
            
            foreach (var sType in _typeCheckStack)
            {
                if (TypesEqual(sType, type))
                {
                    StringBuilder builder = new StringBuilder();
                    
                    foreach (var ssType in _typeCheckStack)
                    {
                        builder.Append($"-> {ssType.Name}");
                    }
                    
                    builder.Append($" ->");
                    
                    throw new Exception($"[Circular Dependency] {builder.ToString()}");
                }
                
                prevType = sType;
            }

            _typeCheckStack.Push(type);
        }

        bool TypesEqual(Type type, Type type2)
        {
            return type.IsSubclassOf(type2) || type2.IsSubclassOf(type) || type == type2;
        }

        void PopTypeCheck(Type type)
        {
            var popType = _typeCheckStack.Pop();

            if (popType != type) throw new Exception("Popped wrong type! (how this is even possible???????)");
        }

        // Binding =============================

        public void BindResolver(Type type, IDIResolver resolver)
        {
            if (_instanceMap.TryAdd(type, resolver) == false)
            {
                throw new Exception($"Resolver already binded for type: {type.Name}");
            }
        }

        public void BindResolver<T>(IDIResolver resolver)
        {
            var type = typeof(T);
            BindResolver(type, resolver);
        }

        public void BindFunction(Type typeToResolve, Func<object> func)
        {
            BindResolver(typeToResolve, new FunctionResolver(func));
        }

        public void BindFunction<T>(Func<object> func)
        {
            var type = typeof(T);
            BindResolver(type, new FunctionResolver(func));
        }
        
        public void BindSingleton(Type type, object instance, bool injected = false)
        {
            BindResolver(type, new SingletonResolver(instance, this, injected));
        }
        
        public void BindSingleton<T>(T instance, bool injected = false)
        {
            var type = typeof(T);
            BindResolver(type, new SingletonResolver(instance, this, injected));
        }
        
        public void BindSingleton<T>()
        {
            var type = typeof(T);
            BindResolver(type, new SingletonResolver(type, this));
        }

        public void BindSingleton<TInterface, TConcrete>() where TConcrete : TInterface
        {
            BindResolver<TInterface>(new SingletonResolver(typeof(TConcrete), this));
        }
        
        public void BindConstructor<TConstructor, TArg1>()
        {
            BindConstructor(typeof(TConstructor), typeof(TArg1));
        }
        
        public void BindConstructor<TConstructor, TArg1, TArg2>()
        {
            BindConstructor(typeof(TConstructor), typeof(TArg1), typeof(TArg2));
        }
        
        public void BindConstructor<TConstructor, TArg1, TArg2, TArg3>()
        {
            BindConstructor(typeof(TConstructor), typeof(TArg1), typeof(TArg2), typeof(TArg3));
        }
        
        public void BindConstructor<TConstructor>()
        {
            BindConstructor(typeof(TConstructor));
        }

        void BindConstructor(Type resolveType, params Type[] constructorParams)
        {
            var constructor = resolveType.GetConstructor(constructorParams);

            if (constructor == null)
            {
                throw new Exception(
                    $"Could not find constructor for type: {resolveType.Name} wiht parameters: {constructorParams.Select(t => t.Name)}");
            }

            Delegate func = DIUtils.CreateConstructorFunction(resolveType, constructorParams, constructor, this);

            Type funcType = DIUtils.CreateGenericFuncType(resolveType, constructorParams);

            BindSingleton(funcType, func, true);
        }

        public void Bind<TypeToResolve>()
        {
            Bind<TypeToResolve, TypeToResolve>();
        }

        public void Bind<TypeToResolve, TypeToBind>()
        {
            Type toResolve = typeof(TypeToResolve);
            Type toBind = typeof(TypeToBind);

            BindResolver(toResolve, new NewInstanceResolver(toBind, this));
        }

        // Instantiation =============================

        /// <summary>
        /// create new instance of T with DI.
        /// </summary>
        public T Instantiate<T>()
        {
            var obj = (T)Activator.CreateInstance(typeof(T));
            InjectObjectFields(obj);
            return obj;
        }

        /// <summary>
        /// Instantiates the gameObject of the component passed with DI. 
        /// </summary>
        public void Instantiate<T>(T component, Transform parent, out T instance, out GameObject obj)
            where T : MonoBehaviour
        {
            obj = GameObject.Instantiate(component.gameObject, parent);
            InjectGameObject(obj);
            instance = obj.GetComponent<T>();
        }

        /// <summary>
        /// Instantiates the gameObject of the component passed with DI. 
        /// </summary>
        public void Instantiate<T>(T component, Transform parent, out T instance) where T : MonoBehaviour
        {
            var obj = GameObject.Instantiate(component.gameObject, parent);
            InjectGameObject(obj);
            instance = obj.GetComponent<T>();
        }

        /// <summary>
        /// Instantiates the gameObject of the component passed with DI. 
        /// </summary>
        public void Instantiate<T>(T component, Transform parent, out GameObject obj) where T : MonoBehaviour
        {
            obj = GameObject.Instantiate(component.gameObject, parent);
            InjectGameObject(obj);
        }

        /// <summary>
        /// Instantiates the gameObject of the component passed with DI. 
        /// </summary>
        public T Instantiate<T>(T component, Transform parent) where T : MonoBehaviour
        {
            var obj = GameObject.Instantiate(component.gameObject, parent);
            InjectGameObject(obj);
            return obj.GetComponent<T>();
        }

        /// <summary>
        /// Instantiates the gameObject with DI. 
        /// </summary>
        public GameObject Instantiate(GameObject gameObject, Transform parent)
        {
            var obj = GameObject.Instantiate(gameObject, parent);
            InjectGameObject(obj);
            return obj;
        }

        /// <summary>
        /// Instantiates the gameObject with DI. 
        /// </summary>
        public GameObject Instantiate(GameObject gameObject)
        {
            var obj = GameObject.Instantiate(gameObject);
            InjectGameObject(obj);
            return obj;
        }
    }
}