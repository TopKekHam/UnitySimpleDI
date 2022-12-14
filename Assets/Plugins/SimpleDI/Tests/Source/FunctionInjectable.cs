using SimpleDI;
using UnityEngine;

namespace SimpleDI.Tests
{
    public class FunctionInjectable
    {
        private static string injectable = "FunctionInjectable";

        [Inject] private Injectable _injectable;
        [Inject] private InjectableInstance _injectableInstance;

        public void Test(string from)
        {
            _injectable.Test(injectable);
            _injectableInstance.Test(injectable);
            Debug.Log($"Injecting {injectable} to: {from}");
        }
    }
}