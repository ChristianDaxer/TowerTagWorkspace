using System;
using System.Collections;
using System.Collections.Generic;
#if !UNITY_ANDROID
using System.Windows.Forms;
#endif
using UnityEngine;

public class ProjectorMaterialAnimator : MonoBehaviour {
    [SerializeField] private Renderer _renderer;

    [SerializeField] private Texture[] _sprites;

    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private float _animationDuration;
    private Coroutine _animationCoroutine;

    private void OnEnable() {
        Keyframe[] keys = _curve.keys;
        keys[0] = new Keyframe(0, 0);
        keys[keys.Length-1]= new Keyframe(_animationDuration, _sprites.Length-1);
        _curve.keys = keys;
        StartAnimation();
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    [UnityEngine.ContextMenu("PlayAnimation")]
    private void StartAnimation() {
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);
        _animationCoroutine = StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation() {
        float timer = 0;
        int spriteIndex = 0;
        while (timer <= _animationDuration) {
            if(_curve.Evaluate(timer) >= spriteIndex) {
                _renderer.material.mainTexture = _sprites[spriteIndex];
                spriteIndex++;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        _animationCoroutine = null;
    }
}
