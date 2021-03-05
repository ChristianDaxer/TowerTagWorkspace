using TowerTag;
using TowerTagSOES;
using UnityEngine;

public class CameraFogSettingOverride : MonoBehaviour {
    [SerializeField] private SharedFogSettings _sceneSettings;
    [SerializeField] private SharedFogSettings _outOfChaperoneSettings;

    private Camera _cam;
    private IPlayer _localPlayer;
    private bool _effectToggled;

    private void Awake() {
        _cam = GetComponent<Camera>();
        _localPlayer = GetComponentInParent<IPlayer>();
        if (_cam == null || _localPlayer == null) enabled = false;
    }

    private void OnEnable() {
        _localPlayer.OutOfChaperoneStateChanged += EffectToggled;
        _localPlayer.InTowerStateChanged += EffectToggled;
        Camera.onPreRender += MyPreRender;
        Camera.onPostRender += MyPostRender;
    }

    private void OnDisable() {
        _localPlayer.OutOfChaperoneStateChanged -= EffectToggled;
        _localPlayer.InTowerStateChanged -= EffectToggled;
        Camera.onPreRender -= MyPreRender;
        Camera.onPostRender -= MyPostRender;
    }

    private void EffectToggled(IPlayer player, bool setActive) {
        _effectToggled = setActive;
    }

    private void MyPreRender(Camera cam) {
        if (cam != _cam || _sceneSettings.IsSettingAlreadyApplied()) return;
        _sceneSettings.Apply();
    }

    private void MyPostRender(Camera cam) {
        if (cam != _cam || !_effectToggled) return;
        _outOfChaperoneSettings.Apply();
    }
}