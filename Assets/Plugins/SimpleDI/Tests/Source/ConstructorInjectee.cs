using SimpleDI;
using UnityEngine;

namespace SimpleDI.Tests
{
    class ConstructorInjectee
    {
        [Inject] FunctionInjectable _functionInjectable;

        static string injectable = "ConstructorInjectee";

        [Inject]
        public ConstructorInjectee(Injectable _injectable)
        {
            _injectable.Test(injectable);
        }

        public void Test(string from)
        {
            _functionInjectable.Test(injectable);
            Debug.Log($"Injecting {injectable} to: {from}");
        }
    }
}