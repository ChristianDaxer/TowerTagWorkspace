using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Home.UI;
using Photon.Pun;
using TMPro;
using TowerTagSOES;
using UnityEngine;

public static class PhotonRegionHelper {
    public static string CurrentRegion => ConfigurationManager.Configuration.PreferredRegion;

    public static Dictionary<string, string> NameToCodeDictionary { get; } = new Dictionary<string, string>() {
        {"GLOBAL / EU", "eu"},
        {"NORTH AMERICA", "usw"},
        {"ASIA", "asia"}
    };

    public static string GetRegionCodeByName(string displayText) {
        return NameToCodeDictionary[displayText];
    }

    public static string GetRegionNameByCode(string regionCode) {
        return NameToCodeDictionary.FirstOrDefault(element => element.Value.Equals(regionCode)).Key;
    }

    public static void FillRegionsIntoDropdown(TMP_Dropdown dropdown) {
        dropdown.ClearOptions();
        var regionOptions = new List<TMP_Dropdown.OptionData>();
        NameToCodeDictionary
            .ForEach(region
                => regionOptions.Add(new TMP_Dropdown.OptionData(region.Key)));
        dropdown.options = regionOptions;
        TMP_Dropdown.OptionData currentOption = dropdown.options
            .FirstOrDefault(option => option.text.Equals(
                GetRegionNameByCode(PhotonRegionHelper.CurrentRegion)));
        dropdown.SetValueWithoutNotify(dropdown.options.IndexOf(currentOption));
    }

    public static IEnumerator ChangeRegion(string regionName, HubUIController uiController, HubUIController.PanelType
        panelToLoadIn, int delay = 2) {
        if (ConfigurationManager.Configuration.PreferredRegion.Equals(GetRegionCodeByName(regionName))) yield break;
        ConfigurationManager.Configuration.PreferredRegion = GetRegionCodeByName(regionName);
        ConfigurationManager.WriteConfigToFile();
        ConnectionManager.Instance.Disconnect();
        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);
        ConnectingPanel.PanelTypeToLoadIn = panelToLoadIn;
        if (!SharedControllerType.Spectator) uiController.SwitchPanel(HubUIController.PanelType.Loading);
        yield return new WaitForSeconds(delay);
        ConnectionManager.Instance.Connect();
    }
}