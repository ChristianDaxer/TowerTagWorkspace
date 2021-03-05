using UnityEngine;

public class BHapticsActivator : MonoBehaviour {

    [SerializeField] private GameObject _bHapticsManager;

    private void Start() {
        _bHapticsManager.SetActive(ConfigurationManager.Configuration.EnableHapticHitFeedback);
    }
}
