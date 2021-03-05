using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Photon.Realtime;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.Networking;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Player = Photon.Realtime.Player;

public class Customization : IInRoomCallbacks, IMatchmakingCallbacks {
    private static Customization _instance;

    [NotNull]
    public static Customization Instance => _instance ?? (_instance = new Customization());

    public static bool UseCustomLogos { get; private set; }
    public static bool UseCustomColors { get; private set; }

    public static event Action CustomizationChanged;

    [Serializable]
    public enum SpriteType {
        Symbol,
        Icon,
        MapLogo,
        MapLogo1,
        MapLogo2
    }

    private struct CustomLogo {
        public readonly SpriteType SpriteType;
        public Sprite Sprite;
        public readonly string Filename;

        public CustomLogo(SpriteType spriteType, Sprite sprite, string filename) {
            SpriteType = spriteType;
            Sprite = sprite;
            Filename = filename;
        }
    }

    // Hard coded file names! Customers have to know them!
    private const string SymbolFileName = "Symbol.png";
    private const string IconFileName = "Icon.png";
    private const string SingleMapLogoFileName = "MapLogo.png";
    private const string MapLogoFileName1 = "MapLogo_1.png";
    private const string MapLogoFileName2 = "MapLogo_2.png";

    private static List<CustomLogo> CustomLogos;

    private static readonly string _folderPath = Application.persistentDataPath + "/Customization";
    private readonly IPhotonService _photonService;
    private const string CustomLogosKey = "customizeLogos";
    private const string CustomColorsKey = "customizeColors";
    private const string CustomFireHueKey = "customFireColor";
    private const string CustomIceHueKey = "customIceColor";

    private Customization() {
        _photonService = ServiceProvider.Get<IPhotonService>();
        _photonService.AddCallbackTarget(this);
    }

    /// <summary>
    /// Looks for all customizable components
    /// </summary>
    public void CustomizeLogos(bool useCustomLogos) {
        if (UseCustomLogos == useCustomLogos) return;
        if (!useCustomLogos) {
            UseCustomLogos = false;
            CustomLogos.Clear();
            if (SharedControllerType.IsAdmin) SendCustomizationSettings();
            CustomizationChanged?.Invoke();
            return;
        }

        if (CheckCustomFolder()) {
            UseCustomLogos = true;
            CustomLogos = new List<CustomLogo> {
                new CustomLogo(SpriteType.Symbol, null, SymbolFileName),
                new CustomLogo(SpriteType.Icon, null, IconFileName),
                new CustomLogo(SpriteType.MapLogo, null, SingleMapLogoFileName),
                new CustomLogo(SpriteType.MapLogo1, null, MapLogoFileName1),
                new CustomLogo(SpriteType.MapLogo2, null, MapLogoFileName2),
            };
            CustomLogos.ForEach(tuple => StaticCoroutine.StartStaticCoroutine(TryToLoadCustomSprite(tuple)));

            if (SharedControllerType.IsAdmin) SendCustomizationSettings();
            CustomizationChanged?.Invoke();
        }
        else {
            Debug.Log("Customization is allowed but no directory of content found!");
        }
    }

    private void SendCustomizationSettings() {
        if (!SharedControllerType.IsAdmin || TowerTagSettings.Home) return;
        IRoom room = _photonService.CurrentRoom;
        if (room == null) return;
        Hashtable customProperties = room.CustomProperties;
        var changed = false;

        // custom logo
        bool customLogo = customProperties.ContainsKey(CustomLogosKey) && (bool) customProperties[CustomLogosKey];
        if (customLogo != UseCustomLogos) {
            changed = true;
            customProperties[CustomLogosKey] = UseCustomLogos;
        }

        // custom colors
        bool customColors = customProperties.ContainsKey(CustomColorsKey) && (bool) customProperties[CustomColorsKey];
        if (customColors != UseCustomColors) {
            changed = true;
            customProperties[CustomColorsKey] = UseCustomColors;
            customProperties[CustomFireHueKey] = (int) ConfigurationManager.Configuration.FireHue;
            customProperties[CustomIceHueKey] = (int) ConfigurationManager.Configuration.IceHue;
        }

        if (changed) room.SetCustomProperties(customProperties);
    }

