using JetBrains.Annotations;
using TowerTagAPIClient;
using TowerTagAPIClient.Store;
using UnityEngine;
using UnityEngine.UI;
using Version = TowerTagAPIClient.Model.Version;

public class GameVersionUpdate : MonoBehaviour {
    // ReSharper disable once NotAccessedField.Local
    [SerializeField, Tooltip("The update tower tag button")]
    // ReSharper disable once NotAccessedField.Local
    private GameObject _updateButton;

#pragma warning disable CS0414
    [SerializeField, Tooltip("The text above the update button with the new version")]
    private Text _versionText;

    [SerializeField, Tooltip("Text to display next to version number")]
    // ReSharper disable once NotAccessedField.Local
    private string _infoText;

    [SerializeField, Tooltip("Version before or after text")]
    private bool _asPrefix;
#pragma warning restore CS0414

    private string _currentVersion;

    private void OnEnable() {
        VersionStore.VersionReceived += OnVersionReceived;
    }

    private void Start() {
        if(!TowerTagSettings.BasicMode) StartCheckForUpdates();
    }

    private void OnDisable() {
        VersionStore.VersionReceived -= OnVersionReceived;
    }

    private void OnVersionReceived(Version version) {
        _currentVersion = version.version;
        if (_currentVersion.Equals(ConnectionManager.Instance.GameVersion)) return;

        _updateButton.SetActive(true);
        if (_asPrefix)
            _versionText.text = _currentVersion + " " + _infoText;
        else
            _versionText.text = _infoText + " " + _currentVersion;
    }

    private void StartCheckForUpdates() {
        VersionStore.GetLatestVersion(Authentication.OperatorApiKey);
    }

    /// <summary>
    /// Opens the web browser and instantly downloads the new setup
    /// </summary>
    [UsedImplicitly]
    public void DownloadNewVersionInWebBrowser() {
        if (!string.IsNullOrEmpty(_currentVersion)) {
            Application.OpenURL("https://tower-tag.com/blog/");
            Application.OpenURL("http://vrnerds.biz/TowerTag/Tower%20Tag%20" + _currentVersion + "%20Setup.exe");
        }
    }
}