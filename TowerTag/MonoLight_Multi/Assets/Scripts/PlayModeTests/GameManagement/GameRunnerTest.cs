using System.Collections;
using System.Collections.Generic;
using GameManagement;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameRunnerTest {
    /// <summary>
    /// Test that the test runner calls tick on the active game manager once every frame.
    /// </summary>
    [UnityTest]
    public IEnumerator ShouldTick() {
        var gameRunner = new GameObject().AddComponent<GameRunner>();
        var gameManager = Substitute.For<IGameManager>();
        gameRunner.ActiveGameManager = gameManager;

        gameManager.DidNotReceive().Tick();
        yield return null;
        gameManager.Received().Tick();
    }
}