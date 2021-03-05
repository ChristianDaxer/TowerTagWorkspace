using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using IPlayer = TowerTag.IPlayer;

namespace Network
{
    /// <summary>
    /// Class to control the voice chat settings for the local client (don't needed on remotes).
    /// Player can speak to one channel and listen to multiple channels.
    /// </summary>
    public class VoiceChatPlayer : MonoBehaviour
    {
        [SerializeField] private bool _debugMode;

        public event Action<ChatType> ConversationGroupChanged;

        #region Singleton

        public static VoiceChatPlayer Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            if (_player != null)
            {
                _player.PlayerTeamChanged -= OnTeamChanged;
            }
        }

        #endregion

        /// <summary>
        /// channel IDs: TeamChannels are given by <see cref="VoiceChatPlayer.GetTeamChannel"/>
        /// </summary>
        private enum Channel
        {
            DefaultChannel = 0, // unused
            AdminOnly = 1, // only listened to by admin
            AdminIndividual = 2, // admin speaking to selected players
            AdminBroadcast = 3, // admin speaking to all
            AllPlayers = 4, // all players
            None = 5, // nobody
            Count = 6
        }

        private static byte GetTeamChannel(TeamID teamID)
        {
            return (byte) (Channel.Count + 1 + (int) teamID);
        }

        /// <summary>
        /// VoiceChat states
        /// </summary>
        public enum ChatType
        {
            TalkToAll = 0,
            TalkInTeam = 1,
            TalkToOperator = 2,
            EmergencyAnnounce = 3,
            Pause = 4,
            Count = 5
        }

        /// <summary>
        /// are we a normal player or the admin?
        /// </summary>
        public enum Role
        {
            NotDefined = 0,
            Admin = 1,
            Player = 2
        }

        /// <summary>
        /// The VoiceRecorder used to record voice and send to other players.
        /// </summary>
        [SerializeField, ReadOnly] private Recorder _recorder;

        /// <summary>
        /// Player reference (used to get TeamID from).
        /// </summary>
        /// [SerializeField, ReadOnly]
        private IPlayer _player;

        /// <summary>
        /// Are we a normal Player or the Admin?
        /// </summary>
        [SerializeField, ReadOnly] private Role _voiceChatRole = Role.NotDefined;

        /// <summary>
        /// Did we initialized successfully yet?
        /// </summary>
        public bool IsInitialized { get; private set; }

        private byte[] ChannelsToListenTo { get; set; }

        /// <summary>
        /// Cache used listenToChannels before we reset the in MuteOthers() (used to reset afterwards).
        /// </summary>
        [SerializeField, ReadOnly] private byte[] _channelsToListenToBeforeMuteOthers;

        /// <summary>
        /// Are we in Paused state?
        /// </summary>
        [SerializeField, ReadOnly] private bool _isPaused;

        /// <summary>
        /// Current state of VoiceChat
        /// </summary>
        [SerializeField, ReadOnly] private ChatType _currentChatType;

        private ChatType CurrentChatType => _currentChatType;

        /// <summary>
        /// Cache VoiceChatState before we enter Pause state (to switch back afterwards).
        /// </summary>
        [HideInInspector] [SerializeField, ReadOnly]
        private ChatType _chatTypeBeforePaused;

        /// <summary>
        /// Cache ChatType when activating operator broadcast.
        /// </summary>
        [SerializeField, ReadOnly] private ChatType _chatTypeBeforeBroadcast;

        /// <summary>
        /// Cache VoiceChatState before we enter Emergency state (to switch back afterwards).
        /// </summary>
        [SerializeField, ReadOnly] private ChatType _chatTypeBeforeEmergencyStop;

        [SerializeField, ReadOnly] private ChatType _chatTypeBeforeDirectOperatorChat;

        private readonly List<int> _directChannelPlayerIDs = new List<int>();

