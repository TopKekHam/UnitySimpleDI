using SimpleDI;
using UnityEngine;

namespace SimpleDI.Tests
{
    public class InjectableInstance
    {
        private static string injectable = "InjectableInstance";

        [Inject] private Injectable _injectable;

        public void Test(string from)
        {
            _injectable.Test(injectable);
            Debug.Log($"Injecting {injectable} to: {from}");
        }
    }
}