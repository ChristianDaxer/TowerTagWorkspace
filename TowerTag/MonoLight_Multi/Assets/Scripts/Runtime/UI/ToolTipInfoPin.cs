using JetBrains.Annotations;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(LookToMainCam))]
public class ToolTipInfoPin : MonoBehaviour {
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private TMP_Text _textField;

    public bool IsAnimatorInDefaultState => _animator.GetCurrentAnimatorStateInfo(0).IsName("DefaultState");

    private LookToMainCam _lookToMainCam;
    private static readonly int End = Animator.StringToHash("End");
    private static readonly int Start = Animator.StringToHash("Start");

    private void Awake() {
        _lookToMainCam = GetComponent<LookToMainCam>();
    }

    public void Init([CanBeNull] Transform target, string infoText, bool lookToMainCam = false, bool justRotateAroundY = false) {
        var trans = transform;
        trans.parent = target;
        trans.localPosition = Vector3.zero;
        _textField.text = infoText;
        _lookToMainCam.enabled = lookToMainCam;
        _lookToMainCam.OnlyRotateAroundYAxis = justRotateAroundY;
        StartAnimation();
    }

    public void EndAnimation() {
        if (_animator != null)
            _animator.SetTrigger(End);
    }

    [UsedImplicitly]
    public void StartAnimation() {
        _animator.SetTrigger(Start);
        _audioSource.PlayDelayed(1);
    }
}