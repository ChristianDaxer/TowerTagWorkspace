using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace VRNerdsUtilities.Tests {
    public class SingletonMonoBehaviourTest {
        [Test]
        public void ShouldAutoCreate() {
            // querying singleton instance should create object
            DummySingletonMonoBehaviour singleton = DummySingletonMonoBehaviour.Instance;
            Assert.NotNull(singleton);
            var singletonGameObject = Object.FindObjectOfType<DummySingletonMonoBehaviour>();
            Assert.NotNull(singletonGameObject);
        }

        [UnityTest]
        public IEnumerator ShouldFindInstanceInScene() {
            foreach (DummySingletonMonoBehaviour go in Object.FindObjectsOfType<DummySingletonMonoBehaviour>()) {
                Object.Destroy(go.gameObject);
            }

            yield return null;

            var dummySingletonMonoBehaviour = new GameObject().AddComponent<DummySingletonMonoBehaviour>();

            Assert.AreSame(dummySingletonMonoBehaviour, DummySingletonMonoBehaviour.Instance);
        }

        [UnityTest]
        public IEnumerator ShouldDestroySecondInstanceByDefault() {
            foreach (DummySingletonMonoBehaviour go in Object.FindObjectsOfType<DummySingletonMonoBehaviour>()) {
                Object.Destroy(go.gameObject);
            }

            yield return null;

            new GameObject().AddComponent<DummySingletonMonoBehaviour>();
            var testGameObject = new GameObject();
            testGameObject.AddComponent<DummySingletonMonoBehaviour>();

            yield return null;

            Assert.AreEqual(1, Object.FindObjectsOfType<DummySingletonMonoBehaviour>().Length);
            Assert.Null(testGameObject.GetComponent<DummySingletonMonoBehaviour>());
        }
    }
}