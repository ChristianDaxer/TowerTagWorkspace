using NUnit.Framework;
using SOEventSystem.Shared;
using UnityEngine;

namespace SOEventSystem.Tests {
    public class SharedIntTest {
        [Test]
        public void ShouldBeCastToFalseWhenNull() {
            // GIVEN a shared int that is null
            SharedInt sharedInt = null;

            // THEN it should be cast to false
            Assert.False(sharedInt);
        }

        [Test]
        public void ShouldBeCreatable() {
            // GIVEN a shared int
            var sharedInt = ScriptableObject.CreateInstance<SharedInt>();

            // THEN it should not be null
            Assert.NotNull(sharedInt);
        }

        [Test]
        public void ShouldBeCastToTrueWhenNotNull() {
            // GIVEN a shared int that is not null
            var sharedInt = ScriptableObject.CreateInstance<SharedInt>();

            // THEN it should be cast to false
            Assert.True(sharedInt);
        }

        [Test]
        public void ShouldHaveDefaultValueAfterCreation() {
            // GIVEN a shared int
            var sharedInt = ScriptableObject.CreateInstance<SharedInt>();

            // then it should hold the default value
            Assert.AreEqual(default(int), (int) sharedInt);
        }

        [Test]
        public void ShouldHoldSetValue() {
            // GIVEN a shared int
            var sharedInt = ScriptableObject.CreateInstance<SharedInt>();

            // WHEN setting the value to 7
            sharedInt.Set(this, 7);

            // THEN it should hold the value 7
            Assert.AreEqual(7, sharedInt.Value);
        }

        [Test]
        public void ShouldBeCastableToInt() {
            // GIVEN a shared int with value 7
            var sharedInt = ScriptableObject.CreateInstance<SharedInt>();
            sharedInt.Set(this, 7);

            // THEN it should hold the value 7
            Assert.AreEqual(7, (int) sharedInt);
        }

        [Test]
        public void ShouldRaiseChangeEvent() {
            // GIVEN a shared int
            var sharedInt = ScriptableObject.CreateInstance<SharedInt>();
            // that passes the test when triggered with this as sender and value 7
            sharedInt.ValueChanged += (sender, value) => {
                Assert.AreEqual(this, sender, "Event raised with wrong sender");
                Assert.AreEqual(7, value, "Event raised with wrong value");
                Assert.Pass();
            };

            // WHEN setting the value
            sharedInt.Set(this, 7);

            // THEN the test passes via the callback or fails otherwise
            Assert.Fail("Should have caught change event and passed Test");
        }

        [Test]
        public void ShouldNotRaiseChangeEventOnSetUnchanged() {
            // GIVEN a shared int with value 5
            var sharedInt = ScriptableObject.CreateInstance<SharedInt>();
            sharedInt.Set(this, 5);
            // that fails the test on any event
            sharedInt.ValueChanged += (sender, value) => { Assert.Fail("Should not have triggered change event"); };

            // WHEN set to value 5 again
            sharedInt.Set(this, 5);

            // THEN the test fails via the callback or passes otherwise
        }

        [Test]
        public void ShouldRaiseSetEventOnValueChange() {
            // GIVEN a shared int with value 5
            var sharedInt = ScriptableObject.CreateInstance<SharedInt>();
            sharedInt.Set(this, 5);
            // that passes the test when triggered with value 5 and this as sender
            sharedInt.ValueSet += (sender, value) => {
                Assert.AreEqual(this, sender, "Event raised with wrong sender");
                Assert.AreEqual(7, value, "Event raised with wrong value");
                Assert.Pass();
            };

            // WHEN changing to value 7
            sharedInt.Set(this, 7);

            // THEN the test passes via the callback or fails otherwise
            Assert.Fail("Should have caught change event and passed Test");
        }

        [Test]
        public void ShouldRaiseSetEventOnSetUnchanged() {
            // GIVEN a shared int with value 5
            var sharedInt = ScriptableObject.CreateInstance<SharedInt>();
            sharedInt.Set(this, 5);
            // that passes the test when triggered with value 5 and this as sender
            sharedInt.ValueSet += (sender, value) => {
                Assert.AreEqual(this, sender, "Event raised with wrong sender");
                Assert.AreEqual(5, value, "Event raised with wrong value");
                Assert.Pass();
            };

            // WHEN set to value 5 again
            sharedInt.Set(this, 5);

            // THEN the test passes via the callback or fails otherwise
            Assert.Fail("Should have caught change event and passed Test");
        }

        [Test]
        public void ShouldRaiseChangeEventOnValueChange() {
            // GIVEN a shared int with value 5
            var sharedInt = ScriptableObject.CreateInstance<SharedInt>();
            sharedInt.Set(this, 5);
            // that passes the test when triggered with value 7 and this as sender
            sharedInt.ValueChanged += (sender, value) => {
                Assert.AreEqual(this, sender, "Event raised with wrong sender");
                Assert.AreEqual(7, value, "Event raised with wrong value");
                Assert.Pass();
            };

            // WHEN set to value 7
            sharedInt.Set(this, 7);

            // THEN the test passes via the callback or fails otherwise
            Assert.Fail("Should have caught change event and passed Test");
        }
    }
}