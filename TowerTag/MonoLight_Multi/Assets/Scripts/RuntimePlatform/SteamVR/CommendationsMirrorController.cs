using HTC.UnityPlugin.StereoRendering;
using UnityEngine;
using UnityEngine.XR;

namespace Commendations {
    /// <summary>
    /// Enables the <see cref="StereoRenderer"/> mirror of the commendation scene.
    /// Also flips the scoreboard so that its reflection is correctly displayed.
    ///
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    /// </summary>
    public class CommendationsMirrorController : MonoBehaviour 
    {
        [SerializeField, Tooltip("The Scoreboard. Needs to be flipped on clients to appear correctly in the mirror.")]
        private GameObject _scoreBoard;

        [SerializeField] private GameObject _mirrorPrefab;

        [SerializeField, Tooltip("The mirror is only activated in VR.")]
        private GameObject _mirror;

        private void Start() 
        {
            #if UNITY_ANDROID
            return;
            #endif

            if (_mirror == null && _mirrorPrefab != null)
                _mirror = GameObject.Instantiate(_mirrorPrefab, null, true);

            if (XRSettings.enabled) 
            {
                Debug.Log("Activating VR commendations mirror");
                _mirror.SetActive(true);
                Vector3 localScale = _scoreBoard.transform.localScale;
                localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
                _scoreBoard.transform.localScale = localScale;
            }
        }
    }
}