using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Home.UI {
    public class RegionPanel : HomeMenuPanel {
        [SerializeField] private TMP_Dropdown _regionDropdown;

        // Start is called before the first frame update
        private new void Awake() {
            base.Awake();
            FeedRegionDropdown();
        }

        private void FeedRegionDropdown() {
            _regionDropdown.ClearOptions();
            PhotonRegionHelper.NameToCodeDictionary
                .ForEach(region
                    => _regionDropdown.options.Add(new TMP_Dropdown.OptionData(region.Key)));
        }

        [UsedImplicitly]
        public void OnConnectToServerButtonPressed() {
            ConfigurationManager.Configuration.PreferredRegion = PhotonRegionHelper.GetRegionCodeByName(_regionDropdown.captionText.text);
            ConfigurationManager.WriteConfigToFile();
            ConnectionManager.Instance.Connect();
            UIController.SwitchPanel(HubUIController.PanelType.GTC);
        }
    }
}