using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using SOEventSystem.Addressable;
using SOEventSystem.Shared;
using UnityEngine;
using UnityEngine.TestTools;

namespace SOEventSystem.Tests {
    public class SerializedSharedVariableTest {
        public class TestSerializableSharedVariable : SerializableSharedVariable<string> { }

        [TearDown]
        public void TearDown() {
            if (AddressableAssetDatabase.Singleton != null) Object.Destroy(AddressableAssetDatabase.Singleton);
        }

        [UnityTest]
        public IEnumerator ShouldSerializeAutomatically() {
            // GIVEN
            var testSerializableVariable = ScriptableObject.CreateInstance<TestSerializableSharedVariable>();
            var serialized = 0;
            testSerializableVariable.ValueSerialized += (sender, bytes) => {
                Assert.AreEqual(34, bytes.Length, "Byte serialization of test string is expected to be 34 bytes");
                Assert.AreEqual(testSerializableVariable.SerializedData, bytes);
                serialized++;
            };

            // WHEN
            testSerializableVariable.Set(this, "testString");

            // THEN
            Assert.AreEqual(1, serialized);
            return null;
        }

        [UnityTest]
        public IEnumerator ShouldDeserialize() {
            // GIVEN
            var testSerializableVariable = ScriptableObject.CreateInstance<TestSerializableSharedVariable>();

            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, "testString");
            byte[] bytes = memoryStream.ToArray();

            // WHEN
            testSerializableVariable.Set(this, bytes);

            // THEN
            Assert.AreEqual("testString", testSerializableVariable.Value);
            return null;
        }

        [UnityTest]
        public IEnumerator ShouldRegister() {
            // GIVEN
            var testAddressableAssetDatabase = ScriptableObject.CreateInstance<AddressableAssetDatabase>();
            Assert.AreEqual(0, testAddressableAssetDatabase.AddressableAssets.Count);
            ScriptableObject.CreateInstance<TestSerializableSharedVariable>();

            // THEN
            Assert.AreEqual(1, testAddressableAssetDatabase.AddressableAssets.Count);
            return null;
        }

        [UnityTest]
        public IEnumerator ShouldUnregister() {
            // GIVEN
            var testAddressableAssetDatabase = ScriptableObject.CreateInstance<AddressableAssetDatabase>();
            var testSerializableVariable = ScriptableObject.CreateInstance<TestSerializableSharedVariable>();
            Object.Destroy(testSerializableVariable);
            yield return null;

            // THEN
            Assert.AreEqual(0, testAddressableAssetDatabase.AddressableAssets.Count);
        }
    }
}