using System.Collections;
using System.Linq;
using System.Reflection;
using GameManagement;
using NSubstitute;
using NUnit.Framework;
using Photon.Pun;
using Photon.Realtime;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using IPlayer = Photon.Realtime.IPlayer;
using Player = TowerTag.Player;

[TestFixture, Category("Tower Tag")]
public abstract class TowerTagTest {
    [SetUp]
    public void SetUp() {
        ServiceProvider.Clear();
    }

    private GameInitialization gameInitialization;
    private GameInitialization GameInitializationInstance
    {
        get
        {
            if (gameInitialization == null)
            {
                if (!GameInitialization.GetInstance(out gameInitialization))
                    return null;
            }

            return gameInitialization;
        }
    }

    [TearDown]
    public void TearDown() {
        Debug.Log("TEAR DOWN");
        if (gameInitialization != null)
            Object.Destroy(gameInitialization.gameObject);
        Object.FindObjectsOfType<GameObject>()
            .Where(go => go.transform.parent == null)
            .ForEach(Object.Destroy);
        ServiceProvider.Clear();
    }

    protected IEnumerator StartOperator(IPhotonService mockPhotonService) {
        ServiceProvider.Set(mockPhotonService);
        ServiceProvider.Set(Substitute.For<IMatchMaker>());

        SharedControllerType.Singleton.Set(this, ControllerType.Admin);

        yield return SceneManager.LoadSceneAsync(0);
        var gameInit = Object.FindObjectOfType<GameInitialization>();
        var gameManagerPhotonView = ((GameObject) Resources.Load("MockGameManagerPhotonView"))
            .GetComponent<GameManagerPhotonView>();
        gameInit.GetType().GetField("_gameManagerPhotonViewPrefab", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(gameInit, gameManagerPhotonView);

        // connect when button appears
        GameObject connectButton = null;
        yield return new WaitUntil(() => (connectButton = GameObject.Find("ConnectButton")) != null);
        ConfigurationManager.Configuration.Room = "testRoom";
        connectButton.GetComponent<Button>().OnPointerClick(new PointerEventData(EventSystem.current));
        yield return null;

        // directly execute network spawn requests
        mockPhotonService.When(x =>
                x.InstantiateSceneObject(Arg.Any<string>(), Arg.Any<Vector3>(), Arg.Any<Quaternion>()))
            .Do(x => {
                Object prefab = Resources.Load((string) x.Args()[0]);
                Object.Instantiate(prefab, (Vector3) x.Args()[1], (Quaternion) x.Args()[2]);
            });
        mockPhotonService
            .Instantiate(Arg.Any<string>(), Arg.Any<Vector3>(), Arg.Any<Quaternion>())
            .Returns(x => {
                Object prefab = Resources.Load((string) x.Args()[0]);
                return Object.Instantiate(prefab, (Vector3) x.Args()[1], (Quaternion) x.Args()[2]);
            });

        // simulate successful connection
        mockPhotonService.IsConnectedAndReady.Returns(true);
        mockPhotonService.IsMasterClient.Returns(true);
        mockPhotonService.ServerTimestamp.Returns(callInfo => (int) (Time.time * 1000));
        mockPhotonService.NetworkClientState.Returns(ClientState.ConnectedToMasterServer);
        mockPhotonService.RoundTripTime.Returns(77);

        // mock room
        var room = Substitute.For<IRoom>();
        room.CustomProperties.Returns(new Hashtable());
        mockPhotonService.CurrentRoom.Returns(room);

        // invoke connection callbacks
        var connectionManager = Object.FindObjectOfType<ConnectionManager>();
        connectionManager.OnConnectedToMaster();
        yield return new WaitUntil(() => TTSceneManager.Instance.IsInConnectScene);
        connectionManager.OnJoinedRoom();

        yield return new WaitUntil(() => TTSceneManager.Instance.IsInHubScene);
    }

    protected IEnumerator StartFPSPlayer(IPhotonService mockPhotonService) {
        ServiceProvider.Set(mockPhotonService);
        ServiceProvider.Set(Substitute.For<IMatchMaker>());

        SharedControllerType.Singleton.Set(this, ControllerType.NormalFPS);

        yield return SceneManager.LoadSceneAsync(0);
        var gameInit = Object.FindObjectOfType<GameInitialization>();
        var mockPlayer = ((GameObject) Resources.Load("MockTT_Player")).GetComponent<Player>();
        gameInit.GetType().GetField("_playerPrefab", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(gameInit, mockPlayer);

        // connect when button appears
        GameObject connectButton = null;
        yield return new WaitUntil(() => (connectButton = GameObject.Find("ConnectButton")) != null);
        ConfigurationManager.Configuration.Room = "testRoom";
        connectButton.GetComponent<Button>().OnPointerClick(new PointerEventData(EventSystem.current));
        yield return null;

        // directly execute network spawn requests
        mockPhotonService.When(x =>
                x.InstantiateSceneObject(Arg.Any<string>(), Arg.Any<Vector3>(), Arg.Any<Quaternion>()))
            .Do(x => {
                Object prefab = Resources.Load((string) x.Args()[0]);
                Object.Instantiate(prefab, (Vector3) x.Args()[1], (Quaternion) x.Args()[2]);
            });
        mockPhotonService
            .Instantiate(Arg.Any<string>(), Arg.Any<Vector3>(), Arg.Any<Quaternion>())
            .Returns(x => {
                Object prefab = Resources.Load((string) x.Args()[0]);
                Object instance = Object.Instantiate(prefab, (Vector3) x.Args()[1], (Quaternion) x.Args()[2]);

                // the first player that is created is the local player
                var player = ((GameObject) instance).GetComponent<Player>();
                if (player != null) {
                    var photonPlayer = Substitute.For<IPlayer>();
                    photonPlayer.ActorNumber.Returns(7);
                    ConfigurePlayer(player, 7001, photonPlayer, TeamID.Ice, true);
                    mockPhotonService.LocalPlayer.Returns(photonPlayer);
                }

                return instance;
            });

        // simulate successful connection
        mockPhotonService.IsConnectedAndReady.Returns(true);
        mockPhotonService.IsMasterClient.Returns(false);
        mockPhotonService.ServerTimestamp.Returns(callInfo => (int) (Time.time * 1000));
        mockPhotonService.NetworkClientState.Returns(ClientState.ConnectedToMasterServer);

        // mock room
        var room = Substitute.For<IRoom>();
        room.CustomProperties.Returns(new Hashtable());
        mockPhotonService.CurrentRoom.Returns(room);
        mockPhotonService.InRoom.Returns(true);

        // invoke connection callbacks
        var connectionManager = Object.FindObjectOfType<ConnectionManager>();
        connectionManager.OnConnectedToMaster();
        yield return new WaitUntil(() => TTSceneManager.Instance.IsInConnectScene);
        connectionManager.OnJoinedRoom();

        mockPhotonService.Instantiate("MockGameManagerPhotonView", Vector3.zero, Quaternion.identity);

        yield return new WaitUntil(() => TTSceneManager.Instance.IsInHubScene);
    }

    protected static Player InstantiateFakePlayer(int id, IPlayer photonPlayer, TeamID teamID, bool isLocal = false) {
        var playerPrefab = (GameObject) Resources.Load("MockTT_Player");
        var player = Object.Instantiate(playerPrefab).GetComponent<Player>();
        ConfigurePlayer(player, id, photonPlayer, teamID, isLocal);
        return player;
    }

    private static void ConfigurePlayer(Player player, int id, IPlayer photonPlayer, TeamID teamID, bool isLocal) {
        photonPlayer.CustomProperties.Returns(new Hashtable {
            {$"{id}_N", "name"},
            {$"{id}_T", teamID}
        });
        IPhotonView photonView = player.GetComponent<PhotonViewWrapper>().Implementation;
        int actorNumber = photonPlayer.ActorNumber; // need to cache or the compiler will optimize mock away
        photonView.Owner.Returns(photonPlayer);
        photonView.IsMine.Returns(isLocal);
        photonView.OwnerActorNr.Returns(actorNumber);
        photonView.ViewID = id;
        player.SetTeam(teamID);
    }
}