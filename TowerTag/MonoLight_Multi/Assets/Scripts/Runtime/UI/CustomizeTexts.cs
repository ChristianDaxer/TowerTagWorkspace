using TMPro;
using UnityEngine;

public class CustomizeTexts : MonoBehaviour {
    [SerializeField] private TMP_Text[] _defaultTexts;
    [SerializeField] private TMP_Text[] _fireTexts;
    [SerializeField] private TMP_Text[] _iceTexts;
    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private Material _fireMaterial;
    [SerializeField] private Material _iceMaterial;

    private void OnValidate() {
        if (gameObject.scene.buildIndex == -1) return;
        Refresh();
    }

    private void Start() {
        Refresh();
    }

    private void Refresh() {
        _defaultTexts.ForEach(txt => txt.color = _defaultMaterial.color);
        _iceTexts.ForEach(txt => txt.color = _iceMaterial.color);
        _fireTexts.ForEach(txt => txt.color = _fireMaterial.color);
    }
}