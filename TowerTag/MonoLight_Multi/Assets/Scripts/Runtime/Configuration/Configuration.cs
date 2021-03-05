using System;
using UnityEngine;
using System.Runtime.Serialization;

[Serializable]
[DataContract, KnownType(typeof(Vector3)), KnownType(typeof(Vector2))]
public class Configuration {
    [SerializeField]
    [IgnoreDataMember, DataMember(Name = "ControllerType", Order = 0),
     Obsolete("For Serialization backwards compatibility")]
    private string _controllerType;

    // Debug Config -> delete this before deploy
    [SerializeField]
    [DataMember(Name = "Team-ID", Order = 1)]
    private int _teamID;

    public int TeamID {
        get => _teamID;
        set => _teamID = value;
    }

    [SerializeField]
    [DataMember(Name = "RoomName", Order = 2)]
    private string _room = "";

    public string Room {
        get => _room;
        set => _room = value;
    }

    [SerializeField]
    [DataMember(Name = "RegionCode_Cloud", Order = 2)]
    private string _preferredRegion = "best";

    public string PreferredRegion {
        get => _preferredRegion;
        set => _preferredRegion = value;
    }

    [SerializeField]
    [DataMember(Name = "PlayLAN", Order = 3)]
    private bool _playInLocalNetwork;

    public bool PlayInLocalNetwork {
        get => _playInLocalNetwork;
        set => _playInLocalNetwork = value;
    }

    [SerializeField]
    [DataMember(Name = "Server-IP_LAN", Order = 4)]
    private string _serverIp = "127.0.0.1";

    public string ServerIp {
        get => _serverIp;
        set => _serverIp = value;
    }

    [SerializeField]
    [DataMember(Name = "Server-Port_LAN", Order = 5)]
    private int _serverPort = 5055;

    public int ServerPort {
        get => _serverPort;
        set => _serverPort = value;
    }

    [SerializeField]
    [DataMember(Name = "Enable_Gun_Controller", Order = 6)]
    private bool _enableRumbleController;

    public bool EnableRumbleController {
        get => _enableRumbleController;
        set => _enableRumbleController = value;
    }

    [SerializeField]
    [DataMember(Name = "Gun_COM-Port", Order = 7)]
    private string _arduinoComPortName = "COM9";

    [SerializeField]
    public string ArduinoCOMPortName => _arduinoComPortName;

    [SerializeField]
    [DataMember(Name = "PillarPositionOffset_X", Order = 8)]
    private float _pillarPositionOffsetX;

    [SerializeField]
    [DataMember(Name = "PillarPositionOffset_Y", Order = 8)]
    private float _pillarPositionOffsetY;

    [SerializeField]
    [DataMember(Name = "PillarPositionOffset_Z", Order = 8)]
    private float _pillarPositionOffsetZ;

    public Vector3 PillarPositionOffset {
        get => new Vector3(_pillarPositionOffsetX, _pillarPositionOffsetY, _pillarPositionOffsetZ);
        set {
            var pillarPositionOffset =
                new Vector3(_pillarPositionOffsetX, _pillarPositionOffsetY, _pillarPositionOffsetZ);
            if (pillarPositionOffset != value) {
                pillarPositionOffset = value;
                _pillarPositionOffsetX = pillarPositionOffset.x;
                _pillarPositionOffsetY = pillarPositionOffset.y;
                _pillarPositionOffsetZ = pillarPositionOffset.z;
            }
        }
    }

    [SerializeField]
    [DataMember(Name = "pillarRotationOffset", Order = 9)]
    private float _pillarRotationOffsetAngle;

    public float PillarRotationOffsetAngle {
        get => _pillarRotationOffsetAngle;
        set => _pillarRotationOffsetAngle = value;
    }

    [SerializeField]
    [DataMember(Name = "PillarOffset_EnableIngameConfiguration", Order = 10)]
    private bool _ingamePillarOffset;

    public bool IngamePillarOffset {
        get => _ingamePillarOffset;
        set => _ingamePillarOffset = value;
    }

    [SerializeField]
    [DataMember(Name = "TeamVoiceChat_EnableVoiceChat", Order = 14)]
    private bool _teamVoiceChatEnableVoiceChat = true;

    public bool TeamVoiceChatEnableVoiceChat {
        get => _teamVoiceChatEnableVoiceChat;
        set => _teamVoiceChatEnableVoiceChat = value;
    }

    [SerializeField]
    [DataMember(Name = "TeamVoiceChat_Microphone", Order = 15)]
    private string _teamVoiceChatMicrophone;

    public string TeamVoiceChatMicrophone {
        get => _teamVoiceChatMicrophone;
        set => _teamVoiceChatMicrophone = value;
    }

    [SerializeField]
    [DataMember(Name = "TeamVoiceChat_VoiceDetectionThreshold", Order = 16)]
    private float _teamVoiceChatVoiceDetectionThreshold = 0.01f;

