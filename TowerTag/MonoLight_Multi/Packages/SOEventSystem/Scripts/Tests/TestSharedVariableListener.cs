using System;
using SOEventSystem.Listeners;
using UnityEngine;
using UnityEngine.Events;

namespace SOEventSystem.Tests {
    public class
        TestSharedVariableListener : SharedVariableListener<TestClass, TestSharedVariable, TestResponse> {
        [SerializeField] private string test;
    }

    [Serializable]
    public class TestResponse : UnityEvent<object, TestClass> { }
}