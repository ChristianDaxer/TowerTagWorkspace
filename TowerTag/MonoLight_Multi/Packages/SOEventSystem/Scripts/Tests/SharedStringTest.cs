using NUnit.Framework;
using SOEventSystem.Shared;
using UnityEngine;

namespace SOEventSystem.Tests {
    public class SharedStringTest {
        
        [Test]
        public void ShouldSetValue() {
            // GIVEN a shared string
            var sharedString = ScriptableObject.CreateInstance<SharedString>();
            // WHEN setting a value
            sharedString.Set(this, "bla");
            // THEN it should hold that value
            Assert.AreEqual("bla", sharedString.Value);
        }

        [Test]
        public void ShouldTriggerChangeEventWhenValueChanged() {
            // GIVEN a shared string with some value
            var sharedString = ScriptableObject.CreateInstance<SharedString>();
            sharedString.Set(this, "bla");
            sharedString.ValueChanged += (sender, value) => {
                Assert.AreEqual(this, sender);
                Assert.AreEqual("blubb", value);
                Assert.Pass();
            };
            // WHEN changing the value
            sharedString.Set(this, "blubb");
            // THEN the change event should be triggered with that value
            Assert.Fail();
        }
    }
}