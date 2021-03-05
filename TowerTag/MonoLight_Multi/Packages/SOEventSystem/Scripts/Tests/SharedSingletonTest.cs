using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SOEventSystem.Tests {
    public class SharedSingletonTest {
        private const string SingletonTestScene = "SingletonTest";

        [SetUp]
        public void SetUp() {
            foreach (TestSingleton singleton in Resources.FindObjectsOfTypeAll<TestSingleton>()) {
                if (AssetDatabase.Contains(singleton)) {
                    Resources.UnloadAsset(singleton);
                }
                else {
                    Object.Destroy(singleton);
                }
            }
        }

        [Test]
        public void ShouldReturnNullSingletonInstance() {
            // GIVEN no TestSingleton asset loaded
            // THEN the TestSingleton instance should be null
            Assert.Null(TestSingleton.Singleton);
        }

        [UnityTest]
        public IEnumerator ShouldReturnDynamicallyCreatedInstance() {
            // GIVEN the certainty that that there is no TestSingleton asset loaded
            Object.Destroy(TestSingleton.Singleton);
            Assert.Null(TestSingleton.Singleton);

            // WHEN creating an instance of TestSingleton
            var singleton = ScriptableObject.CreateInstance<TestSingleton>();

            // THEN should find freshly created instance
            Assert.AreEqual(singleton, TestSingleton.Singleton);

            // cleanup
            Object.Destroy(singleton);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ShouldLogWarningForMultipleInstances() {
            // GIVEN the certainty that that there is no TestSingleton asset loaded
            Assert.Null(TestSingleton.Singleton);

            // WHEN creating two instances of TestSingleton
            var singleton1 = ScriptableObject.CreateInstance<TestSingleton>();
            var singleton2 = ScriptableObject.CreateInstance<TestSingleton>();
            // and querying the singleton instance
            Assert.NotNull(TestSingleton.Singleton);

            // THEN there should be a warning that there are multiple instances
            LogAssert.Expect(LogType.Warning,
                "There are multiple instances of singleton type SOEventSystem.Tests.TestSingleton. Returning a random instance.");

            // cleanup
            Object.Destroy(singleton1);
            Object.Destroy(singleton2);
            yield return null;
        }
    }
}