using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using SOEventSystem.Addressable;
using SOEventSystem.Shared;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace SOEventSystem.Tests {
    public class AddressableAssetTest {
        public class TestAddressableAsset : AddressableAsset { }

        [SetUp]
        public void SetUp() {
            foreach (AddressableAssetDatabase singleton in Resources.FindObjectsOfTypeAll<AddressableAssetDatabase>()) {
                if (AssetDatabase.Contains(singleton)) {
                    Resources.UnloadAsset(singleton);
                }
                else {
                    Object.Destroy(singleton);
                }
            }
            ScriptableObject.CreateInstance<AddressableAssetDatabase>();
        }

        [UnityTest]
        public IEnumerator ShouldRegister() {
            // GIVEN
            Assert.AreEqual(0, AddressableAssetDatabase.Singleton.AddressableAssets.Count);
            var testAddressableAsset = ScriptableObject.CreateInstance<TestAddressableAsset>();

            // THEN
            Assert.AreEqual(1, AddressableAssetDatabase.Singleton.AddressableAssets.Count);
            Assert.AreEqual(testAddressableAsset, AddressableAssetDatabase.Singleton.Get(testAddressableAsset.AssetGuid));
            return null;
        }

        [UnityTest]
        public IEnumerator ShouldUnregister() {
            // GIVEN
            var testAddressableAsset = ScriptableObject.CreateInstance<TestAddressableAsset>();
            Object.Destroy(testAddressableAsset);
            yield return null;

            // THEN
            Assert.AreEqual(0, AddressableAssetDatabase.Singleton.AddressableAssets.Count);
        }
    }
}