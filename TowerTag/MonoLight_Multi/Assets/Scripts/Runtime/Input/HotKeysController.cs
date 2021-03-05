using UnityEngine;

/// <summary>
/// Controller that runs the coroutine for listening to hot key input.
/// Manages a <see cref="HotKeysImpl"/> event system asset.
///
/// <author>Ole Jürgensen (ole@vrnerds.de)</author>
/// </summary>
public class HotKeysController : MonoBehaviour {
    [SerializeField, Tooltip("Hot Keys event system asset")]
    private HotKeys _hotKeys;

    private void OnEnable() {
#if !UNITY_ANDROID
        StartCoroutine(_hotKeys.Listen());
#endif
    }

    private void OnDisable() {
        StopAllCoroutines();
    }
}