        private void OnEnable()
        {
            PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
            GameManager.Instance.FullSyncRequestCompleted += OnFullSyncRequestCompleted;
        }

        private void OnDisable()
        {
            PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
            AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
            GameManager.Instance.FullSyncRequestCompleted -= OnFullSyncRequestCompleted;
        }

        private void OnTeamChanged(IPlayer player, TeamID newTeam)
        {
            //Debug.LogError("Test Voice Chat Player On Team Changed");
            if (_currentChatType == ChatType.TalkInTeam && _voiceChatRole == Role.Player)
                ChangeConversationGroups(_currentChatType);
        }

        private void OnPlayerRemoved(IPlayer player)
        {
            if (_directChannelPlayerIDs.Remove(player.PlayerID)
                && PhotonNetwork.IsMasterClient
                && _directChannelPlayerIDs.Count == 0)
            {
                ChangeConversationGroups(_chatTypeBeforeDirectOperatorChat);
            }
        }

        private void OnFullSyncRequestCompleted(IMatch obj)
        {
            //Debug.LogError("VoicechatPlayer: Full sync request completed. Check Team Channel");
            if (_currentChatType == ChatType.TalkInTeam && _voiceChatRole == Role.Player)
                ChangeConversationGroups(CurrentChatType);
        }

        /// <summary>
        /// Init VoiceChat channels.
        /// </summary>
        /// <param name="voiceChatRole">Are we a Player or the Admin.</param>
        /// <param name="recorder">The Recorder to record the voice to transmit voice over network.</param>
        /// <param name="player">If we are a Player we need the Player object to grab the Team/TeamChannel to talk to.</param>
        public void Init(Role voiceChatRole, Recorder recorder, IPlayer player)
        {
            if (recorder == null)
            {
                Debug.LogError("Cannot init VoiceChat: VoiceRecorder is null!");
                return;
            }

            if (voiceChatRole == Role.Player)
            {
                if (player == null)
                {
                    Debug.LogError("Cannot init VoiceChat: Player is null!");
                    return;
                }

                _player = player;
            }

            // Init with values:
            _voiceChatRole = voiceChatRole;
            _recorder = recorder;

            Configuration configuration = ConfigurationManager.Configuration;

            SetMicrophoneVoiceDetectionThreshold(configuration.TeamVoiceChatVoiceDetectionThreshold);
            SetMicrophone(configuration.TeamVoiceChatMicrophone);
            if (_recorder.IsInitialized)
            {
                _recorder.RestartRecording();
            }
            else
            {
                _recorder.Init(PhotonVoiceNetwork.Instance.VoiceClient, _recorder.UserData);
            }

            // if we are admin and no emergency event occured -> mute the mic
            MuteMicrophone(voiceChatRole == Role.Admin);

            if (!ConfigurationManager.Configuration.TeamVoiceChatEnableVoiceChat)
            {
                PhotonVoiceNetwork.Instance.AutoConnectAndJoin = false;
                PhotonVoiceNetwork.Instance.Disconnect();
            }

            IsInitialized = true;
            if (_player != null)
            {
                _player.PlayerTeamChanged += OnTeamChanged;
                OnTeamChanged(_player, _player.TeamID);
            }

            // log current state
            PrintCurrentPUNVoiceState("VoiceChatPlayer.Init: ");
        }

