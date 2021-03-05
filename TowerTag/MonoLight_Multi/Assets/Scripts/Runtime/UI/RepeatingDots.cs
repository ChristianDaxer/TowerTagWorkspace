using UnityEngine;
using UnityEngine.UI;

public class RepeatingDots : MonoBehaviour {
    private Text _affectedText;
    private LTDescr _tween;
    private string _initialText;

    private void Awake() {
        _affectedText = GetComponent<Text>();
    }

    private void OnEnable() {
        _initialText = _affectedText.text;

        _tween = LeanTween.value(0, 3, 1f)
            .setLoopPingPong()
            .setOnUpdate(v => {
                string dots = new string('.', Mathf.RoundToInt(v));
                _affectedText.text = _initialText + dots;
            });
    }

    private void OnDisable() {
        LeanTween.cancel(_tween.id);
        _affectedText.text = _initialText;
    }
}