using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class LobbyManagementTest {
    private bool _homeAtStart;
    [SetUp]
    public void SetUp() {
        _homeAtStart = TowerTagSettings.Home;
        TowerTagSettings.Home = true;
    }

    [TearDown]
    public void TearDown() {
        TowerTagSettings.Home = _homeAtStart;
    }

    [Test]
    public void ShouldUpdateRoomInfo() {

    }
}
