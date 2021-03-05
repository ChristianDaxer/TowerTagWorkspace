using GameManagement;
using NUnit.Framework;
using UnityEngine;

namespace Tests {
    public class GameRunnerTest {
        [Test]
        public void ShouldWorkWithGameManagerByDefault() {
            var gameRunner = new GameObject().AddComponent<GameRunner>();
            Assert.True(gameRunner.ActiveGameManager is GameManager);
        }
    }
}