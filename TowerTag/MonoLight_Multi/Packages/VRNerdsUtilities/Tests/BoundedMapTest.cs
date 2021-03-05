using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace VRNerdsUtilities.Tests {
    public class BoundedMapTest {
        [Test]
        public void ShouldCreate() {
            var map = new BoundedMap<int, int>(7);
            Assert.AreEqual(0, map.Count);
        }

        [Test]
        public void ShouldAddItem() {
            var map = new BoundedMap<int, int>(7);
            map.Add(3, 7);
            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(7, map[3]);
        }

        [Test]
        public void ShouldNotCreateWithNegativeBounds() {
            try {
                // ReSharper disable once ObjectCreationAsStatement : testing constructor throws exception
                new BoundedMap<int, int>(-1);
            }
            catch (ArgumentOutOfRangeException) {
                Assert.Pass();
            }

            Assert.Fail("Should have thrown argument out of bounds exception");
        }

        [Test]
        public void ShouldStayInBounds() {
            var map = new BoundedMap<int, int>(3);
            map.Add(1, 2);
            map.Add(3, 4);
            map.Add(5, 6);
            map.Add(7, 8);
            Assert.AreEqual(3, map.Count);
            Assert.True(map.ContainsKey(3));
            Assert.True(map.ContainsKey(5));
            Assert.True(map.ContainsKey(7));
            Assert.False(map.ContainsKey(1));
        }

        [Test]
        public void ShouldClear() {
            var map = new BoundedMap<int, int>(3);
            map.Add(1, 2);
            map.Add(3, 4);
            map.Add(5, 6);
            map.Clear();
            Assert.AreEqual(0, map.Count);
        }

        [Test]
        public void ShouldRemoveItem() {
            var map = new BoundedMap<int, int>(3);
            map.Add(1, 2);
            map.Add(3, 4);
            map.Remove(1);
            Assert.AreEqual(1, map.Count);
            Assert.True(map.ContainsKey(3));
            Assert.False(map.ContainsKey(1));
        }

        [Test]
        public void ShouldAddViaBrackets() {
            var map = new BoundedMap<int, int>(3);
            map.Add(1, 2);
            map.Add(3, 4);
            map[5] = 6;
            map[7] = 8;
            Assert.AreEqual(3, map.Count);
            Assert.AreEqual(4, map[3]);
            Assert.AreEqual(6, map[5]);
            Assert.AreEqual(8, map[7]);
        }

        [Test]
        public void ShouldThrowKeyNotFoundException() {
            var map = new BoundedMap<int, int>(3);
            try {
                int i = map[3];
                Assert.Fail("Should have thrown KeyNotFoundException, but found {0}", i);
            }
            catch (KeyNotFoundException) {
                Assert.Pass();
            }
        }

        [Test]
        public void ShouldHandleNullValues() {
            var map = new BoundedMap<int, string>(3);
            map[1] = "test";
            map[1] = null;
            Assert.AreEqual(1, map.Count);
            Assert.True(map.ContainsKey(1));
            Assert.IsNull(map[1]);
        }

        [Test]
        public void ShouldSucceedTryingGet() {
            var map = new BoundedMap<int, string>(3);
            map[1] = "test";
            string test;
            Assert.True(map.TryGetValue(1, out test));
            Assert.AreEqual("test", test);
        }

        [Test]
        public void ShouldFailTryingGet() {
            var map = new BoundedMap<int, string>(3);
            map[1] = "test";
            Assert.False(map.TryGetValue(2, out string _));
        }
    }
}