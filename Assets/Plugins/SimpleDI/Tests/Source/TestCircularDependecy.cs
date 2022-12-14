using SimpleDI;
using UnityEngine;

namespace SimpleDI.Tests
{
    class TestC1
    {
        [Inject] private TestC2 _testC2;
    }

    class TestC2
    {
        [Inject] private TestC3 _testC3;
    }

    class TestC3
    {
        [Inject] private TestC1 _testC1;
    }

    public class TestCircularDependecy : MonoBehaviour
    {
        [Inject] private TestC1 _testC1;
    }
}