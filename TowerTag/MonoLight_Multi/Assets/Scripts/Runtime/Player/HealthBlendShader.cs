using UnityEngine;

public class HealthBlendShader : FloatVisuals {
    [SerializeField] private Renderer[] _renderer;
    [SerializeField] private string _blendPropName = "_BlendValue";
    private int _blendPropValueID;

    private void Start() {
        _blendPropValueID = Shader.PropertyToID(_blendPropName);
    }

    public override void SetValue(float newValue) {
        if (_renderer != null) {
            ColorChanger.SetCustomFloatPropertyInRenderers(_renderer, _blendPropValueID, newValue);
        }
    }
}