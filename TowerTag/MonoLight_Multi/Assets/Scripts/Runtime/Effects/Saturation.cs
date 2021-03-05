using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class Saturation : MonoBehaviour {
    public float Value { private get; set; } = 1;
    [SerializeField, FormerlySerializedAs("shader")] private Shader _shader;

    private Material _material;
    private int _propertyID;

    private Material Material {
        get {
            if (_material == null) {
                _material = new Material(_shader) { hideFlags = HideFlags.HideAndDontSave };
            }
            return _material;
        }
    }

    protected virtual void Start() {

        if (_shader == null) {
            _shader = Shader.Find("_OwnShader/PostEffect/Saturation");
        }

        // Disable the image effect if the shader can't
        // run on the users graphics card
        if (!_shader || !_shader.isSupported) {
            enabled = false;
            return;
        }

        _propertyID = Shader.PropertyToID("_BlendFactor");
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (Value >= 1) {
            Graphics.Blit(source, destination);
            return;
        }
        Material.SetFloat(_propertyID, Value);
        Graphics.Blit(source, destination, Material);
    }

    private void OnDisable() {
        if (_material != null) {
            DestroyImmediate(_material);
            _material = null;
        }
    }
}