        /// <summary>
        /// Re-initializes the recorder microphone when the output has changed due to windows audio changes
        /// </summary>
        /// <param name="deviceWasChanged"></param>
        private void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
            if (deviceWasChanged && _recorder != null)
            {
                //Hack to re-init the mic
                int bitrate = _recorder.Bitrate;
                _recorder.Bitrate = 1;
                _recorder.Bitrate = bitrate;
                _recorder.UnityMicrophoneDevice = ConfigurationManager.Configuration.TeamVoiceChatMicrophone;
                _recorder.RestartRecording();
            }
        }

        /// <summary>
        /// Set Microphone to use with PhotonVoiceChat.
        /// Attention, sometimes Unity crashes if u use this Function.
        /// </summary>
        /// <param name="microphoneToUse">Name of the Microphone we want to use (see Microphone.devices).</param>
        private void SetMicrophone(string microphoneToUse)
        {
            if (_recorder != null)
            {
                _recorder.UnityMicrophoneDevice = microphoneToUse;
            }
            else
            {
                Debug.LogError("Cannot set microphone: recorder is null");
            }
        }

        /// <summary>
        /// Wrapper function for PhotonVoice VoiceDetector.
        /// Threshold: Voice detected as soon as signal level exceeds threshold.
        /// </summary>
        /// <param name="threshold">Threshold to detect voice.</param>
        private void SetMicrophoneVoiceDetectionThreshold(float threshold)
        {
            if (_recorder != null && _recorder.VoiceDetector != null)
            {
                _recorder.VoiceDetector.Threshold = threshold;
            }
            else
            {
                Debug.LogError("Cannot set Microphone threshold: Recorder or VoiceDetector is null");
            }
        }

        /// <summary>
        /// Set the channel the Player speaks to (see Channels enum).
        /// </summary>
        /// <param name="channelToSpeakTo">Channel the Player should speak to (see Channels enum).</param>
        private void SpeakToChannel(byte channelToSpeakTo)
        {
            if (_recorder != null)
            {
                _recorder.InterestGroup = channelToSpeakTo;
            }
            else
            {
                Debug.LogError("Cannot set channel, because recorder is null");
            }
        }

        /// <summary>
        /// Activate/Deactivate Transmission of VoiceRecorder.
        /// </summary>
        /// <param name="mute"></param>
        private void MuteMicrophone(bool mute)
        {
            if (_recorder != null)
            {
                _recorder.TransmitEnabled = !mute;
                _recorder.VoiceDetection = !mute;
            }
            else
            {
                Debug.LogError("Cannot mute microphone, because recorder is null");
            }
        }

        /// <summary>
        /// Sets the channels this Player should listen to.
        /// </summary>
        /// <param name="channelsToListenTo">Channels the Player should listen to.
        /// (Null will not add any. A byte[0] will add all current.)</param>
        private void ListenToChannels(byte[] channelsToListenTo)
        {
            // cache channels for un-muting
            ChannelsToListenTo = channelsToListenTo;
            StartCoroutine(ChangeListenToChannelsCoroutine(channelsToListenTo));
        }

        private IEnumerator ChangeListenToChannelsCoroutine(byte[] channels, float timeout = 5f,
            float retryDelay = 0.1f)
        {
            if (!ConfigurationManager.Configuration.TeamVoiceChatEnableVoiceChat) yield break;

            float started = Time.time;
            while (Time.time - started < timeout && !IsPUNVoiceConnectedCorrectly())
            {
                yield return new WaitForSeconds(retryDelay);
            }

            while (Time.time - started < timeout &&
                   PhotonVoiceNetwork.Instance.Client.LoadBalancingPeer.PeerState != PeerStateValue.Connected)
            {
                yield return new WaitForSeconds(retryDelay);
            }

            if (!IsPUNVoiceConnectedCorrectly())
            {
                Debug.LogWarning($"Not connected even after timeout of {timeout} seconds. Voice chat will not work");
                yield break;
            }

            if (PhotonVoiceNetwork.Instance.Client.LoadBalancingPeer.OpChangeGroups(new byte[0], channels))
            {
                ChannelsToListenTo = channels;
                yield break;
            }

            Debug.LogError("VoiceChatPlayer.ListenToChannels: Request to change Audio groups was not enqueued!");
        }

        /// <summary>
        /// Sets channels to listen to (none if muted, cached otherwise).
        /// Attention: even if muted: channel 0 is always on (to hear).
        /// </summary>
        /// <param name="mute">Mute or not!</param>
        private void MuteOtherPlayers(bool mute)
        {
            if (mute)
            {
                _channelsToListenToBeforeMuteOthers = ChannelsToListenTo;
                StartCoroutine(ChangeListenToChannelsCoroutine(null));
            }
            else
            {
                StartCoroutine(ChangeListenToChannelsCoroutine(_channelsToListenToBeforeMuteOthers));
            }

            PrintCurrentPUNVoiceState("VoiceChatPlayer.MuteOtherPlayers: " + mute);
        }

        /// <summary>
        /// Open a direct voice link between operator and a player.
        /// </summary>
        public void OpenDirectChannelToOperator(IPlayer player)
        {
            _directChannelPlayerIDs.Add(player.PlayerID);
            if (CurrentChatType != ChatType.TalkToOperator) _chatTypeBeforeDirectOperatorChat = CurrentChatType;
            ChangeConversationGroups(ChatType.TalkToOperator);
            if (SharedControllerType.IsAdmin)
            {
                AdminController.Instance.ToggleTalkToIcon(player, true);
                player.ToggleDirectAdminChatOnMaster(true);
            }
        }

        /// <summary>
        /// Close a direct voice link between operator and a player.
        /// </summary>
        public void CloseDirectChannelToOperator(IPlayer player)
        {
            _directChannelPlayerIDs.Remove(player.PlayerID);
            if (!PhotonNetwork.IsMasterClient || _directChannelPlayerIDs.Count == 0)
            {
                ChangeConversationGroups(_chatTypeBeforeDirectOperatorChat);
            }

            if (SharedControllerType.IsAdmin)
            {
                AdminController.Instance.ToggleTalkToIcon(player, false);
                player.ToggleDirectAdminChatOnMaster(false);
            }
        }

        /// <summary>
        /// Switch conversation channels (speakTo & ListenTo) appropriate to the given chatType.
        /// </summary>
        /// <param name="chatType">Type of conversation.</param>
        /// <param name="teamChannel">Channel of the Team we switch to if we want to talk with teammates.</param>
        private void ChangeConversationGroups(ChatType chatType, byte teamChannel)
        {
            // don't override emergency stop
            if (_currentChatType == ChatType.EmergencyAnnounce)
            {
                Debug.LogWarning("Cannot change conversation group: current chat type is emergency");
                return;
            }

            // don't override pause (except it's an emergency stop)
            if (_currentChatType == ChatType.Pause && chatType != ChatType.EmergencyAnnounce)
            {
                Debug.LogWarning("Cannot change conversation group: current chat type is pause");
                return;
            }

            print($"Changing conversation group {chatType} Role: {_voiceChatRole} TeamChannel: {teamChannel}");

            if (CurrentChatType == ChatType.TalkToOperator && chatType != ChatType.TalkToOperator)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    new List<int>(_directChannelPlayerIDs) // copy to make editable during iteration
                        .Select(PlayerManager.Instance.GetPlayer)
                        .ForEach(CloseDirectChannelToOperator);
                }
            }

            // cache new (current) state to resume from pause later on
            _currentChatType = chatType;

            // if we are a player (client)
            if (_voiceChatRole == Role.Player)
            {
                switch (chatType)
                {
                    case ChatType.TalkToAll:
                    case ChatType.Pause:
                        SpeakToChannel((byte) Channel.AllPlayers);
                        ListenToChannels(new[]
                            {(byte) Channel.DefaultChannel, (byte) Channel.AllPlayers, (byte) Channel.AdminBroadcast});
                        break;
                    case ChatType.TalkInTeam:
                        SpeakToChannel(teamChannel);
                        ListenToChannels(new[] {teamChannel, (byte) Channel.AdminBroadcast});
                        break;
                    case ChatType.EmergencyAnnounce:
                        // only admin can talk, all other have to listen only to him/her ;)
                        SpeakToChannel((byte) Channel.None); // talk to the hand
                        ListenToChannels(new[] {(byte) Channel.AdminBroadcast});
                        break;
                    case ChatType.TalkToOperator:
                        SpeakToChannel((byte) Channel.AdminOnly);
                        ListenToChannels(new[] {(byte) Channel.AdminIndividual});
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(chatType), chatType, null);
                }
            }
            // if we are the admin
            else if (_voiceChatRole == Role.Admin)
            {
                switch (chatType)
                {
                    case ChatType.EmergencyAnnounce:
                        MuteMicrophone(false);
                        SpeakToChannel((byte) Channel.AdminBroadcast);
                        ListenToChannels(null);
                        break;
                    case ChatType.TalkToAll:
                    case ChatType.TalkInTeam:
                        SpeakToChannel((byte) Channel.AdminBroadcast);
                        // listen to all if we un-mute (pushToTalk)
                        ListenToChannels(new[]
                            {
                                (byte) Channel.DefaultChannel, (byte) Channel.AllPlayers, (byte) Channel.AdminBroadcast,
                                (byte) Channel.AdminOnly
                            }
                            .Concat(GetAllTeamChannels())
                            .ToArray());
                        MuteMicrophone(true); // speak with push to talk
                        MuteOtherPlayers(true);
                        break;
                    case ChatType.Pause:
                        // pause -> same as emergency except the admin is listening to players
                        MuteMicrophone(false);
                        SpeakToChannel((byte) Channel.AdminBroadcast);
                        ListenToChannels(new[]
                        {
                            (byte) Channel.AllPlayers, (byte) Channel.AdminBroadcast, (byte) Channel.AdminOnly
                        });
                        break;
                    case ChatType.TalkToOperator:
                        MuteMicrophone(false);
                        SpeakToChannel((byte) Channel.AdminIndividual);
                        ListenToChannels(new[] {(byte) Channel.AdminOnly});
                        break;
                    default:
                        MuteMicrophone(true);
                        break;
                }
            }

            ConversationGroupChanged?.Invoke(chatType);
        }

        /// <summary>
        /// Returns teamChannels of all (in TeamManager) available Teams.
        /// </summary>
        /// <returns>Returns teamChannels of all (in TeamManager) available Teams, empty array if non available
        /// (check for console messages).</returns>
        private static byte[] GetAllTeamChannels()
        {
            return new[] {(byte) (Channel.Count + 1), (byte) (Channel.Count + 2)};
        }


        /// <summary>
        /// PushToTalk functionality switches Muting of Microphone and other Players so the admin can only here and
        /// speak when this function is toggled on!
        /// Attention: does not work when we are in EmergencyState!!!
        /// </summary>
        /// <param name="talk">True: if we want to speak and Listen to other players,
        /// False: Mute Microphone and other players.</param>
        public void TogglePushToTalkOnOff(bool talk)
        {
            if (_currentChatType == ChatType.EmergencyAnnounce) // || _currentChatType == ChatType.pause)
                return;

            if (_voiceChatRole != Role.Admin)
            {
                Debug.LogError("VoiceChatPlayer.PushToTalk: don't use this if u are not the Admin!");
                return;
            }

            if (talk)
            {
                _chatTypeBeforeBroadcast = CurrentChatType;
                ChangeConversationGroups(ChatType.TalkToAll);
                MuteMicrophone(false);
                MuteOtherPlayers(false);
            }
            else
            {
                ChangeConversationGroups(_chatTypeBeforeBroadcast);
                MuteMicrophone(true);
                MuteOtherPlayers(true);
            }

            // log current state
            PrintCurrentPUNVoiceState("VoiceChatPlayer.TogglePushToTalkOnOff: " + talk);
        }

        /// <summary>
        /// Function to set VoiceChat to Pause mode (all players can talk to each other and the admin).
        /// Attention: does not work when we are in EmergencyState!!!
        /// </summary>
        /// <param name="pause">True: if you want to switch to VoiceChatPauseMode, False if you want to resume to the
        /// last VoiceChatMode (which was set before pausing).</param>
        public void Pause(bool pause)
        {
            if (_currentChatType == ChatType.EmergencyAnnounce)
                return;

            if (pause != _isPaused)
            {
                _isPaused = pause;

                // pause
                if (pause)
                {
                    _chatTypeBeforePaused = CurrentChatType;
                    ChangeConversationGroups(ChatType.Pause);
                }
                // unPause -> reset to last state
                else
                {
                    _currentChatType = ChatType.Count;
                    ChangeConversationGroups(_chatTypeBeforePaused);
                }
            }

            // log current state
            PrintCurrentPUNVoiceState("VoiceChatPlayer.Pause: " + pause);
        }

        /// <summary>
        /// Set currentState to Emergency Announcement (admin talks to all, no player can talk to admin or each other)!!!
        /// </summary>
        /// <param name="setEmergency">True if you want to set to Emergency State, false when you want to resume from
        /// emergency and go back to last state (before emergency).</param>
        public void SetEmergency(bool setEmergency)
        {
            if (setEmergency)
            {
                // remember old state to resume from emergencyStop
                _chatTypeBeforeEmergencyStop = _currentChatType;
                ChangeConversationGroups(ChatType.EmergencyAnnounce);
            }
            else
            {
                _currentChatType = ChatType.Count; // otherwise chat type cannot be changed from emergency mode
                ChangeConversationGroups(_chatTypeBeforeEmergencyStop);
            }

            // log current state
            PrintCurrentPUNVoiceState("VoiceChatPlayer.SetEmergency: " + setEmergency);
        }

        /// <summary>
        /// Switch conversation channels (speakTo & ListenTo) appropriate to the given chatType.
        /// </summary>
        /// <param name="chatType">Type of conversation.</param>
        public void ChangeConversationGroups(ChatType chatType)
        {
            //Debug.LogError("Test Voice Chat Player Change conversation group " + chatType);

            if (!IsInitialized)
            {
                Debug.LogError("Can't change channels: VoiceChatPlayer is not initialized");
                return;
            }

            if (_voiceChatRole == Role.Player && _player == null)
            {
                Debug.LogError("Can't change channels: Player is null");
                return;
            }

            // TODO: refactor
            // 1vs1 can talk to each other q&d hack
            if (chatType == ChatType.TalkInTeam && GameManager.Instance.CurrentMatch != null
                                                && !GameManager.Instance.CurrentMatch.MatchStarted) 
			{
				print($"VoiceChatPlayer:ChangeConversationGroup -> find {PlayerManager.Instance.GetAllParticipatingHumanPlayerCount()} participating player(s)");
                if (PlayerManager.Instance.GetAllParticipatingHumanPlayerCount() <= 2)
                {
                    chatType = ChatType.TalkToAll;
                    print("1 vs 1 -> set voice chat type to " + chatType);
                }
            }

            PrintCurrentPUNVoiceState("VoiceChatPlayer.ChangeConversationGroups");

            // if we are a player -> use our teamChannel
            if (_voiceChatRole == Role.Player)
            {
                ChangeConversationGroups(chatType, GetTeamChannel(_player.TeamID));
            }
            // if we are admin -> talk to the hand ( ..erm adminChannel i mean)
            else if (_voiceChatRole == Role.Admin)
            {
                ChangeConversationGroups(chatType, (byte) Channel.AdminBroadcast);
            }

            // log current state
            PrintCurrentPUNVoiceState("VoiceChatPlayer.ChangeConversationGroups");
        }

        /// <summary>
        /// Prints internal State to logFile/console
        /// </summary>
        /// <param name="callInfo">Info when and by whom this Function is called to relate Messages to Game flow when debugging.</param>
        private void PrintCurrentPUNVoiceState(string callInfo)
        {
            if (!_debugMode)
                return;

            var sBuilder = new StringBuilder();
            sBuilder.AppendLine("***** VoiceChat State *****");
            sBuilder.Append("CallInfo: " + (callInfo ?? "-"));
            sBuilder.AppendLine(" -> Current ChatType: " + _currentChatType);

            // print client Info
            if (_voiceChatRole == Role.Admin)
            {
                sBuilder.AppendLine("Admin:");
            }
            else if (_voiceChatRole == Role.Player && _player != null)
            {
                sBuilder.AppendLine("Player: " + _player.PlayerName + " playerID: " +
                                    (_player.IsPhotonViewValid ? _player.PlayerID.ToString() : "-"));
            }

            // Print current state
            sBuilder.AppendLine("PUN client connected and ready: " + PhotonNetwork.IsConnectedAndReady +
                                ", detailed: " +
                                PhotonNetwork.NetworkClientState);
            sBuilder.AppendLine("PUNVoice client state: " + PhotonVoiceNetwork.Instance.ClientState);
            sBuilder.AppendLine("PUNVoice current voice room: " + PhotonNetwork.CurrentRoom);

            if (_recorder != null)
            {
                // print recorder info
                float lastVoiceDetectedInSeconds = DateTime.Now.Subtract(_recorder.VoiceDetector.DetectedTime).Seconds;
                sBuilder.AppendLine("PUNVoice Recorder Transmit: " + _recorder.TransmitEnabled + "(" +
                                    _recorder.IsCurrentlyTransmitting + "), Detect: " + _recorder.VoiceDetection + "(" +
                                    _recorder.VoiceDetector.Detected + "), last detected: " +
                                    lastVoiceDetectedInSeconds +
                                    " s ago");

                // print Mic
                sBuilder.AppendLine("PUNVoice Recorder Mic: " + (_recorder.UnityMicrophoneDevice ?? "-"));

                // print channels
                sBuilder.AppendLine("PunVoice client speaksTo channel: " + _recorder.InterestGroup);
                sBuilder.AppendLine("PunVoice client listensTo channels (cached): " +
                                    HelperFunctions.PrintArrayToString(ChannelsToListenTo));
            }
            else
                Debug.LogError("VoiceChatPlayer.PrintCurrentPUNVoiceState: Recorder is null");

            Debug.Log(sBuilder.ToString());
        }

        /// <summary>
        /// Convenience function to evaluate if we are connected correctly
        /// </summary>
        /// <returns></returns>
        private static bool IsPUNVoiceConnectedCorrectly()
        {
            // Photon does no Null checks so we get NullRefExceptions if we try to access the PUNVoice.Client when PhotonVoiceNetwork.instance is null
            if (PhotonVoiceNetwork.Instance == null
                || PhotonVoiceNetwork.Instance.Client == null
                || PhotonVoiceNetwork.Instance.Client.State != ClientState.Joined)
                return false;

            // we can only change AudioGroups if we are connected (PUNVoice tries to send AudioGroup to Server)
            if (PhotonVoiceNetwork.Instance.Client.IsConnected)
            {
                return true;
            }

            return false;
        }

        private void OnGUI()
        {
            if (!_debugMode) return;
            if (GUILayout.Button("Disconnect voice"))
            {
                PhotonVoiceNetwork.Instance.Disconnect();
            }

            if (GUILayout.Button("Connect and join voice"))
            {
                Debug.LogError(PhotonVoiceNetwork.Instance.ConnectAndJoinRoom());
            }

            if (GUILayout.Button("Check connection"))
            {
                Debug.LogError(IsPUNVoiceConnectedCorrectly());
            }

            if (GUILayout.Button("Listen to all channels"))
            {
                ListenToChannels(GetAllTeamChannels());
            }

            if (GUILayout.Button("Talk to all"))
            {
                ChangeConversationGroups(ChatType.TalkToAll);
            }
        }
    }
}