    private void ReadCustomizationSettings() {
        if (SharedControllerType.IsAdmin || TowerTagSettings.Home) return;
        IRoom room = _photonService.CurrentRoom;
        if (room == null) return;
        Hashtable customProperties = room.CustomProperties;
        if (customProperties.ContainsKey(CustomLogosKey) && (bool) customProperties[CustomLogosKey]) {
            if (!UseCustomLogos) CustomizeLogos(true);
        }
        else {
            if (UseCustomLogos) CustomizeLogos(false);
        }

        if (customProperties.ContainsKey(CustomColorsKey)) {
            if ((bool) customProperties[CustomColorsKey] && !UseCustomColors) {
                var fireHue = Convert.ToUInt16(customProperties[CustomFireHueKey]);
                var iceHue = Convert.ToUInt16(customProperties[CustomIceHueKey]);
                bool changed = !UseCustomColors
                               || fireHue != ConfigurationManager.Configuration.FireHue
                               || iceHue != ConfigurationManager.Configuration.IceHue;
                ConfigurationManager.Configuration.FireHue = fireHue;
                ConfigurationManager.Configuration.IceHue = iceHue;
                if (changed) CustomizeColors(TeamManager.Singleton);
            }

            if (!(bool) customProperties[CustomColorsKey] && UseCustomColors) {
                UseDefaultColors(TeamManager.Singleton);
            }
        }
    }

    public void CustomizeColors(TeamManager teamManager) {
        UseCustomColors = true;
        teamManager.TeamIce.Colors = TeamColors.GenerateFromHue(ConfigurationManager.Configuration.IceHue);
        teamManager.TeamFire.Colors = TeamColors.GenerateFromHue(ConfigurationManager.Configuration.FireHue);
        if (SharedControllerType.IsAdmin) SendCustomizationSettings();
        teamManager.Init();
        CustomizationChanged?.Invoke();
    }

    public void UseDefaultColors(TeamManager teamManager) {
        UseCustomColors = false;
        teamManager.TeamIce.Colors = TeamColors.GenerateFromHue(180);
        teamManager.TeamFire.Colors = TeamColors.GenerateFromHue(35);
        if (SharedControllerType.IsAdmin) SendCustomizationSettings();
        teamManager.Init();
        CustomizationChanged?.Invoke();
    }

    private static IEnumerator TryToLoadCustomSprite(CustomLogo customLogo) {
        using (var request = UnityWebRequestTexture.GetTexture(_folderPath + "/" + customLogo.Filename)) {
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError) {
                Debug.Log($"File {customLogo.Filename} not found! {request.error}");
            }
            else {
                var texture = DownloadHandlerTexture.GetContent(request);

                //To set the sprite of the custom logo remove the CustomLogo with a null sprite
                CustomLogos.Remove(customLogo);

                //To have MipMaps enabled the texture has to be copied on a new one and the Apply() has to be called
                var textureWithMipMaps = new Texture2D(texture.width, texture.height);
                textureWithMipMaps.Apply(true);
                textureWithMipMaps.SetPixels(texture.GetPixels());
                customLogo.Sprite = Sprite.Create(textureWithMipMaps, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), 200);

                CustomLogos.Add(customLogo);
                Debug.Log($"Sprite from file {customLogo.Filename} loaded.");
            }
        }
    }

    public static Sprite GetSpriteByType(SpriteType spriteType) {
        CustomLogo chosenLogo = CustomLogos.FirstOrDefault(logo => logo.SpriteType == spriteType);
        return chosenLogo.Sprite != null ? chosenLogo.Sprite : null;
    }

    /// <summary>
    /// Checking for the defined directory, creates it if it doesn't exist
    /// </summary>
    /// <returns>false when the directory doesn't exist or the folder is empty</returns>
    private static bool CheckCustomFolder() {
        if (!Directory.Exists(_folderPath)) {
            Directory.CreateDirectory(_folderPath);
            Debug.Log($"Directory {_folderPath} created.");
            return false;
        }
        else if (Directory.GetFiles(_folderPath, "*.png").Length <= 0) {
            Debug.Log($"No .png files in {_folderPath} found!");
            return false;
        }

        return true;
    }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
        ReadCustomizationSettings();
    }

    public void OnJoinedRoom() {
        if (SharedControllerType.IsAdmin)
            SendCustomizationSettings();
        else
            ReadCustomizationSettings();
    }

    public void OnPlayerEnteredRoom(Player newPlayer) {
    }

    public void OnPlayerLeftRoom(Player otherPlayer) {
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) {
    }

    public void OnMasterClientSwitched(Player newMasterClient) {
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList) {
    }

    public void OnCreatedRoom() {
    }

    public void OnCreateRoomFailed(short returnCode, string message) {
    }

    public void OnJoinRoomFailed(short returnCode, string message) {
    }

    public void OnJoinRandomFailed(short returnCode, string message) {
    }

    public void OnLeftRoom() {
    }
}