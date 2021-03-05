using TowerTagSOES;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script for the namePlates of the remoteClients. Make them shine through obstacles on the operator!
/// </summary>
public class ApplySeeThroughMatOnText : MonoBehaviour {
    [SerializeField, Tooltip("TextMaterial to see Text through objects")]
    private Material _textMaterial;

    private Text _text;

    void Start () {
        _text = GetComponent<Text>();
        if (SharedControllerType.IsAdmin) {
            _text.material = _textMaterial;
        }
    }
}