    public float TeamVoiceChatVoiceDetectionThreshold {
        get => _teamVoiceChatVoiceDetectionThreshold;
        set => _teamVoiceChatVoiceDetectionThreshold = value;
    }

    [SerializeField]
    [DataMember(Name = "TeamVoiceChat_UsePushToTalk", Order = 17)]
    private bool _teamVoiceChatUsePushToTalk;

    public bool TeamVoiceChatUsePushToTalk {
        get => _teamVoiceChatUsePushToTalk;
        set => _teamVoiceChatUsePushToTalk = value;
    }

    [SerializeField]
    [DataMember(Name = "EnableHapticHitFeedback", Order = 18)]
    private bool _enableHapticHitFeedback;

    public bool EnableHapticHitFeedback {
        get => _enableHapticHitFeedback;
        set => _enableHapticHitFeedback = value;
    }

    [SerializeField]
    [DataMember(Name = "EnableHapticShootFeedback", Order = 19)]
    private bool _enableHapticShootFeedback;

    public bool EnableHapticShootFeedback {
        get => _enableHapticShootFeedback;
        set => _enableHapticShootFeedback = value;
    }

    [SerializeField]
    [DataMember(Name = "MasterVolume", Order = 20)]
    private float _masterVolume = 0.8f;

    public float MasterVolume {
        get => _masterVolume;
        set => _masterVolume = value;
    }

    [SerializeField]
    [DataMember(Name = "MusicVolume", Order = 21)]
    private float _musicVolume = 0.1f;

    public float MusicVolume {
        get => _musicVolume;
        set => _musicVolume = value;
    }

    [SerializeField]
    [DataMember(Name = "SoundFXVolume", Order = 22)]
    private float _soundFxVolume = 0.1f;

    public float SoundFxVolume {
        get => _soundFxVolume;
        set => _soundFxVolume = value;
    }

    [SerializeField]
    [DataMember(Name = "AnnouncerVolume", Order = 23)]
    private float _announcerVolume = 0.15f;

    public float AnnouncerVolume {
        get => _announcerVolume;
        set => _announcerVolume = value;
    }

    [SerializeField]
    [DataMember(Name = "TeammatesVolume", Order = 24)]
    private float _teammatesVolume = 1.0f;

    public float TeammatesVolume {
        get => _teammatesVolume;
        set => _teammatesVolume = value;
    }

    [SerializeField]
    [DataMember(Name = "Screenshot", Order = 25)]
    private int _screenShotResolution = 5;

    public int ScreenShotResolution {
        get => _screenShotResolution;
        set => _screenShotResolution = value;
    }

    [SerializeField]
    [DataMember(Name = "LocationName", Order = 26)]
    private string _locationName = "";

    public string LocationName {
        get => _locationName;
        set => _locationName = string.IsNullOrEmpty(value) ? "" : value;
    }

    [SerializeField]
    [DataMember(Name = "CustomizeColors", Order = 27)]
    private bool _customizeColors;

    public bool CustomizeColors => _customizeColors;

    [SerializeField]
    [DataMember(Name = "FireHue", Order = 28)]
    private ushort _fireHue;

    public ushort FireHue {
        get => _fireHue;
        set => _fireHue = value;
    }

    [SerializeField]
    [DataMember(Name = "IceHue", Order = 29)]
    private ushort _iceHue;

    public ushort IceHue {
        get => _iceHue;
        set => _iceHue = value;
    }

    [SerializeField]
    [DataMember(Name = "CustomizeLogo", Order = 30)]
    private bool _customizeLogo;

    public bool CustomizeLogo => _customizeLogo;

    [SerializeField]
    [DataMember(Name = "ControllerVariant", Order = 31)]
    private string _controllerVariant;

    public string ControllerVariant => _controllerVariant;

    [SerializeField]
    [DataMember(Name = "SingleButtonControl", Order = 32)]
    private bool _singleButtonControl;

    public bool SingleButtonControl {
        get => _singleButtonControl;
        set => _singleButtonControl = value;
    }

    [SerializeField]
    [DataMember(Name = "SmallPlayArea", Order = 33)]
    private bool _smallPlayArea;

    public bool SmallPlayArea {
        get => _smallPlayArea;
        set => _smallPlayArea = value;
    }

    [SerializeField]
    [DataMember(Name = "InvertSmallPlayArea", Order = 34)]
    private bool _invertSmallPlayArea;

    public bool InvertSmallPlayArea {
        get => _invertSmallPlayArea;
        set => _invertSmallPlayArea = value;
    }

    [SerializeField]
    [DataMember(Name = "ShowShotsPerSecond", Order = 35)]
    private bool _showShotsPerSecond;

    public bool ShowShotsPerSecond {
        get => _showShotsPerSecond;
        set => _showShotsPerSecond = value;
    }

    [SerializeField]
    [DataMember(Name = "FoveatedRenderingLevel", Order = 36)]
    private int _foveatedRenderingLevel;

    public int FoveatedRenderingLevel {
        get => _foveatedRenderingLevel;
        set => _foveatedRenderingLevel = value;
    }

}