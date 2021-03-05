using UnityEngine;

[CreateAssetMenu(menuName = "MapTheme")]
public class MapTheme : ScriptableObject {
    [SerializeField] private Soundtrack _soundtrack;
    [SerializeField] private Material _skyboxMaterial;

    public void Init(AudioSource source) {
        if (GameManager.Instance.CurrentMatch == null) {
            Debug.LogError("Could not chose a theme! CurrentMatch is null");
            return;
        }

        if(RenderSettings.skybox != _skyboxMaterial)
            RenderSettings.skybox = _skyboxMaterial;

        int matchTimeInMinutes = GameManager.Instance.CurrentMatch.MatchTimeInSeconds / 60;
        source.clip = _soundtrack.GetClipByLength(matchTimeInMinutes);
        source.volume = _soundtrack.Volume;
#if UNITY_ANDROID
        source.spatialize = true;
#endif
    }
}
