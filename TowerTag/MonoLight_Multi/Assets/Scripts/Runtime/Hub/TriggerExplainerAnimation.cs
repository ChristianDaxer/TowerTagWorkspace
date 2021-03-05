using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TriggerExplainerAnimation : MonoBehaviour {
    private Animator _animator;
    private static readonly int _animation = Animator.StringToHash("animation");

    private void Awake() {
        _animator = GetComponent<Animator>();
    }

    public void SwitchToAnimation(int animationIndex) {
        _animator.SetInteger(_animation, animationIndex);
    }
}