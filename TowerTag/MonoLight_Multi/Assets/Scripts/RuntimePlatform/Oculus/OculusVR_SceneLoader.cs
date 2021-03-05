using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRNerdsUtilities;
using UnityEngine.Events;

public class OculusVR_SceneLoader : SingletonMonoBehaviour<OculusVR_SceneLoader>
{
    //Oculus class for compositor layers.
    //We need one for the background and one per object in for the foreground (loading message/progress bar), to a limit of a total of 15 overlays
    //If more than 15 overlays are needed, some overlays should be combined in a rendertexture
    //The background overlay requires a cubemap, the loading overlays a simple texture.
    [SerializeField]
    private OVROverlay _backgroundOverlay;
    [SerializeField]
    private OVROverlay _loadingOverlay;
    [SerializeField]
    private OVROverlay _loadingScreenOverlay;

    [SerializeField]
    private Texture _progressBarEmpty;
    [SerializeField]
    private Texture _progressBarFull;

    private RenderTexture _loadingRenderTexture; // used to render progress bar

    private bool _loading;

    //public static bool Loading { get { return OculusVR_SceneLoader.Instance._loading; } }

    // If true, call LoadLevelAdditiveAsync instead of LoadLevelAsync.
    public bool loadAdditive;

    // Async load causes crashes in some apps.
    public bool loadAsync = true;

    // Additional time to wait after finished loading before we start fading the new scene back in.
    // This is to cover up any initial hitching that takes place right at the start of levels.
    // Most scenes should hopefully not require this.
    public float postLoadSettleTime = 0.0f;

    // Time to fade loading screen in and out (also used for progress bar).
    public float loadingScreenFadeInTime = 1.0f;
    public float loadingScreenFadeOutTime = 0.25f;

    // Sizes of overlays.
    public float loadingScreenWidthInMeters = 3.0f; //TODO implement another Overlay for loading image
    public float loadingScreenHeightInMeters = 3.0f;
    public float progressBarWidthInMeters = 3.0f;
    public float progressBarHeightInMeters = 0.5f;

    // If specified, the loading screen will be positioned in the player's view this far away.
    public float loadingScreenDistance = 0.0f;

    float fadeRate = 1.0f;
    float alpha = 0.0f;

    public string levelName;

    [SerializeField]
    private Texture[] _gameTips;

    private System.IntPtr[] _nativeTexturePtr; //Used to cache the game tips texture native ptr as GetNativeTexturePtr is rather slow

    AsyncOperation async; // used to track level load progress

    bool renderTextureIsDirty = false;
                          

    public static OculusEvent<bool> Loading = new OculusEvent<bool>();

    protected override void Awake() {
        base.Awake();

        if (_progressBarEmpty != null) { 

            _loadingRenderTexture = new RenderTexture(_progressBarEmpty.width, _progressBarEmpty.height, 0);
            _loadingRenderTexture.Create();

            if (_loadingOverlay != null)
                _loadingOverlay.textures[0] = _loadingRenderTexture;

            else Debug.LogErrorFormat("Missing reference to loading overlay on component: {0} attached to GameObject: \"{1}\".", nameof(OculusVR_SceneLoader), gameObject.name);
        }

        else Debug.LogErrorFormat("Missing reference to progress bar on component: {0} attached to GameObject: \"{1}\".", nameof(OculusVR_SceneLoader), gameObject.name);

        if (_backgroundOverlay == null)
            Debug.LogErrorFormat("Missing reference to background overlay on component: {0} attached to GameObject: \"{1}\".", nameof(OculusVR_SceneLoader), gameObject.name);

        if (_gameTips.Length > 0) {
            _nativeTexturePtr = new System.IntPtr[_gameTips.Length];

            for (int i = 0; i < _gameTips.Length; i++) {
                _nativeTexturePtr[i] = _gameTips[i].GetNativeTexturePtr();
            }
        }
    }

    public static float Progress
    {
        get { return (Instance != null && Instance.async != null) ? Instance.async.progress : 0.0f; }
    }

    // Fade our overlays in/out over time.
    void Update()
    {
        if (!_loading)
            return;

        alpha = Mathf.Clamp01(alpha + fadeRate * Time.deltaTime);

        if (_backgroundOverlay != null)
            _backgroundOverlay.colorScale = new Vector4(1, 1, 1, alpha);

        if (_loadingOverlay != null)
            _loadingOverlay.colorScale = new Vector4(1, 1, 1, alpha);

        if (_loadingScreenOverlay != null)
            _loadingScreenOverlay.colorScale = new Vector4(1, 1, 1, alpha);
    }

    public void Trigger()
    {
        if (!_loading && !string.IsNullOrEmpty(levelName))
            StartCoroutine(LoadLevel());
    }

    public static void Begin(string levelName,
            bool showGrid = false, float fadeOutTime = 0.5f,
            float r = 0.0f, float g = 0.0f, float b = 0.0f, float a = 1.0f)
    {
        OculusVR_SceneLoader loader = OculusVR_SceneLoader.Instance;
        
        if (loader._loadingScreenOverlay != null && loader._nativeTexturePtr.Length > 0) {
            System.Random rdm = new System.Random();
            int index = rdm.Next(loader._nativeTexturePtr.Length);

            loader._loadingScreenOverlay.OverrideOverlayTextureInfo(loader._gameTips[index], loader._nativeTexturePtr[index], UnityEngine.XR.XRNode.LeftEye);
        }
        loader.levelName = levelName;
        loader.Trigger();
    }

