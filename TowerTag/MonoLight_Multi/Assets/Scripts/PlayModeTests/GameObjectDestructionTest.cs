using System;
using System.Collections;
using NUnit.Framework;
using TowerTag;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class GameObjectDestructionTest {
    private interface ITestClass {
        bool IsValid { get; }
    }

    public class TestClass : MonoBehaviour, ITestClass {
        public bool IsValid => true;

        public void ThrowException() {
            throw new Exception("Meh!");
        }
    }

    [UnityTest]
    public IEnumerator TestGameObjectDestructionBehaviour() {
        var testClass = new GameObject().AddComponent<TestClass>();
        var iTestClass = (ITestClass) testClass;
        Object.Destroy(testClass.gameObject);

        yield return null;

        Assert.True(testClass == null); // unity's null check override
        Assert.AreNotEqual(null, testClass); // testClass is not actually null
        Assert.False(iTestClass == null); // interface does not use unity's null-check override
        Assert.AreEqual(testClass, iTestClass); // the two are still equal
        Assert.True(iTestClass.IsValid); // this does still work, because IsValid does not use the GO instance
        Assert.True(testClass.IsValid); // this too
//        Debug.Log(testClass.gameObject); // this does not work.
        testClass.CheckForNull()?.ThrowException(); // works. throwException is not called
        // testClass?.ThrowException(); // this calls ThrowException
    }

    [UnityTest]
    public IEnumerator TestPlayerDestruction() {
        IPlayer player = new GameObject().AddComponent<Player>();
        Assert.True(player.IsValid);

        Object.Destroy(player.GameObject);
        yield return null;

        Assert.False(player.IsValid);
    }
}