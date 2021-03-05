using System;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TowerTag.Divider {
    /// <summary>
    /// Central control Script for the Divider.
    /// The first child is defined as the front side, the second child as the back side.
    /// A Divider should not have more than two sides.
    /// </summary>
    [System.Serializable]
    public class TwoFacedDivider : MonoBehaviour, IDivider, IMultiIndexedMesh, ISerializationCallbackReceiver {
        private enum DividerType {
            Transparent,
            Metal
        }

        [FormerlySerializedAs("_frontMainPreset")]
        [Header("Divider Style")]
        [Header("Front")]
        [SerializeField, Tooltip("The preset which should be used on the front main part of this divider")]
        private DividerType _frontType;

        [FormerlySerializedAs("_frontHighlightsPreset")]
        [SerializeField, Tooltip("The preset which should be used on the front highlights part of this divider")]
        private TeamID _frontTeamID;

        [FormerlySerializedAs("_backMainPreset")]
        [Header("Back")]
        [SerializeField, Tooltip("The preset which should be used on the back main part of this divider")]
        private DividerType _backType;

        [FormerlySerializedAs("_backHighlightsPreset")]
        [SerializeField, Tooltip("The preset which should be used on the back of this divider")]
        private TeamID _backTeamID;

        [SerializeField] private Material _metalMaterial;
        [SerializeField] private Material _glassMaterial;
        [SerializeField] private Renderer _frontRenderer;
        [SerializeField] private Renderer _backRenderer;
        [SerializeField] private int[] meshIndices;
        [SerializeField] private DividerEmissionDistributor distributor;

        private MaterialPropertyBlock _frontMaterialPropertyBlock;
        private MaterialPropertyBlock _backMaterialPropertyBlock;

        private static readonly int _emissionPropertyID = Shader.PropertyToID("_EmissionColor");
        private const int HighlightMaterialIndex = 1;

        private float _lastFrontValue = -1;
        private float _lastBackValue = -1;

        public int[] MeshIndices => meshIndices;
        public int GameObjectInstanceID => gameObject.GetInstanceID();

        public void ResetMeshIndices() => meshIndices = new int[0];

        public void ApplyDistributor() {
            if (!GetComponentInParent<DividerEmissionDistributor>()) {
                Debug.LogWarning("Divider: Parent has no DividerEmissionDistributor component.");
                return;
            }
                
            distributor = GetComponentInParent<DividerEmissionDistributor>();
            
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        public void ApplyDistributor(MaterialDataDistributor materialDataDistributor) {
            if (!materialDataDistributor) return;

            distributor = materialDataDistributor as DividerEmissionDistributor;
            
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        public void AppendMeshIndex(int meshIndex)
        {
            int[] temp = new int[meshIndices.Length + 1];
            System.Array.Copy(meshIndices, temp, meshIndices.Length);
            temp[temp.Length - 1] = meshIndex;
            meshIndices = temp;
        }

        public void ResetHighlight() {
            SetFrontHighlight(0, _frontTeamID);
            SetBackHighlight(0, _backTeamID);
        }

        public void SetHighlight(float value) {
            SetFrontHighlight(value, _frontTeamID);
            SetBackHighlight(value, _backTeamID);
        }

        public void SetHighlight(float value, TeamID teamID) {
            SetFrontHighlight(value, teamID);
            SetBackHighlight(value, teamID);
        }

        private void SetFrontHighlight(float value, TeamID teamID) {
            if (value != _lastFrontValue)
            {
                Color newColor = CalculateColor(value, teamID);

                if (_frontMaterialPropertyBlock == null) {
                    _frontMaterialPropertyBlock = new MaterialPropertyBlock();
                }

                if (_frontRenderer.enabled) {
                    UpdateMaterialPropertyBlock(newColor, _frontMaterialPropertyBlock, _frontRenderer);
                }
                else {
                    distributor.SetEmissionColor(meshIndices[0], newColor);
                }
                
                _lastFrontValue = value;
            }
        }

        private void SetBackHighlight(float value, TeamID teamID) {
            if (value != _lastBackValue)
            {
                Color newColor = CalculateColor(value, teamID);

                if (_backMaterialPropertyBlock == null) {
                    _backMaterialPropertyBlock = new MaterialPropertyBlock();
                }

                if (_backRenderer.enabled) {
                    UpdateMaterialPropertyBlock(newColor, _backMaterialPropertyBlock, _backRenderer);
                }
                else {
                    distributor.SetEmissionColor(meshIndices[1], newColor);
                }
                
                _lastBackValue = value;
            }
        }

        private Color CalculateColor(float value, TeamID teamID) {
            ITeam team = TeamManager.Singleton.Get(teamID);
            Color color = Color.white;

            if (team == null) {
                Debug.LogErrorFormat("The divider: \"{0}\" in scene: \"{1}\" is invalid.", gameObject.name,
                    gameObject.scene.name);
                return color;
            }

            if (team != null) {
                color = Color.Lerp(
                    team.Colors.Dark,
                    team.Colors.Effect, value);
            }

            else {
                color = Color.Lerp(
                    Color.red,
                    Color.blue, value);
            }

            return color;
        }

        private void UpdateMaterialPropertyBlock(Color color, 
                MaterialPropertyBlock materialPropertyBlock, Renderer renderer) {
            renderer.GetPropertyBlock(materialPropertyBlock, HighlightMaterialIndex);
            materialPropertyBlock.SetColor(_emissionPropertyID, color);
            renderer.SetPropertyBlock(materialPropertyBlock, HighlightMaterialIndex);
        }

        void OnValidate() {
            if (distributor.IsNull()) {
                ApplyDistributor();
            }
            
            if (gameObject.scene.buildIndex == -1) return;

            _frontRenderer.sharedMaterials = new[] {
                _frontType == DividerType.Metal ? _metalMaterial : _glassMaterial,
                _frontRenderer.sharedMaterials[HighlightMaterialIndex]
            };

            _backRenderer.sharedMaterials = new[] {
                _backType == DividerType.Metal ? _metalMaterial : _glassMaterial,
                _backRenderer.sharedMaterials[HighlightMaterialIndex]
            };
            

            if (!_frontRenderer.CompareTag(DividerType.Transparent == _frontType ? "Dec_Energy" : "Dec_Metal"))
                _frontRenderer.gameObject.tag = DividerType.Metal == _frontType ? "Dec_Metal" : "Dec_Energy";

            if (!_backRenderer.CompareTag(DividerType.Transparent == _backType ? "Dec_Energy" : "Dec_Metal"))
                _backRenderer.gameObject.tag = DividerType.Metal == _backType ? "Dec_Metal" : "Dec_Energy";
            
        }
        
        
        // Divider serialization bugfix, because "2" was stored originally in the scene for team Fire (2 is not in enum range, team Fire has int value = 0)
        public void OnBeforeSerialize() {
            if(!Enum.IsDefined(typeof(TeamID), _backTeamID)) {
                Debug.Log("Manual serialization fix of TeamID enum on two faced divider.");
                _backTeamID = (int) TeamID.Fire; 
            } 
            if (!Enum.IsDefined(typeof(TeamID), _frontTeamID)) {
                Debug.Log("Manual serialization fix of TeamID enum on two faced divider.");
                _frontTeamID = (int) TeamID.Fire;
            }
        }

        public void OnAfterDeserialize() {
        }
    }
}