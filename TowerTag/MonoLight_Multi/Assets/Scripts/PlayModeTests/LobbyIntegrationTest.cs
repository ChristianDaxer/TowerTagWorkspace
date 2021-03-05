using System.Collections;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;
using Photon.Realtime;

[TestFixture, Category("Tower Tag")]
public class LobbyIntegrationTest : TowerTagTest {
    private bool _homeSettingAtStart;

    [SetUp]
    public new void SetUp() {
        base.SetUp();
        _homeSettingAtStart = TowerTagSettings.Home;
        TowerTagSettings.Home = true;
    }

    [TearDown]
    public new void TearDown() {
        base.TearDown();
        TowerTagSettings.Home = _homeSettingAtStart;
    }

    //[UnityTest]
    //public IEnumerator ShouldUpdateRoom() {
        //// Given
        //var mockPhotonService = Substitute.For<IPhotonService>();
        //ServiceProvider.Set(mockPhotonService);
        //var mockRoom = Substitute.For<IRoom>();

        //// When starting game
        //SceneManager.LoadSceneAsync(0);
        //GameObject connectButton = null;
        //yield return new WaitUntil(() => (connectButton = GameObject.Find("ConnectButton")) != null);

        //var connectionMangerHome = Object.FindObjectOfType<ConnectionManagerHome>();
        //mockRoom.PlayerCount.Returns(1);
        //mockRoom.Name.Returns("room");
        ////mockRoom.PlayerCount.Returns((byte)1);
        ////mockRoom.Name.Returns("room");
        //connectionMangerHome.OnRoomListUpdate(new List<RoomInfo> { mockRoom });
        ////mockRoom.PlayerCount.Returns((byte)2);
        ////room.SetCustomProperties(hashtable);
        //connectionMangerHome.OnRoomListUpdate(new List<RoomInfo> { mockRoom as Room });
        //yield return null;

        //// Then should find mode dependent extension
        //Assert.IsTrue(true);
    //}
}
