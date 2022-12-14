using SimpleDI;
using UnityEngine;

namespace SimpleDI.Tests
{
    public class TestDIContainer : DIContainer
    {
        public Injectable sceneRef;

        void Awake()
        {
            BindSingleton<DIContainer>(this);
            BindSingleton(sceneRef);
            Bind<InjectableInstance>();
            BindFunction<FunctionInjectable>(() => InjectObjectFields(new FunctionInjectable()));
            Bind<ConstructorInjectee>();
            BindSingleton<FactoryInjectee>();

            // CheckCircularDependencies test 

            {
                Bind<TestC1>();
                Bind<TestC2>();
                Bind<TestC3>();
                Bind<TestCircularDependecy>();
            }

            //ContainerCircularDependencyChecker checker = new ContainerCircularDependencyChecker();

            InjectActiveScene();
        }
    }
}