using NUnit.Framework;
using TowerTagSOES;
using UnityEngine;

public class SharedControllerTypeTest {
    [Test]
    public void ShouldReturnNullSingletonInstance() {
        var singleton = SharedControllerType.Singleton;
        Resources.UnloadAsset(singleton);
        Assert.Null(SharedControllerType.Singleton);
    }
}