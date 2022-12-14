using UnityEngine;

namespace SimpleDI.Tests
{
    class FactoryInstanceInjectable
    {
        static string name = "FactoryInstanceInjectable";

        public FactoryInstanceInjectable()
        {
        }

        public FactoryInstanceInjectable(Injectable _injectable)
        {
            _injectable.Test(name);
        }

        public void Test(string from)
        {
            Debug.Log($"Injecting {name} to: {from}");
        }
    }
}