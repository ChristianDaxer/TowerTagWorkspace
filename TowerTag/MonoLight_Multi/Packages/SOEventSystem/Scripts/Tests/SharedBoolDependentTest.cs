using System.Collections;
using NUnit.Framework;
using SOEventSystem.Listeners;
using SOEventSystem.Shared;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace SOEventSystem.Tests {
    public class SharedBoolDependentTest {
        [UnityTest]
        public IEnumerator ShouldDeactiveTargetWhenFalse() {
            // GIVEN a SharedBoolDependent with a SharedBool with value false and ActiveWhenTrue policy
            var sharedBoolDependent = new GameObject().AddComponent<SharedBoolDependent>();
            var sharedBool = ScriptableObject.CreateInstance<SharedBool>();
            sharedBoolDependent.SharedBool = sharedBool;
            var target = new GameObject();
            sharedBoolDependent.Target = target;
            sharedBoolDependent.Policy = SharedBoolDependent.ActivityPolicy.ActiveWhenTrue;
            sharedBool.Set(this, false);

            // THEN target should de deactivated
            Assert.IsFalse(target.activeSelf);

            return null;
        }

        [UnityTest]
        public IEnumerator ShouldToggleBackAndForthWithBool() {
            // GIVEN a SharedBoolDependent with a SharedBool with value false and ActiveWhenTrue policy
            var sharedBoolDependent = new GameObject().AddComponent<SharedBoolDependent>();
            var sharedBool = ScriptableObject.CreateInstance<SharedBool>();
            sharedBoolDependent.SharedBool = sharedBool;
            var target = new GameObject();
            sharedBoolDependent.Target = target;
            sharedBoolDependent.Policy = SharedBoolDependent.ActivityPolicy.ActiveWhenTrue;

            // THEN target should be toggled
            sharedBool.Set(this, true);
            Assert.IsTrue(target.activeSelf);
            sharedBool.Set(this, false);
            Assert.IsFalse(target.activeSelf);
            sharedBool.Set(this, true);
            Assert.IsTrue(target.activeSelf);

            return null;
        }

        [UnityTest]
        public IEnumerator ShouldToggleBackAndForthWithPolicy() {
            // GIVEN a SharedBoolDependent with a SharedBool with value false and ActiveWhenTrue policy
            var sharedBoolDependent = new GameObject().AddComponent<SharedBoolDependent>();
            var sharedBool = ScriptableObject.CreateInstance<SharedBool>();
            sharedBoolDependent.SharedBool = sharedBool;
            var target = new GameObject();
            sharedBoolDependent.Target = target;
            sharedBool.Set(this, true);

            // THEN target should be toggled
            sharedBoolDependent.Policy = SharedBoolDependent.ActivityPolicy.ActiveWhenTrue;
            Assert.IsTrue(target.activeSelf);
            sharedBoolDependent.Policy = SharedBoolDependent.ActivityPolicy.ActiveWhenFalse;
            Assert.IsFalse(target.activeSelf);
            sharedBoolDependent.Policy = SharedBoolDependent.ActivityPolicy.ActiveWhenTrue;
            Assert.IsTrue(target.activeSelf);

            return null;
        }

        [UnityTest]
        public IEnumerator ShouldActivateTargetWhenTrue() {
            // GIVEN a SharedBoolDependent with a SharedBool with value true and ActiveWhenTrue policy
            var sharedBoolDependent = new GameObject().AddComponent<SharedBoolDependent>();
            var sharedBool = ScriptableObject.CreateInstance<SharedBool>();
            sharedBoolDependent.SharedBool = sharedBool;
            var target = new GameObject();
            sharedBoolDependent.Target = target;
            sharedBoolDependent.Policy = SharedBoolDependent.ActivityPolicy.ActiveWhenTrue;
            sharedBool.Set(this, true);
            
            // THEN target should be activated
            Assert.IsTrue(target.activeSelf);

            return null;
        }

        [UnityTest]
        public IEnumerator ShouldDeactiveTargetWhenTrue() {
            // GIVEN a SharedBoolDependent with a SharedBool with value false and ActiveWhenTrue policy
            var sharedBoolDependent = new GameObject().AddComponent<SharedBoolDependent>();
            var sharedBool = ScriptableObject.CreateInstance<SharedBool>();
            sharedBoolDependent.SharedBool = sharedBool;
            var target = new GameObject();
            sharedBoolDependent.Target = target;
            sharedBoolDependent.Policy = SharedBoolDependent.ActivityPolicy.ActiveWhenFalse;
            sharedBool.Set(this, true);

            // THEN target should de deactivated
            Assert.IsFalse(target.activeSelf);

            return null;
        }

        [UnityTest]
        public IEnumerator ShouldActivateTargetWhenFalse() {
            // GIVEN a SharedBoolDependent with a SharedBool with value false and ActiveWhenTrue policy
            var sharedBoolDependent = new GameObject().AddComponent<SharedBoolDependent>();
            var sharedBool = ScriptableObject.CreateInstance<SharedBool>();
            sharedBoolDependent.SharedBool = sharedBool;
            var target = new GameObject();
            sharedBoolDependent.Target = target;
            sharedBoolDependent.Policy = SharedBoolDependent.ActivityPolicy.ActiveWhenFalse;
            sharedBool.Set(this, false);

            // THEN target should de deactivated
            Assert.IsTrue(target.activeSelf);

            return null;
        }

        [TearDown]
        public void ClearTestScene() {
            foreach (GameObject gameObject in Object.FindObjectsOfType<GameObject>()) {
                Object.Destroy(gameObject);
            }
        }
    }
}