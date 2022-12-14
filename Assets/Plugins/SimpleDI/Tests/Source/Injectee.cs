using UnityEngine;
using SimpleDI;

namespace SimpleDI.Tests
{
    public class Injectee : MonoBehaviour
    {
        public string from = "Injectee";

        [Inject] private Injectable _injectable;
        [Inject] private InjectableInstance _injectableInstance;
        [Inject] private FunctionInjectable _functionInjectable;
        [Inject] private ConstructorInjectee _constructorInjectee;
        [Inject] private FactoryInjectee _factoryInjectee;

        void Start()
        {
            _injectable.Test(from);
            _injectableInstance.Test(from);
            _functionInjectable.Test(from);
            _constructorInjectee.Test(from);
            _factoryInjectee.Test(from);
        }

        void Update()
        {
        }
    }
}