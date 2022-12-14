using System.Collections;
using System.Collections.Generic;
using SimpleDI;
using UnityEngine;

namespace SimpleDI.Tests
{
    public class PrefabInjectee : MonoBehaviour
    {
        public GameObject injecteePrefab;

        [Inject] private DIContainer container;

        void Start()
        {
            container.Instantiate(injecteePrefab);
        }
    }
}