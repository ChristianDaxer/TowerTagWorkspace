using System;
using System.Collections;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.Rendering;

public class AvatarVisuals : MonoBehaviour {
    [Header("Main Material")] [SerializeField]
    private string _avatarMatTintColorPropertyName = "_Color";

    [SerializeField] private string _avatarMatTintHighlightPropertyName = "_HighlightColor";
    [SerializeField] private Material _standardMaterial;
    [SerializeField] private Material _seeThroughMaterial;
    [SerializeField] private Material _avatarMat;
    [SerializeField] private Renderer[] _stdAvatarMatRenderer;


    [Header("Screen Material")] [SerializeField]
    private string _lightMatTintColorPropertyName = "_Color";

    [SerializeField] private string _lightMatEmissionColorPropertyName = "_EmissionColor";
    [SerializeField] private Material _lightMat;
    [SerializeField] private Renderer[] _lightMatRenderer;


    [Header("Jet Material")] [SerializeField]
    private string _particleMatTintColorPropertyName = "_TintColor";

    [SerializeField] private Material _particleMat;
    [SerializeField] private Renderer[] _particleMatRenderer;

    [Header("Jet Parent")] [SerializeField]
    private GameObject _jetParent;

    [Header("Ghost Shader")] [SerializeField]
    private string _avatarMatBlendToGhostPropertyName = "_Blend";

    public Renderer[] StdAvatarMatRenderer => _stdAvatarMatRenderer;

    private int _avatarMatBlendToGhostPropertyID;
    private Coroutine _currentReanimation;
    private Coroutine _currentFade;
    private static readonly int _srcBlendID = Shader.PropertyToID("_SrcBlend");
    private static readonly int _dstBlendID = Shader.PropertyToID("_DstBlend");
    private static readonly int _zWriteID = Shader.PropertyToID("_ZWrite");
    private static readonly int _color = Shader.PropertyToID("_Color");
    private static readonly int NoiseAlphaFactor = Shader.PropertyToID("NoiseAlphaFactor");
    private const float FadeInTime = 1f;
    private const float FadeOutTime = 0.25f;

    public void Init() {
        _avatarMatBlendToGhostPropertyID = Shader.PropertyToID(_avatarMatBlendToGhostPropertyName);

        _avatarMat = SharedControllerType.IsAdmin || SharedControllerType.Spectator
            ? new Material(_seeThroughMaterial)
            : new Material(_standardMaterial);

        ApplyMaterialToRenderer(_stdAvatarMatRenderer, _avatarMat);

        _lightMat = new Material(_lightMat);
        ApplyMaterialToRenderer(_lightMatRenderer, _lightMat);

        _particleMat = new Material(_particleMat);
        ApplyMaterialToRenderer(_particleMatRenderer, _particleMat);
    }

    public void ToggleSeeThroughMaterial(bool setActive) {
        _avatarMat = setActive ? _seeThroughMaterial : _standardMaterial;
        ApplyMaterialToRenderer(_stdAvatarMatRenderer, _avatarMat);
    }


    /// <summary>
    /// Toggles the visibility of this player
    /// </summary>
    /// <param name="setActive"></param>
    public void ToggleRenderer(bool setActive) {
        if (_stdAvatarMatRenderer == null)
            return;

        _stdAvatarMatRenderer.ForEach(r => r.enabled = setActive);
        _particleMatRenderer.ForEach(r => r.enabled = setActive);
        _lightMatRenderer.ForEach(r => r.enabled = setActive);
    }

    private static void ApplyMaterialToRenderer(Renderer[] r, Material m) {
        if (r == null)
            return;

        if (m == null)
            return;

        foreach (Renderer renderer in r) {
            renderer.sharedMaterial = m;
        }
    }

    public void SetTeamColor(TeamID teamID) {
        ITeam team = TeamManager.Singleton.Get(teamID);
        if (team == null) {
            Debug.LogWarning("AvatarVisuals.ChangeAvatarColor: Can't change Team colors (team is null)!");
            return;
        }

        // Variant 2: copied material
        // avatar main Material
        Color tmp = _avatarMat.GetColor(_avatarMatTintColorPropertyName);
        Color albedoTint = team.Colors.Avatar;
        albedoTint.a = tmp.a;
        _avatarMat.SetColor(_avatarMatTintColorPropertyName, albedoTint);

        if (SharedControllerType.IsAdmin)
            _avatarMat.SetColor(_avatarMatTintHighlightPropertyName, albedoTint);

        // Avatar Screen Material
        tmp = _lightMat.GetColor(_lightMatTintColorPropertyName);
        Color emissiveColor = team.Colors.Effect;
        emissiveColor.a = tmp.a;
        _lightMat.SetColor(_lightMatTintColorPropertyName, emissiveColor);
        _lightMat.SetColor(_lightMatEmissionColorPropertyName, emissiveColor);

        // Jet particle Material
        Color jetColor = team.Colors.Effect;
        tmp = _particleMat.GetColor(_particleMatTintColorPropertyName);
        jetColor.a = tmp.a;
        _particleMat.SetColor(_particleMatTintColorPropertyName, jetColor);
    }


