using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Camera))]
public sealed class ColorFaderPostFX : MonoBehaviour {
    [SerializeField, FormerlySerializedAs("fadeColor")] private Color _fadeColor;
    [SerializeField, FormerlySerializedAs("fadeFactor")] private float _fadeFactor;
    [SerializeField, FormerlySerializedAs("shader")] private Shader _shader;
    public Color FadeColor {
        set => _fadeColor = value;
    }
    public float FadeFactor {
        set => _fadeFactor = value;
    }


    private Material _material;
    private static readonly int _fadeFactorID = Shader.PropertyToID("_FadeFactor");
    private static readonly int _tintColorID = Shader.PropertyToID("_TintColor");

    private Material Material {
        get {
            if (_material == null) {
                _material = new Material(_shader) { hideFlags = HideFlags.HideAndDontSave };
            }
            return _material;
        }
    }

    private void Start() {

        // Disable the image effect if the shader can't
        // run on the users graphics card
        if (!_shader || !_shader.isSupported)
            enabled = false;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Material.SetFloat(_fadeFactorID, _fadeFactor);
        Material.SetColor(_tintColorID, _fadeColor);
        Graphics.Blit(source, destination, Material);
    }

    private void OnDisable() {
        if (_material != null) {
            DestroyImmediate(_material);
            _material = null;
        }
    }
}
