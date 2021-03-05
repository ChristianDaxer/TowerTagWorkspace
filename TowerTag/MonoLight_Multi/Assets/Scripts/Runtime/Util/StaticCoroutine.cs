using UnityEngine;
using System.Collections;

public class StaticCoroutine : MonoBehaviour {
    private static StaticCoroutine _instance;

    public static Coroutine StartStaticCoroutine(IEnumerator coroutineToStart) {
        if (_instance == null)
            _instance = CreateInstance();

        return _instance.StartCoroutine(coroutineToStart);
    }

    public static void StopStaticCoroutine(Coroutine coroutineToStop) {
        if (_instance == null)
            _instance = CreateInstance();

        _instance.StopCoroutine(coroutineToStop);
    }

    private void Awake() {
        _instance = this;
    }

    private void OnDestroy() {
        if (_instance != null) {
            _instance.StopAllCoroutines();
            Destroy(_instance.gameObject);
            _instance = null;
        }
    }

    private static StaticCoroutine CreateInstance() {
        var coroutineRunner = new GameObject("StaticCoroutine") {hideFlags = HideFlags.HideInHierarchy};
        return coroutineRunner.AddComponent<StaticCoroutine>();
    }
}