    private PlayerHeadBase playerHeadBase;
    IEnumerator LoadLevel()
    {
        if (playerHeadBase == null)
        {
            if (!PlayerHeadBase.GetInstance(out playerHeadBase))
            {
                Debug.LogErrorFormat($"No singleton instance of: {typeof(PlayerHeadBase).FullName} available in the scene.");
                yield break;
            }
        }

        

        if (_loadingOverlay != null) {
            Quaternion centerEyeRotation = Quaternion.Euler(0.0f, playerHeadBase.transform.rotation.eulerAngles.y, 0.0f);
            Vector3 centerEyePosition = centerEyeRotation * new Vector3(0.0f, playerHeadBase.transform.position.y - ((loadingScreenHeightInMeters / 2) + (progressBarHeightInMeters / 2) + 0.1f), loadingScreenDistance);
            Vector3 overlayScale = new Vector3(progressBarWidthInMeters, progressBarHeightInMeters, 0f);

            _loadingOverlay.transform.localPosition = centerEyePosition;
            _loadingOverlay.transform.rotation = centerEyeRotation;
            _loadingOverlay.transform.localScale = overlayScale;
        }
        else Debug.LogErrorFormat($"{typeof(OculusVR_SceneLoader).FullName} attached to: \"{gameObject.name}\" is missing a reference to a OVR loading overlay.");

        if (_loadingScreenOverlay != null) {
            Quaternion centerEyeRotation = Quaternion.Euler(0, playerHeadBase.transform.rotation.eulerAngles.y, 0);
            Vector3 centerEyePosition = centerEyeRotation * new Vector3(0.0f, playerHeadBase.transform.position.y, loadingScreenDistance);
            Vector3 overlayScale = new Vector3(-1 * loadingScreenWidthInMeters, loadingScreenHeightInMeters, 0f);

            _loadingScreenOverlay.transform.localPosition = centerEyePosition;
            _loadingScreenOverlay.transform.rotation = centerEyeRotation;
            _loadingScreenOverlay.transform.localScale = overlayScale;
        }
        else Debug.LogErrorFormat($"{typeof(OculusVR_SceneLoader).FullName} attached to: \"{gameObject.name}\" is missing a reference to a OVR loading overlay.");

        Debug.LogFormat($"{typeof(OculusVR_SceneLoader).FullName}: Started level loading.");


        _loading = true;
        Loading.Invoke(true);

        if (loadingScreenFadeInTime > 0.0f)
            fadeRate = 1.0f / loadingScreenFadeInTime;
        else alpha = 1.0f;
            
        if (_loadingOverlay != null)
            _loadingOverlay.enabled = true;

        if (_loadingScreenOverlay != null)
            _loadingScreenOverlay.enabled = true;

        if (_backgroundOverlay != null)
            _backgroundOverlay.enabled = true;

        //Waits until overlay fully faded in
        while (alpha < 1.0f)
            yield return null;

        var mode = loadAdditive ? UnityEngine.SceneManagement.LoadSceneMode.Additive : UnityEngine.SceneManagement.LoadSceneMode.Single;
        if (loadAsync)
        {
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(levelName, mode);

            // Performing this in a while loop instead seems to help smooth things out.
            //yield return async;
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.5f);
            while (!async.isDone)
            {
                Debug.LogFormat($"{typeof(OculusVR_SceneLoader).FullName}: Waiting for asynchronous loading of level: \"{levelName}\".");
                yield return waitForSeconds;
            }
        }

        else
        {
            Debug.LogFormat($"{typeof(OculusVR_SceneLoader).FullName}: Started synchronous loading of level: \"{levelName}\".");
            UnityEngine.SceneManagement.SceneManager.LoadScene(levelName, mode);
        }

        Debug.LogFormat($"{typeof(OculusVR_SceneLoader).FullName}: Finished loading level: \"{levelName}\".");

        System.GC.Collect();
        yield return new WaitForSeconds(postLoadSettleTime);

        if (loadingScreenFadeOutTime > 0.0f)
            fadeRate = -1.0f / loadingScreenFadeOutTime;
        else alpha = 0.0f;

        //Wait for the overlays to fully fade out
        while (alpha > 0.0f) {
            yield return null;
        }

        if (_backgroundOverlay != null)
            _backgroundOverlay.enabled = false;

        if (_loadingOverlay)
            _loadingOverlay.enabled = false;

        if (_loadingScreenOverlay)
            _loadingScreenOverlay.enabled = false;

        renderTextureIsDirty = true;

        _loading = false;
        Loading.Invoke(false);
        async = null;
    }

    private void OnGUI() {
        if (renderTextureIsDirty) {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = _loadingRenderTexture;

            if (Event.current.type == EventType.Repaint) {
                GL.Clear(false, true, Color.clear);
                renderTextureIsDirty = false;
            }
                
            RenderTexture.active = rt;
        }

        if (!_loading || _progressBarFull == null || _progressBarEmpty == null 
            || _loadingRenderTexture == null)
            return;

        float progress = Progress;
        float w = _progressBarFull.width;
        float h = _progressBarFull.height;

        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = _loadingRenderTexture;

        if(Event.current.type == EventType.Repaint)
            GL.Clear(false, true, Color.clear);

        GUILayout.BeginArea(new Rect(0, 0, w, h));

        GUI.DrawTexture(new Rect(0, 0, w, h), _progressBarEmpty);

        // Reveal the full bar texture based on progress.
        GUI.DrawTextureWithTexCoords(new Rect(0, 0, progress * w, h), _progressBarFull, new Rect(0.0f, 0.0f, progress, 1.0f));

        GUILayout.EndArea();

        RenderTexture.active = previousActive;
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        StopAllCoroutines();
    }

    public class OculusEvent<T> : UnityEvent<T>
    {
    }
}
