using SimpleDI;
using System;
using UnityEngine;

namespace SimpleDI.Tests
{
    class FactoryInjectee
    {
        [Inject] Func<Injectable, FactoryInstanceInjectable> newInjectee;
        [Inject] Injectable injectable;

        static string name = "FactoryInjectee";

        public void Test(string from)
        {
            FactoryInstanceInjectable instance = newInjectee(injectable);
            instance.Test(name);

            Debug.Log($"Injecting {name} to: {from}");
        }
    }
}