    /// <summary>
    /// Initiates the ghost or normal mode
    /// </summary>
    /// <param name="makeGhost"></param>
    /// <param name="gunParent"></param>
    private void SwitchToGhost(bool makeGhost, GameObject gunParent) {
        MakeTransparent(_avatarMat, makeGhost);
        MakeTransparent(_lightMat, makeGhost);
        _jetParent.SetActive(!makeGhost);
        gunParent.SetActive(!makeGhost);
    }

    /// <summary>
    /// Changes some shader values for the ghost or normal mode
    /// </summary>
    /// <param name="material"></param>
    /// <param name="makeTransparent"></param>
    private static void MakeTransparent(Material material, bool makeTransparent) {
        if (makeTransparent) {
            material.SetOverrideTag("RenderType", "Fade");
            material.SetInt(_srcBlendID, (int) BlendMode.SrcAlpha);
            material.SetInt(_dstBlendID, (int) BlendMode.OneMinusSrcAlpha);
            material.SetInt(_zWriteID, 0);
            material.SetFloat(NoiseAlphaFactor, 0f);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int) RenderQueue.Transparent;
        }
        else {
            material.SetOverrideTag("RenderType", "");
            material.SetInt(_srcBlendID, (int) BlendMode.One);
            material.SetInt(_dstBlendID, (int) BlendMode.Zero);
            material.SetInt(_zWriteID, 1);
            material.SetFloat(NoiseAlphaFactor, 0.2f);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = -1;
        }
    }

    /// <summary>
    /// Initiates the switch between alive (normal) and dead (ghost)
    /// </summary>
    /// <param name="setActive"></param>
    /// <param name="gunParent">Gets disabled as ghost</param>
    public void OnSetActive(bool setActive, GameObject gunParent) {
        if (!gameObject.activeSelf)
            return;
        // disable
        if (!setActive) {
            if (_currentFade != null)
                StopCoroutine(_currentFade);

            //Disable
            SwitchToGhost(true, gunParent);
            _currentFade = StartCoroutine(GhostFade(false, FadeOutTime, 1f, 0f, gunParent));
        }
        // enable
        else {
            if (_currentFade != null)
                StopCoroutine(_currentFade);
            _currentFade = StartCoroutine(GhostFade(true, FadeInTime, 0f, 1f, gunParent, SwitchToGhost));
        }
    }

    /// <summary>
    /// Lerps the necessary shader values to switch between ghost and normal
    /// </summary>
    /// <param name="setActive"></param>
    /// <param name="duration">Duration of the lerp</param>
    /// <param name="min">"From"-value of the lerp</param>
    /// <param name="max">"To"-value of the lerp</param>
    /// <param name="gunParent">Parameter for the callback</param>
    /// <param name="finishedCallback">Gets called after the fade is complete</param>
    /// <returns></returns>
    private IEnumerator GhostFade(bool setActive, float duration, float min, float max, GameObject gunParent,
        Action<bool, GameObject> finishedCallback = null) {
        float startTime = Time.time;
        var t = 0f;
        if (duration > 0) {
            while (t <= 1) {
                t = (Time.time - startTime) / duration;
                float delta = Mathf.Lerp(min, max, t);
                _avatarMat.SetFloat(_avatarMatBlendToGhostPropertyID, delta);
                _lightMat.SetFloat(_avatarMatBlendToGhostPropertyID, delta);
                yield return new WaitForEndOfFrame();
            }
        }

        Color stdColor = _avatarMat.GetColor(_color);
        Color lightColor = _lightMat.GetColor(_color);
        stdColor.a = max;
        lightColor.a = max;
        _avatarMat.SetColor(_color, stdColor);
        _lightMat.SetColor(_color, lightColor);
        _avatarMat.SetFloat(_avatarMatBlendToGhostPropertyID, max);
        _lightMat.SetFloat(_avatarMatBlendToGhostPropertyID, max);


        finishedCallback?.Invoke(!setActive, gunParent);
        _currentFade = null;
    }
}