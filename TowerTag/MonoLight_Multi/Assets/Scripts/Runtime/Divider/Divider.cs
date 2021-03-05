using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TowerTag.Divider {
    [System.Serializable]
    public class Divider : MonoBehaviour, IDivider, IMultiIndexedMesh {
        [SerializeField] private TeamID _teamID;
        [SerializeField] private Renderer _tintRenderer;
        [SerializeField] private float _defaultHighlight;
        private MaterialPropertyBlock _propertyBlock;
        private static readonly int _emissionPropertyID = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private int[] meshIndices;
        public int[] MeshIndices => meshIndices;
        public int GameObjectInstanceID => gameObject.GetInstanceID();
        [SerializeField] private DividerEmissionDistributor distributor;
        public void ApplyDistributor(MaterialDataDistributor materialDataDistributor)
        {
            distributor = materialDataDistributor as DividerEmissionDistributor;
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        public void AppendMeshIndex(int meshIndex)
        {
            int[] temp = new int[meshIndices.Length + 1];
            if (meshIndices.Length > 0)
                System.Array.Copy(meshIndices, temp, meshIndices.Length);
            temp[temp.Length - 1] = meshIndex;
            meshIndices = temp;
        }

        public void ResetMeshIndices() => meshIndices = new int[0];

        private void OnEnable() {
            ResetHighlight();
        }

        private void OnValidate() {
            ApplyDistributor(GetComponentInParent<DividerEmissionDistributor>());
            if (gameObject.scene.buildIndex == -1) return;
            if(TeamManager.Singleton != null)
                ResetHighlight();
        }

        public void ResetHighlight() {
            SetHighlight(_defaultHighlight);
        }

        public void SetHighlight(float value) {
            SetHighlight(value, _teamID);
        }

        public void SetHighlight(float value, TeamID teamID) {
            ITeam team = TeamManager.Singleton.Get(teamID);
            Color color = Color.white;
            if (team != null)
            {
                color = Color.Lerp(
                    team.Colors.Dark,
                    team.Colors.Effect, value);
            }

            else
            {
                color = Color.Lerp(
                    Color.red,
                    Color.blue, value);
            }

            if (_tintRenderer.enabled)
            {
                _propertyBlock = _propertyBlock ?? new MaterialPropertyBlock();
                _tintRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(_emissionPropertyID,
                    Color.Lerp(TeamManager.Singleton.Get(teamID).Colors.Dark,
                        TeamManager.Singleton.Get(teamID).Colors.Effect, value));
                _tintRenderer.SetPropertyBlock(_propertyBlock);
            }

            else distributor.SetEmissionColor(meshIndices[0], color);
        }
    }
}