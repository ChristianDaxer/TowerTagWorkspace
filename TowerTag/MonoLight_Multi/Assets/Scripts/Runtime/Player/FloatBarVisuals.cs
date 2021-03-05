using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FloatBarVisuals : FloatVisuals {
    [FormerlySerializedAs("bar")] [SerializeField]
    private Image _bar;

    public override void SetValue(float newValue) {
        _bar.fillAmount = newValue;
    }

    private void OnDestroy() {
        _bar = null;
    }
}