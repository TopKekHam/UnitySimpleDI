using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace SimpleDI
{
    public static class DIUtils
    {
        static Type[] s_genericFuncs = new Type[]
        {
            typeof(Func<>),
            typeof(Func<,>),
            typeof(Func<,,>),
            typeof(Func<,,,>),
            typeof(Func<,,,,>),
            typeof(Func<,,,,,>),
            typeof(Func<,,,,,,>),
            typeof(Func<,,,,,,,>),
            typeof(Func<,,,,,,,,>),
            typeof(Func<,,,,,,,,,>),
        };

        public static MethodCallExpression CreateCallExpression(object instance, string methodName,
            params Type[] paramTypes)
        {
            var type = instance.GetType();

            var method = type.GetRuntimeMethod(methodName, paramTypes);

            return Expression.Call(Expression.Constant(instance), method);
        }

        public static MethodCallExpression CreateGenericCallExpression<T>(object instance, string methodName,
            params Type[] paramTypes)
        {
            var type = instance.GetType();

            var method = type.GetRuntimeMethod(methodName, paramTypes);

            var methodInstance = method.GetGenericMethodDefinition().MakeGenericMethod(typeof(T));

            return Expression.Call(Expression.Constant(instance), methodInstance);
        }

        public static MethodCallExpression CreateGenericCallExpression(object instance, string methodName,
            Type genericType, params Type[] paramTypes)
        {
            var type = instance.GetType();

            var method = type.GetRuntimeMethod(methodName, paramTypes);

            var methodInstance = method.GetGenericMethodDefinition().MakeGenericMethod(genericType);

            return Expression.Call(Expression.Constant(instance), methodInstance);
        }

        public static ConstructorInfo? GetInjectableConstructor(this Type instanceType)
        {
            var constractors = instanceType.GetConstructors();

            for (int i = 0; i < constractors.Length; i++)
            {
                var attrs = constractors[i].GetCustomAttributes(true);

                for (int ai = 0; ai < attrs.Length; ai++)
                {
                    if (attrs[ai] is InjectAttribute attr)
                    {
                        return constractors[i];
                    }
                }
            }

            return null;
        }

        public static ConstructorInfo? GetEmptyConstructor(this Type instanceType)
        {
            return instanceType.GetConstructor(Type.EmptyTypes);
        }

        public static Func<object> CreateConstructorFucntion(ConstructorInfo constructor, DIContainer container)
        {
            var paramInfo = constructor.GetParameters();
            Expression[] exParams = new Expression[paramInfo.Length];

            for (int i = 0; i < exParams.Length; i++)
            {
                var resolveType = paramInfo[i].ParameterType;
                exParams[i] = DIUtils.CreateGenericCallExpression(container, "Resolve", resolveType);
            }

            NewExpression constructorExpression = Expression.New(constructor, exParams);

            Expression<Func<object>> ex = Expression.Lambda<Func<object>>(constructorExpression);

            return ex.Compile();
        }

        public static Type CreateGenericFuncType(Type returnType, params Type[] args)
        {
            Type[] allTypes = new Type[args.Length + 1];

            Array.Copy(args, allTypes, args.Length);
            allTypes[^1] = returnType;

            return s_genericFuncs[allTypes.Length - 1].MakeGenericType(allTypes);
        }

        public static Type CreateGenericFuncType(params Type[] types)
        {
            return s_genericFuncs[types.Length].MakeGenericType(types);
        }

        public static Delegate CreateConstructorFunction(Type resolveType, Type[] constructorParams,
            ConstructorInfo constructor, DIContainer container)
        {
            // function parameters
            ParameterExpression[] funcParams = new ParameterExpression[constructorParams.Length];

            for (int i = 0; i < constructorParams.Length; i++)
            {
                funcParams[i] = Expression.Parameter(constructorParams[i]);
            }

            string methodName = "InjectObjectFields";

            // create instance and assign to temp variable
            var body = Expression.New(constructor, funcParams);
            var instanceVariable = Expression.Variable(resolveType, "instance");
            var assign = Expression.Assign(instanceVariable, body);

            // inject
            var injectMethod = container.GetType().GetMethod(methodName);
            var resolveCall = Expression.Call(Expression.Constant(container), injectMethod, instanceVariable);

            // create block to contain the exprs.
            var block = Expression.Block(
                new ParameterExpression[] { instanceVariable },
                assign, resolveCall, instanceVariable);

            var funcExpr = Expression.Lambda(block, funcParams);

            var func = funcExpr.Compile();
            return func;
        }

        public static Delegate CreateSetFieldFunction(Type objType, FieldInfo info)
        {
            //params
            var paramObj = Expression.Parameter(objType);
            var paramValue = Expression.Parameter(info.FieldType);

            var field = Expression.Field(paramObj, info);

            var assign = Expression.Assign(field, paramValue);

            var block = Expression.Block(assign);

            var lambda = Expression.Lambda(block, new ParameterExpression[] { paramObj, paramValue });

            var func = lambda.Compile();

            return func;
        }
        
    }
}