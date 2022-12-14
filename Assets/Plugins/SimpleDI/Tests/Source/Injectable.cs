using UnityEngine;

namespace SimpleDI.Tests
{
    public class Injectable : MonoBehaviour
    {
        public string injectable = "Injectable";

        public void Test(string from)
        {
            Debug.Log($"Injecting {injectable} to: {from}");
        }
    }
}