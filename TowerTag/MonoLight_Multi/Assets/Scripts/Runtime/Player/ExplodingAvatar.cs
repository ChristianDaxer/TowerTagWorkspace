using System;
using System.Diagnostics.CodeAnalysis;
using TowerTag;
using UnityEngine;

[ExecuteInEditMode]
public class ExplodingAvatar : MonoBehaviour {
    [SerializeField] private float _explosionForce = 10f;
    [SerializeField] private float _explosionRadius = 2f;
    [SerializeField] private float _heightOffset = 2f;
    [SerializeField] public GameObject _explosionEffectParent;
    [SerializeField] private AvatarAnchor _anchor;
    [SerializeField] private Rigidbody[] _explodingObjects;
    [SerializeField, HideInInspector] private TransformValues[] _defaultValues;

    [Header("Team Colors")]
    [SerializeField] private TeamID _teamID;
    private MaterialPropertyBlock _propertyBlock;
    [SerializeField] private Renderer[] _teamTintedRenderers;
    [SerializeField] private Renderer[] _teamTintedEmitters;
    [SerializeField] private float _intensity = 1;
    private readonly int _colorPropertyID = Shader.PropertyToID("_Color");
    private readonly int _emissionColorPropertyID = Shader.PropertyToID("_EmissionColor");

    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    private struct TransformValues {
        public Vector3 Position;
        public Vector3 Scale;
        public Quaternion Rotation;

        public TransformValues(Vector3 position, Quaternion rotation, Vector3 scale) {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }

    public TeamID TeamID => _teamID;

    private void OnValidate() {
        if (gameObject.scene.buildIndex == -1) return;
        ChangeTeam(TeamID);
    }

    private void ChangeTeam(TeamID teamID) {
        _propertyBlock = _propertyBlock ?? new MaterialPropertyBlock();
        _teamTintedRenderers.ForEach(r => {
            _propertyBlock.Clear();
            r.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(_colorPropertyID, TeamManager.Singleton.Get(teamID).Colors.Avatar);
            r.SetPropertyBlock(_propertyBlock);
        });

        _teamTintedEmitters.ForEach(r => {
            _propertyBlock.Clear();
            r.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(_emissionColorPropertyID, _intensity * TeamManager.Singleton.Get(teamID).Colors.Emissive);
            r.SetPropertyBlock(_propertyBlock);
        });
    }

    public void InitAvatar(Transform originalHead, Transform originalBody, TeamID teamID) {
        if (_anchor == null) {
            Debug.LogWarning("ExplodingAvatar.InitAvatar: no anchors available!");
            return;
        }

        if (_defaultValues == null) {
            Debug.LogWarning("ExplodingAvatar.InitAvatar: no default values available!");
            return;
        }

        if (_explodingObjects == null) {
            Debug.LogWarning("ExplodingAvatar.InitAvatar: no objects to handle!");
            return;
        }

        if (_explodingObjects.Length != _defaultValues.Length) {
            Debug.LogWarning(
                "ExplodingAvatar.InitAvatar: number of defaultValues doesn't match number of objects to handle!");
            return;
        }

        CopyTransform(originalHead, _anchor.HeadTransform, false);
        CopyTransform(originalBody, _anchor.BodyTransform, false);

        for (var i = 0; i < _defaultValues.Length; i++) {
            CopyTransform(_defaultValues[i], _explodingObjects[i].transform, true);
            _explodingObjects[i].useGravity = false;
            _explodingObjects[i].isKinematic = true;
        }

        ChangeTeam(teamID);
    }

    public void Explode() {
        if (_explodingObjects != null) {
            foreach (var r in _explodingObjects) {
                r.velocity = Vector3.zero;
                r.angularVelocity = Vector3.zero;
                r.useGravity = true;
                r.isKinematic = false;
                r.AddExplosionForce(_explosionForce, _anchor.HeadTransform.position, _explosionRadius, _heightOffset,
                    ForceMode.Force);
            }
        }
    }

    public void CopyDefaultValues() {
        _defaultValues = new TransformValues[_explodingObjects.Length];
        for (var i = 0; i < _defaultValues.Length; i++) {
            Transform t = _explodingObjects[i].transform;
            _defaultValues[i] = new TransformValues(t.localPosition, t.localRotation, t.localScale);
            _explodingObjects[i].useGravity = false;
            _explodingObjects[i].isKinematic = true;
        }
    }

    private static void CopyTransform(Transform from, Transform to, bool local) {
        if (local) {
            to.localPosition = from.localPosition;
            to.localRotation = from.localRotation;
            to.localScale = from.localScale;
        }
        else {
            to.position = from.position;
            to.rotation = from.rotation;
        }
    }

    private static void CopyTransform(TransformValues from, Transform to, bool local) {
        if (local) {
            to.localPosition = from.Position;
            to.localRotation = from.Rotation;
            to.localScale = from.Scale;
        }
        else {
            to.position = from.Position;
            to.rotation = from.Rotation;
        }
    }
}