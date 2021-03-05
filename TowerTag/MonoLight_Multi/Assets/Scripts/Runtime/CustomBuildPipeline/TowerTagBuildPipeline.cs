using System.Linq;
using Photon.Pun;
using TowerTagSOES;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomBuildPipeline {
    [CreateAssetMenu(menuName = "Build Pipeline/Build Pipeline")]
    public class TowerTagBuildPipeline : ScriptableObject {

#if UNITY_EDITOR
        private static TowerTagBuildPipeline cachedInstance;
        public static bool GetFirstInstance (out TowerTagBuildPipeline pipeline)
        {
            if (cachedInstance == null)
            {
                cachedInstance = AssetDatabase.FindAssets($"t:{nameof(TowerTagBuildPipeline)}")
                    .Select(guid => AssetDatabase.LoadAssetAtPath<TowerTagBuildPipeline>(AssetDatabase.GUIDToAssetPath(guid)))
                    .First();
            }

            pipeline = cachedInstance;
            return cachedInstance != null;
        }
#endif

        [Header("Asset References")] [SerializeField]
        private SharedVersion _sharedVersion;

        [SerializeField] private SharedControllerType _controllerType;
        [SerializeField, HideInInspector] private int[] _compositionIndices = new int[4];
        [SerializeField, HideInInspector] private string _executableName;
        [SerializeField, HideInInspector] private bool _developmentBuild;
        [SerializeField] private Sprite _proSprite;
        [SerializeField] private Sprite _basicSprite;
        [SerializeField] private Sprite _homeSprite;
        [SerializeField] private ServerSettings _photonServerSettings;

        public int GetCompositionIndice (int index) { return _compositionIndices[index]; }
        public void SetCompositionIndice (int index, int compositionIndice)
        {
            _compositionIndices[index] = compositionIndice;
        }

        public SharedVersion SharedVersion => _sharedVersion;
        public SharedControllerType ControllerType => _controllerType;
        public string ExecutableName => _executableName;

        public bool DevelopmentBuild {
            get => _developmentBuild;
            set {
                if (_developmentBuild == value) return;
                _developmentBuild = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public bool BasicMode {
            get => TowerTagSettings.BasicMode;
            set => TowerTagSettings.BasicMode = value;
        }

        public bool Home {
            get => TowerTagSettings.Home;
            set => TowerTagSettings.Home = value;
        }
        
        public HomeTypes HomeType {
            get => TowerTagSettings.HomeType;
            set => TowerTagSettings.HomeType = value;
        }
        
        public bool HologateSetting
        {
            get => TowerTagSettings.Hologate;
            set => TowerTagSettings.Hologate = value;
        }
        
        public bool SteamEditorId
        {
            get => TowerTagSettings.SteamEditorId;
            set => TowerTagSettings.SteamEditorId = value;
        }

        public Sprite ProSprite => _proSprite;

        public Sprite BasicSprite => _basicSprite;


        public Sprite HomeSprite => _homeSprite;

        public ServerSettings PhotonServerSettings => _photonServerSettings;
    }
}