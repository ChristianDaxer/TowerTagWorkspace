using NUnit.Framework;
using SOEventSystem.Shared;
using UnityEngine;
using UnityEngine.TestTools;

namespace SOEventSystem.Tests {
    public class SharedEventTest {
        [Test]
        public void ShouldBeCreatable() {
            // GIVEN a shared event
            var sharedEvent = ScriptableObject.CreateInstance<SharedEvent>();

            // THEN it should not be null
            Assert.NotNull(sharedEvent);
        }

        [Test]
        public void ShouldBeCastToFalseWhenNull() {
            // GIVEN a shared event that is null
            SharedEvent sharedEvent = null;

            // THEN it should be cast to false
            Assert.False(sharedEvent);
        }

        [Test]
        public void ShouldBeCastToTrueWhenNotNull() {
            // GIVEN a shared event that is not null
            var sharedEvent = ScriptableObject.CreateInstance<SharedEvent>();

            // THEN it should be cast to false
            Assert.True(sharedEvent);
        }

        [Test]
        public void ShouldTriggerEventWithoutListeners() {
            // GIVEN a shared event with no event listeners
            var sharedEvent = ScriptableObject.CreateInstance<SharedEvent>();

            // WHEN it is triggered
            sharedEvent.Trigger(this);

            // THEN everything should be fine. No exceptions should be thrown.
        }

        [Test]
        public void ShouldTriggerEvent() {
            // GIVEN a shared event that passes the test when triggered with this as sender
            var sharedEvent = ScriptableObject.CreateInstance<SharedEvent>();
            sharedEvent.Triggered += sender => {
                if (sender.Equals(this)) Assert.Pass();
                Assert.Fail("Triggered with wrong sender");
            };

            // WHEN it is triggered
            sharedEvent.Trigger(this);

            // THEN the test passes via the callback or fails otherwise
            Assert.Fail("Should have triggerd change event");
        }

        [Test]
        public void ShouldLogWhenDebugFlagIsSet() {
            // GIVEN a shared event with name blubb and debug flag active
            var sharedEvent = ScriptableObject.CreateInstance<SharedEvent>();
            sharedEvent.name = "blubb";
            sharedEvent.Debug = true;

            // WHEN it is triggered
            sharedEvent.Trigger(this);

            // THEN it should log that blubb was triggered by this test
            LogAssert.Expect(LogType.Log, this + " triggered blubb");
        }

        [Test]
        public void ShouldNotLogByDefault() {
            // GIVEN a shared event
            var sharedEvent = ScriptableObject.CreateInstance<SharedEvent>();
            sharedEvent.name = "blubb";

            // WHEN it is triggered
            sharedEvent.Trigger(this);

            // THEN it should not log
            LogAssert.NoUnexpectedReceived();
        }
    }
}