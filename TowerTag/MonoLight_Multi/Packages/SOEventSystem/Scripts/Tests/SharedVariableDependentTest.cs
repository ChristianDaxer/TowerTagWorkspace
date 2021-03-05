using System.Collections;
using SOEventSystem.Listeners;
using SOEventSystem.Shared;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace SOEventSystem.Tests {
    public class SharedVariableDependentTest {
        public class SharedStringDependent : SharedVariableDependent<string, SharedString> { }

        [UnityTest]
        public IEnumerator ShouldActivateOnStart() {
            // GIVEN a shared variable with value from list, some target objects, and a sharedVariableDependent
            SharedString sharedString = SharedVariable.Create<string, SharedString>("contained");
            var target1 = new GameObject("Target1");
            var target2 = new GameObject("Target2");
            target1.SetActive(false);
            target2.SetActive(false);
            var sharedStringDependent = SharedVariableDependent<string, SharedString>
                .Create<SOEventSystem.Listeners.SharedStringDependent>(
                    new[] {target1, target2},
                    sharedString,
                    new[] {"contained", "also contained"},
                    SharedVariableDependent.Policy.ActiveWhenContained
                );

            // WHEN waiting a frame
            yield return null;

            // THEN targets should be deactivated
            Assert.IsTrue(target1.activeSelf);
            Assert.IsTrue(target2.activeSelf);

            // CLEANUP
            Object.Destroy(sharedString);
            Object.Destroy(target1);
            Object.Destroy(target2);
            Object.Destroy(sharedStringDependent.gameObject);
        }

        [UnityTest]
        public IEnumerator ShouldDeactivateOnStart() {
            // GIVEN a shared variable with value not from list, some target objects, and a sharedVariableDependent
            SharedString sharedString = SharedVariable.Create<string, SharedString>("contained");
            var target1 = new GameObject("Target1");
            var target2 = new GameObject("Target2");
            var sharedStringDependent = SharedVariableDependent<string, SharedString>
                .Create<SOEventSystem.Listeners.SharedStringDependent>(
                    new[] {target1, target2},
                    sharedString,
                    new[] {"contained", "also contained"},
                    SharedVariableDependent.Policy.DeactivatedWhenContained
                );

            // WHEN waiting a frame
            yield return null;

            // THEN targets should be deactivated
            Assert.IsFalse(target1.activeSelf);
            Assert.IsFalse(target2.activeSelf);

            // CLEANUP
            Object.Destroy(sharedString);
            Object.Destroy(target1);
            Object.Destroy(target2);
            Object.Destroy(sharedStringDependent.gameObject);
        }

        [UnityTest]
        public IEnumerator ShouldActivateWhenValueChangedToContained() {
            // GIVEN a shared variable with value not in list, some target objects, and a sharedVariableDependent
            SharedString sharedString = SharedVariable.Create<string, SharedString>("not contained");
            var target1 = new GameObject("Target1");
            var target2 = new GameObject("Target2");
            var sharedStringDependent = SharedVariableDependent<string, SharedString>
                .Create<SOEventSystem.Listeners.SharedStringDependent>(
                    new[] {target1, target2},
                    sharedString,
                    new[] {"contained", "also contained"},
                    SharedVariableDependent.Policy.ActiveWhenContained
                );

            // WHEN waiting a frame and then setting the shared variable to value from the list
            yield return null;
            Assert.IsFalse(target1.activeSelf);
            Assert.IsFalse(target2.activeSelf);
            sharedString.Set(this, "contained");

            // THEN targets should be activated
            Assert.IsTrue(target1.activeSelf);
            Assert.IsTrue(target2.activeSelf);

            // CLEANUP
            Object.Destroy(sharedString);
            Object.Destroy(target1);
            Object.Destroy(target2);
            Object.Destroy(sharedStringDependent.gameObject);
        }

        [UnityTest]
        public IEnumerator ShouldDeactivateWhenValueChangedToContained() {
            // GIVEN a shared variable with value not in list, some target objects, and a sharedVariableDependent
            SharedString sharedString = SharedVariable.Create<string, SharedString>("not contained");
            var target1 = new GameObject("Target1");
            var target2 = new GameObject("Target2");
            var sharedStringDependent = SharedVariableDependent<string, SharedString>
                .Create<SOEventSystem.Listeners.SharedStringDependent>(
                    new[] {target1, target2},
                    sharedString,
                    new[] {"contained", "also contained"},
                    SharedVariableDependent.Policy.DeactivatedWhenContained
                );

            // WHEN waiting a frame and then setting the shared variable to a value not contained in the list
            yield return null;
            Assert.IsTrue(target1.activeSelf);
            Assert.IsTrue(target2.activeSelf);
            sharedString.Set(this, "contained");

            // THEN targets should be deactivated
            Assert.IsFalse(target1.activeSelf);
            Assert.IsFalse(target2.activeSelf);

            // CLEANUP
            Object.Destroy(sharedString);
            Object.Destroy(target1);
            Object.Destroy(target2);
            Object.Destroy(sharedStringDependent.gameObject);
        }

        [UnityTest]
        public IEnumerator ShouldActivateWhenValueChangedToNotContained() {
            // GIVEN a shared variable with value not in list, some target objects, and a sharedVariableDependent
            SharedString sharedString = SharedVariable.Create<string, SharedString>("contained");
            var target1 = new GameObject("Target1");
            var target2 = new GameObject("Target2");
            var sharedStringDependent = SharedVariableDependent<string, SharedString>
                .Create<SOEventSystem.Listeners.SharedStringDependent>(
                    new[] {target1, target2},
                    sharedString,
                    new[] {"contained", "also contained"},
                    SharedVariableDependent.Policy.DeactivatedWhenContained
                );

            // WHEN waiting a frame and then setting the shared variable to value from the list
            yield return null;
            Assert.IsFalse(target1.activeSelf);
            Assert.IsFalse(target2.activeSelf);
            sharedString.Set(this, "not contained");

            // THEN targets should be activated
            Assert.IsTrue(target1.activeSelf);
            Assert.IsTrue(target2.activeSelf);

            // CLEANUP
            Object.Destroy(sharedString);
            Object.Destroy(target1);
            Object.Destroy(target2);
            Object.Destroy(sharedStringDependent.gameObject);
        }

        [UnityTest]
        public IEnumerator ShouldDeactivateWhenValueChangedToNotContained() {
            // GIVEN a shared variable with value not in list, some target objects, and a sharedVariableDependent
            SharedString sharedString = SharedVariable.Create<string, SharedString>("contained");
            var target1 = new GameObject("Target1");
            var target2 = new GameObject("Target2");
            var sharedStringDependent = SharedVariableDependent<string, SharedString>
                .Create<SOEventSystem.Listeners.SharedStringDependent>(
                    new[] {target1, target2},
                    sharedString,
                    new[] {"contained", "also contained"},
                    SharedVariableDependent.Policy.ActiveWhenContained
                );

            // WHEN waiting a frame and then setting the shared variable to a value not contained in the list
            yield return null;
            Assert.IsTrue(target1.activeSelf);
            Assert.IsTrue(target2.activeSelf);
            sharedString.Set(this, "not contained");

            // THEN targets should be deactivated
            Assert.IsFalse(target1.activeSelf);
            Assert.IsFalse(target2.activeSelf);

            // CLEANUP
            Object.Destroy(sharedString);
            Object.Destroy(target1);
            Object.Destroy(target2);
            Object.Destroy(sharedStringDependent.gameObject);
        }
        
        [UnityTest]
        public IEnumerator ShouldHandleNullControlledObject() {
            // GIVEN a shared variable with value not in list, some target objects, and a sharedVariableDependent
            SharedString sharedString = SharedVariable.Create<string, SharedString>("not contained");
            var target1 = new GameObject("Target1");
            SharedVariableDependent<string, SharedString>
                .Create<SOEventSystem.Listeners.SharedStringDependent>(
                    new[] {target1, null},
                    sharedString,
                    new[] {"contained", "also contained"},
                    SharedVariableDependent.Policy.ActiveWhenContained
                );

            // WHEN waiting a frame and then setting the shared variable to value from the list
            yield return null;
        }
    }
}