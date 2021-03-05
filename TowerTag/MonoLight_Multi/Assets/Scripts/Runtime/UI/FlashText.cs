using UnityEngine;
using UnityEngine.UI;

public class FlashText : MonoBehaviour {
    [SerializeField] private string[] _texts;
    [SerializeField] private float _period;
    [SerializeField] private Text _text;

    private void Update() {
        _text.text = _texts[(int) (Time.time / _period) % _texts.Length];
    }

    public void SetTexts(string[] texts) {
        _texts = texts;
    }

    public void SetTextVisible(bool visible) {
        _text.enabled = visible;
    }

    public void SetMaterial(Material material) {
        _text.material = material;
    }
}