using NUnit.Framework;
using SOEventSystem.Shared;
using UnityEngine;
using UnityEngine.TestTools;

namespace SOEventSystem.Tests {
    public class SharedVariableTest {
        [Test]
        public void ShouldBeCreatable() {
            // GIVEN a shared variable
            var sharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();

            // THEN it should not be null
            Assert.NotNull(sharedVariable);
        }

        [Test]
        public void ShouldHaveValueNullByDefault() {
            // GIVEN a shared variable
            var sharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();

            // THEN it should have null as a value
            Assert.Null(sharedVariable.Value);
        }

        [Test]
        public void ShouldHaveSetValue() {
            // GIVEN a shared variable with null as a value
            var sharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();

            // WHEN the value is set to an instance of TestClass
            var testClass = new TestClass();
            sharedVariable.Set(this, testClass);

            // THEN it should hold the set value
            Assert.AreEqual(testClass, sharedVariable.Value);
        }

        [Test]
        public void ShouldBeCastableToTypeOfValue() {
            // GIVEN a shared variable with null as a value
            var sharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();

            // WHEN the value is set to an instance of TestClass
            var testClass = new TestClass();
            sharedVariable.Set(this, testClass);

            // THEN it should be castable to that value
            Assert.AreEqual(testClass, (TestClass) sharedVariable);
        }

        [Test]
        public void ShouldRaiseChangeEventOnSet() {
            // GIVEN a shared variable with an instance of TestClass as value
            var sharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            var testClass = new TestClass();
            sharedVariable.Set(this, testClass);
            // and that passes the test when the change event is called with some specific value
            sharedVariable.ValueSet += (sender, value) => {
                if (value.Equals(testClass) && sender.Equals(this)) Assert.Pass();
                else Assert.Fail("Caught change event with wrong value");
            };

            // WHEN setting it to that specific value
            sharedVariable.Set(this, testClass);

            // THEN the test passes via the callback or fails otherwise
            Assert.Fail("Should have caught change event and passed Test");
        }

        [Test]
        public void ShouldRaiseChangeEventOnValueChange() {
            // GIVEN a shared variable with an instance of TestClass as value
            var sharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            var testClass = new TestClass();
            sharedVariable.Set(this, testClass);
            // and that passes the test when the change event is called with some specific value
            var newTestClass = new TestClass();
            sharedVariable.ValueChanged += (sender, value) => {
                if (value.Equals(newTestClass)) Assert.Pass();
                else Assert.Fail("Caught change event with wrong value");
            };

            // WHEN setting it to that specific value
            sharedVariable.Set(this, newTestClass);

            // THEN the test passes via the callback or fails otherwise
            Assert.Fail("Should have caught change event and passed Test");
        }

        [Test]
        public void ShouldNotRaiseChangeEventOnSetUnchanged() {
            // GIVEN a shared variable with some non-null value
            var sharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            var testClass = new TestClass();
            sharedVariable.Set(this, testClass);
            // that fails the test on any change event
            sharedVariable.ValueChanged += (sender, value) => Assert.Fail("Should not have triggered change event");

            // WHEN setting the value to the same as before
            sharedVariable.Set(this, testClass);

            // THEN the test fails via the callback or passes otherwise
        }

        [Test]
        public void ShouldLogWhenDebugFlagIsChanged() {
            // GIVEN a shared variable with name blubb and debug flag active
            var sharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            sharedVariable.name = "blubb";
            sharedVariable.Verbose = true;

            // WHEN it is set to testClass
            var testClass = new TestClass();
            sharedVariable.Set(this, testClass);

            // THEN it should log that blubb was set to testClass
            LogAssert.Expect(LogType.Log, this + " changed blubb to " + testClass);
        }

        [Test]
        public void ShouldLogWhenDebugFlagIsSetToUnchanged() {
            // GIVEN a shared variable with name blubb and debug flag active
            var sharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            sharedVariable.name = "blubb";
            var testClass = new TestClass();
            sharedVariable.Set(this, testClass);
            sharedVariable.Verbose = true;

            // WHEN it is set to testClass
            sharedVariable.Set(this, testClass);

            // THEN it should log that blubb was set to testClass
            LogAssert.Expect(LogType.Log, this + " set blubb to the same value " + testClass);
        }

        [Test]
        public void ShouldNotLogByDefault() {
            // GIVEN a shared event
            var sharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            sharedVariable.name = "blubb";

            // WHEN it is triggered or set
            sharedVariable.Set(this, new TestClass());

            // THEN it should not log
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ShouldDefaultToValueDefault() {
            Assert.AreEqual(null, (string) (SharedString) null);
            Assert.AreEqual(null, (TestClass) (TestSharedVariable) null);
            Assert.AreEqual(0, (int) (SharedInt) null);
            Assert.AreEqual(false, (bool) (SharedBool) null);
        }

        [Test]
        public void ShouldCreate() {
            var testClass = new TestClass();
            Assert.AreEqual(testClass, (TestClass) SharedVariable.Create<TestClass, TestSharedVariable>(testClass));
            Assert.AreEqual(7, SharedVariable.Create<int, SharedInt>(7).Value);
        }
    }
}