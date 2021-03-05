using System;
using System.Collections;
using NUnit.Framework;
using SOEventSystem.Listeners;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace SOEventSystem.Tests {
    [Serializable]
    public class SharedVariableListenerTest {
        [UnityTest]
        public IEnumerator ShouldNotRespondAtAll() {
            // GIVEN a TestSharedVariableEventListener for a TestSharedVariable reacting never
            var testSharedVariableListener = new GameObject("Listener").AddComponent<TestSharedVariableListener>();
            var testSharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            testSharedVariableListener.SharedVariable = testSharedVariable;
            testSharedVariableListener.Policy = SharedVariableListener.ListenerPolicy.ValueSet;
            testSharedVariableListener.enabled = false;
            var oldValue = new TestClass();
            testSharedVariable.Set(this, oldValue);
            // with a UnityEvent as response that registers whether it was invoked with certain parameters
            var newValue = new TestClass();
            int invocations = 0;
            testSharedVariableListener.Response.AddListener((sender, value) => { invocations++; });

            // WHEN changing the value of the SharedVariable
            testSharedVariable.Set(this, newValue);
            yield return null;

            // THEN there should be no response
            Assert.AreEqual(0, invocations, "Should not have responded");
        }

        [UnityTest]
        public IEnumerator ShouldRespondToChanges() {
            // GIVEN a TestSharedVariableEventListener for a TestSharedVariable reacting to changes
            var testSharedVariableListener = new GameObject("Listener").AddComponent<TestSharedVariableListener>();
            var testSharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            testSharedVariableListener.SharedVariable = testSharedVariable;
            testSharedVariableListener.Policy = SharedVariableListener.ListenerPolicy.ValueChanged;
            var oldValue = new TestClass();
            testSharedVariable.Set(this, oldValue);
            // with a UnityEvent as response that registers whether it was invoked with certain parameters
            var newValue = new TestClass();
            int invocations = 0;
            testSharedVariableListener.Response.AddListener((sender, value) => {
                Assert.AreEqual(this, sender);
                Assert.AreEqual(newValue, value);
                invocations++;
            });

            // WHEN changing the value of the SharedVariable
            testSharedVariable.Set(this, newValue);
            yield return null;

            // THEN the response should have been processed
            Assert.AreEqual(1, invocations, "Should have responded once");
        }

        [UnityTest]
        public IEnumerator ShouldNotRespondToChangesOfPreviouslyListenedToVariable() {
            // GIVEN a TestSharedVariableEventListener for a TestSharedVariable reacting to changes
            var testSharedVariableListener = new GameObject("Listener").AddComponent<TestSharedVariableListener>();
            var testSharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            testSharedVariableListener.SharedVariable = testSharedVariable;
            testSharedVariableListener.Policy = SharedVariableListener.ListenerPolicy.ValueChanged;
            var oldValue = new TestClass();
            testSharedVariable.Set(this, oldValue);
            // with a UnityEvent as response that registers whether it was invoked with certain parameters
            var newValue = new TestClass();
            int invocations = 0;
            testSharedVariableListener.Response.AddListener((sender, value) => {
                Assert.AreEqual(this, sender);
                Assert.AreEqual(newValue, value);
                invocations++;
            });

            // WHEN changing the variable that is listened to and changing the old one
            testSharedVariableListener.SharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            testSharedVariable.Set(this, newValue);
            yield return null;

            // THEN the response should not have been processed
            Assert.AreEqual(0, invocations, "Should not have responded");
        }

        [UnityTest]
        public IEnumerator ShouldRespondToChangesWhenListeningToSet() {
            // GIVEN a TestSharedVariableEventListener for a TestSharedVariable reacting to all calls to set
            var testSharedVariableListener = new GameObject("Listener").AddComponent<TestSharedVariableListener>();
            var testSharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            testSharedVariableListener.SharedVariable = testSharedVariable;
            testSharedVariableListener.Policy = SharedVariableListener.ListenerPolicy.ValueSet;
            var oldValue = new TestClass();
            testSharedVariable.Set(this, oldValue);
            // with a UnityEvent as response that registers whether it was invoked with certain parameters
            var newValue = new TestClass();
            int invocations = 0;
            testSharedVariableListener.Response.AddListener((sender, value) => {
                Assert.AreEqual(this, sender);
                Assert.AreEqual(newValue, value);
                invocations++;
            });

            // WHEN setting the value of the SharedVariable
            testSharedVariable.Set(this, newValue);
            yield return null;

            // THEN the response should have been processed
            Assert.AreEqual(1, invocations, "Should have responded once");
        }

        [UnityTest]
        public IEnumerator ShouldNotRespondToSetUnchangedWhenListeningToChanges() {
            // GIVEN a TestSharedVariableEventListener for a TestSharedVariable reacting to changes
            var testSharedVariableListener = new GameObject("Listener").AddComponent<TestSharedVariableListener>();
            var testSharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            testSharedVariableListener.SharedVariable = testSharedVariable;
            testSharedVariableListener.Policy = SharedVariableListener.ListenerPolicy.ValueChanged;
            var oldValue = new TestClass();
            testSharedVariable.Set(this, oldValue);
            // with a UnityEvent as response that registers whether it was invoked with certain parameters
            int invocations = 0;
            testSharedVariableListener.Response.AddListener((sender, value) => { invocations++; });

            // WHEN setting the value of the SharedVariable
            testSharedVariable.Set(this, oldValue);
            yield return null;

            // THEN there should be no response
            Assert.AreEqual(0, invocations, "Should not have responded");
        }

        [UnityTest]
        public IEnumerator ShouldRespondToSet() {
            // GIVEN a TestSharedVariableEventListener for a TestSharedVariable reacting to all calls to set
            var testSharedVariableListener = new GameObject("Listener").AddComponent<TestSharedVariableListener>();
            var testSharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            testSharedVariableListener.SharedVariable = testSharedVariable;
            testSharedVariableListener.Policy = SharedVariableListener.ListenerPolicy.ValueSet;
            var oldValue = new TestClass();
            testSharedVariable.Set(this, oldValue);
            // with a UnityEvent as response that registers whether it was invoked with certain parameters
            int invocations = 0;
            testSharedVariableListener.Response.AddListener((sender, value) => {
                Assert.AreEqual(this, sender);
                Assert.AreEqual(oldValue, value);
                invocations++;
            });

            // WHEN setting the value to the old one again
            testSharedVariable.Set(this, oldValue);
            yield return null;

            // THEN the response should have been processed
            Assert.AreEqual(1, invocations, "Should have responded once");
        }

        [TearDown]
        public void ClearScene() {
            foreach (var listener in Object.FindObjectsOfType<TestSharedVariableListener>()) {
                Object.Destroy(listener.gameObject);
            }
        }
    }
}