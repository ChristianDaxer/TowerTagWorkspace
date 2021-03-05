using NUnit.Framework;
using SOEventSystem.References;
using UnityEngine;

namespace SOEventSystem.Tests {
    public class SharedReferenceTest {
        private class SharedTestReference : SharedReference<TestClass, TestSharedVariable> { }

        [Test]
        public void ShouldReturnPrivateValue() {
            // GIVEN a shared reference set to use a private value
            var sharedTestReference = new SharedTestReference {UsePrivateValue = true};
            // WHEN setting the private value
            var testClass = new TestClass();
            sharedTestReference.Private = testClass;
            // THEN it should return that value
            Assert.AreEqual(testClass, sharedTestReference.Value);
            Assert.AreEqual(testClass, (TestClass) sharedTestReference);
        }

        [Test]
        public void ShouldSetPrivateValue() {
            // GIVEN a shared reference set to use a private value
            var sharedTestReference = new SharedTestReference {UsePrivateValue = true};
            // WHEN setting a value
            var testClass = new TestClass();
            sharedTestReference.Value = testClass;
            // THEN it should have that value
            Assert.AreEqual(testClass, sharedTestReference.Value);
            Assert.AreEqual(testClass, (TestClass) sharedTestReference);
        }

        [Test]
        public void ShouldImplicitlyCastPrivateValue() {
            // GIVEN a shared reference set to use a set private value
            var sharedTestReference = new SharedTestReference {UsePrivateValue = true, Private = new TestClass()};
            // THEN it should be cast to its value
            Assert.AreEqual(sharedTestReference.Value, (TestClass) sharedTestReference);
        }

        [Test]
        public void ShouldSetSharedValue() {
            // GIVEN a shared reference using a shared variable
            var testSharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            var sharedTestReference = new SharedTestReference {
                UsePrivateValue = false,
                Shared = testSharedVariable
            };
            // WHEN setting the value of the shared variable
            var testClass = new TestClass();
            sharedTestReference.Value = testClass;
            // THEN the shared variable should have that value
            Assert.AreEqual(testClass, testSharedVariable.Value);
        }

        [Test]
        public void ShouldReturnSharedValue() {
            // GIVEN a shared reference using a shared variable
            var testSharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            var sharedTestReference = new SharedTestReference {
                UsePrivateValue = false,
                Shared = testSharedVariable
            };
            // WHEN setting the value of the shared variable
            var testClass = new TestClass();
            testSharedVariable.Set(this, testClass);
            // THEN the reference should have that value
            Assert.AreEqual(testClass, sharedTestReference.Value);
            Assert.AreEqual(testClass, (TestClass) sharedTestReference);
        }

        [Test]
        public void ShouldImplicitlyCastSharedValue() {
            // GIVEN a shared reference set to use a set shared value
            var testSharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            testSharedVariable.Set(this, new TestClass());
            var sharedTestReference = new SharedTestReference {UsePrivateValue = false, Shared = testSharedVariable};
            // THEN it should be cast to its value
            Assert.AreEqual(sharedTestReference.Value, (TestClass) sharedTestReference);
        }

        [Test]
        public void ShouldReturnConfiguredValue() {
            // GIVEN a shared reference
            var testSharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            var sharedTestClass = new TestClass();
            testSharedVariable.Set(this, sharedTestClass);
            var privateTestClass = new TestClass();
            var sharedTestReference = new SharedTestReference {
                UsePrivateValue = false,
                Shared = testSharedVariable,
                Private = privateTestClass
            };
            // THEN the reference should have these values
            sharedTestReference.UsePrivateValue = true;
            Assert.AreEqual(privateTestClass, sharedTestReference.Value);
            sharedTestReference.UsePrivateValue = false;
            Assert.AreEqual(sharedTestClass, sharedTestReference.Value);
        }

        [Test]
        public void ShouldInitializeSharedVariable() {
            // GIVEN a shared reference set to use a shared variable without assigning one
            var sharedTestReference = new SharedTestReference {UsePrivateValue = false};
            // WHEN setting its value
            var testClass = new TestClass();
            sharedTestReference.Value = testClass;
            // THEN it should have that value
            Assert.AreEqual(testClass, sharedTestReference.Value);
            Assert.AreEqual(testClass, (TestClass) sharedTestReference);
        }

        [Test]
        public void ShouldNotChangePrivateValueWhenUsingShared() {
            // GIVEN a shared reference with a set private value, but using the shared value
            var testClass1 = new TestClass();
            var sharedTestReference = new SharedTestReference {
                UsePrivateValue = false,
                Private = testClass1
            };
            // WHEN setting the value
            var testClass2 = new TestClass();
            sharedTestReference.Value = testClass2;
            // THEN the private value should remain unchanged
            Assert.AreEqual(testClass2, sharedTestReference.Value);
            sharedTestReference.UsePrivateValue = true;
            Assert.AreEqual(testClass1, sharedTestReference.Value);
        }

        [Test]
        public void ShouldNotChangeSharedValueWhenUsingPrivate() {
            // GIVEN a shared reference with a set shared value, but using the private value
            var testClass1 = new TestClass();
            var testSharedVariable = ScriptableObject.CreateInstance<TestSharedVariable>();
            testSharedVariable.Set(this, testClass1);
            var sharedTestReference = new SharedTestReference {
                UsePrivateValue = true,
                Shared = testSharedVariable
            };
            // WHEN setting the value
            var testClass2 = new TestClass();
            sharedTestReference.Value = testClass2;
            // THEN the shared value should remain unchanged
            Assert.AreEqual(testClass2, sharedTestReference.Value);
            sharedTestReference.UsePrivateValue = false;
            Assert.AreEqual(testClass1, sharedTestReference.Value);
        }
    }
}