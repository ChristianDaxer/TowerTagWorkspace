using System.Collections.Generic;
using NUnit.Framework;
using SOEventSystem.Shared;
using UnityEngine;

namespace SOEventSystem.Tests {
    public class SharedTestList : SharedList<TestClass> { }

    public class SharedListTest {
        [Test]
        public void ShouldBeEmptyAfterCreation() {
            // GIVEN a newly created shared list
            var sharedList = ScriptableObject.CreateInstance<SharedTestList>();
            // THEN it should be empty
            Assert.AreEqual(0, sharedList.Value.Count);
        }

        [Test]
        public void ShouldAddItem() {
            // GIVEN an empty shared list
            var sharedList = ScriptableObject.CreateInstance<SharedTestList>();
            // WHEN adding an item
            var testClass = new TestClass();
            sharedList.Add(this, testClass);
            // THEN the list should contain that item and no other
            Assert.AreEqual(new List<TestClass> {testClass}, sharedList.Value);
        }

        [Test]
        public void ShouldRemoveItem() {
            // GIVEN a shared list with some items
            var sharedList = ScriptableObject.CreateInstance<SharedTestList>();
            var testClass1 = new TestClass();
            sharedList.Add(this, testClass1);
            var testClass2 = new TestClass();
            sharedList.Add(this, testClass2);
            var testClass3 = new TestClass();
            sharedList.Add(this, testClass3);
            // WHEN removing an item
            sharedList.Remove(this, testClass2);
            // THEN the list should not contain that item anymore, but still the others
            Assert.AreEqual(new List<TestClass> {testClass1, testClass3}, sharedList.Value);
        }

        [Test]
        public void ShouldClear() {
            // GIVEN a shared list with some items
            var sharedList = ScriptableObject.CreateInstance<SharedTestList>();
            var testClass1 = new TestClass();
            sharedList.Add(this, testClass1);
            var testClass2 = new TestClass();
            sharedList.Add(this, testClass2);
            var testClass3 = new TestClass();
            sharedList.Add(this, testClass3);
            // WHEN clearing the list
            sharedList.Clear(this);
            // THEN the list should be empty
            Assert.AreEqual(0, sharedList.Value.Count);
        }

        [Test]
        public void ShouldInsertItem() {
            // GIVEN a shared list with some items
            var sharedList = ScriptableObject.CreateInstance<SharedTestList>();
            var testClass1 = new TestClass();
            sharedList.Add(this, testClass1);
            var testClass3 = new TestClass();
            sharedList.Add(this, testClass3);
            // WHEN removing an item
            var testClass2 = new TestClass();
            sharedList.Insert(this, 1, testClass2);
            // THEN the list should not contain that item anymore, but still the others
            Assert.AreEqual(new List<TestClass> {testClass1, testClass2, testClass3}, sharedList.Value);
        }

        [Test]
        public void ShouldRaiseEventOnItemAdd() {
            // GIVEN a shared list
            var sharedList = ScriptableObject.CreateInstance<SharedTestList>();
            var testClass = new TestClass();
            sharedList.ItemAdded += (sender, item) => {
                if (sender == this && item == testClass) Assert.Pass();
                Assert.Fail("Event raised with unexpected arguments");
            };
            // WHEN adding an item
            sharedList.Add(this, testClass);
            // THEN the itemAdded event should be raised. Test passes via callback
            Assert.Fail("Should have raised item added event");
        }

        [Test]
        public void ShouldRaiseEventOnItemRemove() {
            // GIVEN a shared list
            var sharedList = ScriptableObject.CreateInstance<SharedTestList>();
            var testClass = new TestClass();
            sharedList.Add(this, testClass);
            sharedList.ItemRemoved += (sender, item) => {
                if (sender == this && item == testClass) Assert.Pass();
                Assert.Fail("Event raised with unexpected arguments");
            };
            // WHEN removing an item
            sharedList.Remove(this, testClass);
            // THEN the itemRemoved event should be raised. Test passes via callback
            Assert.Fail("Should have raised item removed event");
        }
    }
}