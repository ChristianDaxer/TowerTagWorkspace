using System;
using TowerTag;
using UnityEngine;

public class VignetteEffectController : MonoBehaviour {
    [SerializeField] protected Material _vignetteMaterial;
    [SerializeField] protected AfterEffect _afterEffect;
    [SerializeField] protected Direction _frontHitVisuals = Direction.Top;
    [SerializeField] protected float _hitRadarLifeTime;
    [SerializeField] protected AnimationCurve _strengthOverTime;

    protected IPlayer _owner;
    protected float CurrentEffectTimeLeft;
    protected float CurrentEffectTimeRight;
    protected float CurrentEffectTimeTop;
    protected float CurrentEffectTimeBottom;
    protected float CurrentEffectStrengthLeft;
    protected float CurrentEffectStrengthRight;
    protected float CurrentEffectStrengthTop;
    protected float CurrentEffectStrengthBottom;
    protected static readonly int LeftID = Shader.PropertyToID("_Left");
    protected static readonly int RightID = Shader.PropertyToID("_Right");
    protected static readonly int TopID = Shader.PropertyToID("_Top");
    protected static readonly int BottomID = Shader.PropertyToID("_Bottom");


    public Material HitRadarMaterial {
        get => _vignetteMaterial;
        set => _vignetteMaterial = value;
    }

    protected void Awake() {
        _owner = GetComponentInParent<IPlayer>();
    }

    protected void Start() {
        InitTimeValues();
    }

    private void Update() {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.UpArrow)) ResetEffectTime(_frontHitVisuals);
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.DownArrow)) ResetEffectTime(Direction.Bottom);
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.LeftArrow)) ResetEffectTime(Direction.Left);
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.RightArrow)) ResetEffectTime(Direction.Right);
    }

    private void InitTimeValues() {
        CurrentEffectTimeLeft = _hitRadarLifeTime;
        CurrentEffectTimeRight = _hitRadarLifeTime;
        CurrentEffectTimeTop = _hitRadarLifeTime;
        CurrentEffectTimeBottom = _hitRadarLifeTime;
    }

    protected void ResetEffectTime(Direction direction) {
        switch (direction) {
            case Direction.Left:
                CurrentEffectTimeLeft = 0;
                break;
            case Direction.Right:
                CurrentEffectTimeRight = 0;
                break;
            case Direction.Top:
                CurrentEffectTimeTop = 0;
                break;
            case Direction.Bottom:
                CurrentEffectTimeBottom = 0;
                break;
            case Direction.LeftAndRight:
                CurrentEffectTimeLeft = 0;
                CurrentEffectTimeRight = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unknown direction");
        }
    }

    protected enum Direction {
        Left,
        Right,
        Top,
        Bottom,
        LeftAndRight
    }
}