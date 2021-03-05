using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Camera))]
public class BlurWithMask : MonoBehaviour {
    [FormerlySerializedAs("innerRadius")] [SerializeField] private float _innerRadius = 0.25f;
    [FormerlySerializedAs("outerRadius")] [SerializeField] private float _outerRadius = 0.5f;
    [FormerlySerializedAs("downsample")] [SerializeField] private int _downSample = 4;
    [FormerlySerializedAs("blurSpread")] [SerializeField] private float _blurSpread = 1;
    [FormerlySerializedAs("blurIterations")] [SerializeField] private int _blurIterations = 1;
    [FormerlySerializedAs("blurTintColor")] [SerializeField] private Color _blurTintColor = Color.white;
    [FormerlySerializedAs("distortion")] [SerializeField] private float _distortion = 0.025f;
    [FormerlySerializedAs("shader")] [SerializeField] private Shader _shader;
    private Material _material;
    private static readonly int _innerRadiusID = Shader.PropertyToID("_InnerRadius");
    private static readonly int _outerRadiusID = Shader.PropertyToID("_OuterRadius");
    private static readonly int _distortionOffsetID = Shader.PropertyToID("_distortionOffset");
    private static readonly int _tintColorID = Shader.PropertyToID("_blurTintColor");
    private static readonly int _offsetsID = Shader.PropertyToID("offsets");
    private static readonly int _blurredTexID = Shader.PropertyToID("_BlurredTex");

    public float InnerRadius {
        set => _innerRadius = value;
    }
    public float OuterRadius {
        set => _outerRadius = value;
    }
    private Material Material {
        get {
            if (_material == null) {
                _material = new Material(_shader) { hideFlags = HideFlags.HideAndDontSave };
            }
            return _material;
        }
    }

    protected virtual void Start() {

        // Disable the image effect if the shader can't
        // run on the users graphics card
        if (!_shader || !_shader.isSupported)
            enabled = false;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (Material == null) {
            Graphics.Blit(source, destination);
            return;
        }

        Material.SetFloat(_innerRadiusID, _innerRadius);
        Material.SetFloat(_outerRadiusID, _outerRadius);
        Material.SetFloat(_distortionOffsetID, _distortion);
        Material.SetColor(_tintColorID, _blurTintColor);

        _downSample = Mathf.Max(1, _downSample);
        int tmpWidth = source.width / _downSample;
        int tmpHeight = source.height / _downSample;

        RenderTexture tmp = RenderTexture.GetTemporary(tmpWidth, tmpHeight, 0, source.format);
        tmp.vrUsage = source.vrUsage;
        RenderTexture tmp2 = RenderTexture.GetTemporary(tmpWidth, tmpHeight, 0, source.format);
        tmp2.vrUsage = source.vrUsage;
        Graphics.Blit(source, tmp);

        for (int i = 0; i < _blurIterations; i++) {

            Material.SetVector(_offsetsID, new Vector4(0.0f, _blurSpread * 1f / tmpHeight, 0.0f, 0.0f));
            Graphics.Blit(tmp, tmp2, Material, 0);

            Material.SetVector(_offsetsID, new Vector4(_blurSpread * 1f / tmpWidth, 0.0f, 0.0f, 0.0f));
            Graphics.Blit(tmp2, tmp, Material, 0);

        }

        Material.SetTexture(_blurredTexID, tmp);
        Graphics.Blit(source, destination, Material, 1);

        RenderTexture.ReleaseTemporary(tmp);
        RenderTexture.ReleaseTemporary(tmp2);
    }

    private void OnDisable() {
        if (_material != null) {
            DestroyImmediate(_material);
            _material = null;
        }
    }
}
