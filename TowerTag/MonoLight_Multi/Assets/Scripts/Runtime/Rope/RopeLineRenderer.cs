using System.Collections;
using TowerTag;
using UnityEngine;


namespace Rope {
    /// <summary>
    /// Displays the rope via LineRenderer on the MiniMap
    /// </summary>
    public class RopeLineRenderer : MonoBehaviour {
        [SerializeField] private ChargerRopeRenderer _ropeTess;
        [SerializeField] private LineRenderer _lineRenderer;
        private IPlayer _owner;
        private Coroutine _ropeCoroutine;

        private void OnEnable() {
            if (_ropeTess == null) {
                Debug.LogErrorFormat("Cannot add delegates to: \"{0}\", it was destroyed or never referenced.", nameof(ChargerRopeRenderer));
                return;
            }
            _ropeTess.RollingOut += OnRollingOutStarted;
            _ropeTess.RollingIn += OnRollingInStarted;
        }

        private void OnDisable() {
            if (_ropeTess == null) {
                Debug.LogErrorFormat("Cannot remove delegates from: \"{0}\", it was destroyed or never referenced.", nameof(ChargerRopeRenderer));
                return;
            }

            _ropeTess.RollingOut -= OnRollingOutStarted;
            _ropeTess.RollingIn -= OnRollingInStarted;
        }

        private void Start() {
            if (_ropeTess == null) { 
                _owner = _ropeTess.GetOwner();
                _owner.PlayerTeamChanged += OnPlayerTeamChanged;
                _lineRenderer.material = TeamMaterialManager.Singleton.GetFlatUI(_owner.TeamID);
            }
        }

        private void OnPlayerTeamChanged(IPlayer player, TeamID teamID) {
            _lineRenderer.material = TeamMaterialManager.Singleton.GetFlatUI(teamID);
        }

        private void OnRollingInStarted() {
            if(_ropeCoroutine != null) {
                StopCoroutine(_ropeCoroutine);
                _ropeCoroutine = null;
                _lineRenderer.SetPosition(0, Vector3.zero);
                _lineRenderer.SetPosition(1, Vector3.zero);
            }
        }

        private void OnRollingOutStarted() {
            if(_ropeCoroutine == null)
                _ropeCoroutine = StartCoroutine(UpdateLineRenderer());
        }

        private IEnumerator UpdateLineRenderer() {
            yield return new WaitForEndOfFrame();
            while (true) {
                _lineRenderer.SetPosition(0, _ropeTess.SpawnBeamAnchor.position);
                _lineRenderer.SetPosition(1, _ropeTess.HookAsset.position);
                yield return null;
            }
        }

        private void OnDestroy() {
            if(_owner != null)
                _owner.PlayerTeamChanged -= OnPlayerTeamChanged;
        }
    }
}