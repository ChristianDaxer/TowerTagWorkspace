using CustomBuildPipeline;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class SetVersionText : MonoBehaviour {
    [SerializeField, Tooltip("The Game Version to display"), ContextMenuItem("Set Version Text", "SetVersion")]
    private SharedVersion _gameVersion;

    private string MyText {
        set => GetComponent<Text>().text = value;
    }

    private void OnEnable() {
        SetVersion();
    }

    // Use this for initialization
    [ContextMenu("Set Version Text")]
    public void SetVersion() {
        MyText = _gameVersion;
    }
}