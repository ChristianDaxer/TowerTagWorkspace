using System.Collections;
using NSubstitute;
using NUnit.Framework;
using Photon.Realtime;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

[TestFixture, Category("Tower Tag")]
public class ConnectIntegrationTest : TowerTagTest {

    [UnityTest]
    public IEnumerator ShouldConnect() {
        // Given
        var mockPhotonService = Substitute.For<IPhotonService>();
        ServiceProvider.Set(mockPhotonService);
        SharedControllerType.Singleton.Set(this, ControllerType.Admin);

        // When starting game
        SceneManager.LoadSceneAsync(0);
        GameObject connectButton = null;
        yield return new WaitUntil(() => (connectButton = GameObject.Find("ConnectButton")) != null);
        ConfigurationManager.Configuration.Room = "testRoom";
        if (!TowerTagSettings.Home) {
            connectButton.GetComponent<Button>().OnPointerClick(new PointerEventData(EventSystem.current));
        }

        yield return null;

        // Then should connect
        mockPhotonService.Received().ConnectUsingSettings();
    }

    [UnityTest]
    public IEnumerator ShouldAddModeDependentConnectionManager() {
        // Given
        var mockPhotonService = Substitute.For<IPhotonService>();
        ServiceProvider.Set(mockPhotonService);

        // When starting game
        SceneManager.LoadSceneAsync(0);
        GameObject connectButton = null;
        yield return new WaitUntil(() => (connectButton = GameObject.Find("ConnectButton")) != null);

        yield return null;

        // Then should find mode dependent extension
        if(TowerTagSettings.Home) {
            Assert.IsTrue(Object.FindObjectOfType<ConnectionManagerHome>());
        }
        else {
            Assert.IsTrue(Object.FindObjectOfType<ConnectionManagerPro>());
        }
    }

    [UnityTest]
    public IEnumerator ShouldStartMatchmakingOnConnection() {
        // Given
        var mockPhotonService = Substitute.For<IPhotonService>();
        ServiceProvider.Set(mockPhotonService);
        var mockMatchMaker = Substitute.For<IMatchMaker>();
        ServiceProvider.Set(mockMatchMaker);
        SharedControllerType.Singleton.Set(this, ControllerType.Admin);

        // When starting game
        SceneManager.LoadSceneAsync(0);
        GameObject connectButton = null;
        yield return new WaitUntil(() => (connectButton = GameObject.Find("ConnectButton")) != null);
        ConfigurationManager.Configuration.Room = "testRoom";
        connectButton.GetComponent<Button>().OnPointerClick(new PointerEventData(EventSystem.current));

        yield return null;

        // simulate established connection
        mockPhotonService.IsConnectedAndReady.Returns(true);
        mockPhotonService.NetworkClientState.Returns(ClientState.ConnectedToMasterServer);
        Object.FindObjectOfType<ConnectionManagerPro>().OnConnectedToMaster();

        // Then should start "matchmaking"
        mockMatchMaker.Received().StartMatchMaking();
    }

    [UnityTest, Timeout(15000)]
    public IEnumerator ShouldLoadHubAfterJoiningRoom() {
        var mockPhotonService = Substitute.For<IPhotonService>();
        yield return StartOperator(mockPhotonService);
        yield return new WaitUntil(() => TTSceneManager.Instance.IsInHubScene);
    }

    [UnityTest, Timeout(30000)]
    public IEnumerator ShouldInitializeMatch() {
        yield return null;
        var mockPhotonService = Substitute.For<IPhotonService>();
        yield return StartOperator(mockPhotonService);

        GameObject startMatchButton = null;
        yield return new WaitUntil(() =>
            TTSceneManager.Instance.IsInHubScene && (startMatchButton = GameObject.Find("StartMatchButton")) != null);
        var matchInitialized = false;

        startMatchButton.GetComponent<Button>().OnPointerClick(new PointerEventData(EventSystem.current));
        yield return new WaitUntil(() => GameManager.Instance.CurrentMatch != null);
        GameManager.Instance.CurrentMatch.Initialized += match => matchInitialized = true;

        yield return new WaitUntil(() => matchInitialized);
    }
}