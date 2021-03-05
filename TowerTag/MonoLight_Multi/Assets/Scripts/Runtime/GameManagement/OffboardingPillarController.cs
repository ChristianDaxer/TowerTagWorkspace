using UnityEngine;

public class OffboardingPillarController : MonoBehaviour {
    [SerializeField] private GameObject _ingameVRMenuOffline;
    [SerializeField] private GameObject _instructions;

    private void Awake() {
        if (_ingameVRMenuOffline == null || _instructions == null) {
            Debug.LogWarning("Can't find some referenced Objects in Offboarding Pillar Prefab");
            return;
        }
        if (TowerTagSettings.Home) {
            _ingameVRMenuOffline.SetActive(true);
            _instructions.SetActive(false);
        }
        else {
            _instructions.SetActive(true);
            _ingameVRMenuOffline.SetActive(false);
        }
    }
}