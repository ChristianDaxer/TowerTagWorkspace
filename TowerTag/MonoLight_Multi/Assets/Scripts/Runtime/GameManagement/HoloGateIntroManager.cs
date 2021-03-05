using System.Collections;
using System.Diagnostics;
using TowerTagSOES;
using UI;
using UnityEngine;
using UnityEngine.Video;

namespace Hologate{
    public class HoloGateIntroManager : MonoBehaviour {
        [SerializeField] private VideoPlayer _introPlayer;
        private float _maxTime = 5f;

        private void Start() {
            StartCoroutine(ConnectAfterVideoFinished());
        }

        private IEnumerator ConnectAfterVideoFinished() {
            float time = 0;
            while (_introPlayer.isPlaying) {
                yield return null;
            }
            if (SharedControllerType.IsAdmin) {
                while (time <= _maxTime && Process.GetProcessesByName("PhotonSocketServer").Length <= 0) {
                    time += Time.deltaTime;
                    yield return null;
                }

                if (time >= _maxTime) {
                    MessageQueue.Singleton.AddErrorMessage("Could not start a Photon Instance, please restart!");
                    yield break;
                }
            }

            ConnectionManager.Instance.Connect();
        }
    }
}
