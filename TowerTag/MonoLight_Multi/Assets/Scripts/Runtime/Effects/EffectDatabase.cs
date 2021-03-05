using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.Serialization;
using VRNerdsUtilities;

namespace TowerTag {
    public class EffectDatabase : SingletonMonoBehaviour<EffectDatabase> {
        private static Dictionary<string, ObjectPool[]> _dataBase;

        [FormerlySerializedAs("decalTags")] [SerializeField] private string[] _decalTags;
        [FormerlySerializedAs("decalPrefabs")] [SerializeField] private GameObject[] _decalPrefabs;
        [FormerlySerializedAs("decalPoolSizes")] [SerializeField] private int[] _decalPoolSizes;

        public string[] DecalTags {
            get => _decalTags;
            set => _decalTags = value;
        }

        public GameObject[] DecalPrefabs {
            get => _decalPrefabs;
            set => _decalPrefabs = value;
        }

        public int[] DecalPoolSizes {
            get => _decalPoolSizes;
            set => _decalPoolSizes = value;
        }

        private new void Awake() {
            base.Awake();
            InitDatabase();
        }

        private void InitDatabase() {
            if (_decalTags.Length != _decalPrefabs.Length) {
                Debug.LogWarning("DecalDatabase: number of tags and prefabs not equal. Database denied to build");
                return;
            }

            _dataBase = new Dictionary<string, ObjectPool[]>();

            for (var i = 0; i < _decalTags.Length; i++) {
                string decalTag = _decalTags[i];
                if (_dataBase.ContainsKey(decalTag)) {
                    ObjectPool[] pool = _dataBase[decalTag];
                    var newPool = new ObjectPool[pool.Length + 1];
                    pool.CopyTo(newPool, 0);
                    newPool[newPool.Length - 1] = new ObjectPool(_decalPoolSizes[i], _decalPrefabs[i], transform, true);
                    _dataBase[decalTag] = newPool;
                }
                else {
                    var pool = new ObjectPool[1];
                    pool[0] = new ObjectPool(_decalPoolSizes[i], _decalPrefabs[i], transform, true);
                    _dataBase.Add(decalTag, pool);
                }
            }
        }

        public static GameObject PlaceDecal(string tag, Vector3 position, Quaternion rotation,
            bool randomRotateAroundZ = false) {
            if (string.IsNullOrEmpty(tag))
                return null;

            if (_dataBase == null)
                return null;

            if (_dataBase.ContainsKey(tag)) {
                ObjectPool[] pools = _dataBase[tag];
                GameObject go = pools[Random.Range(0, pools.Length)].CreateGameObject(position, rotation);

                if (go != null) {
                    if (randomRotateAroundZ) {
                        go.transform.rotation =
                            Quaternion.AngleAxis(Random.Range(0, 360), go.transform.forward) * rotation;
                    }

                    go.SetActive(false);
                    go.SetActive(true);
                    return go;
                }
            }

            // Debug.LogWarning ("Tag (" + tag + ") not in Database!");
            return null;
        }

        [UsedImplicitly]
        public static GameObject PlaceDecal(string tag, Vector3 position, Quaternion rotation, IPlayer player,
            bool randomRotateAroundZ = false) {
            GameObject newDecal = PlaceDecal(tag, position, rotation, randomRotateAroundZ);

            if (newDecal == null)
                return null;

            if (player == null)
                return newDecal;

            ITeam team = TeamManager.Singleton.Get(player.TeamID);
            if (team != null) {
                ColorChanger.ChangeColorInChildRendererComponents(newDecal, team.Colors.Main, true);

                // tint glow Gradients in metalDecals
                FPSShaderColorGradient[] colorGradients = newDecal.GetComponentsInChildren<FPSShaderColorGradient>();
                if (colorGradients != null) {
                    foreach (FPSShaderColorGradient colorGradient in colorGradients) {
                        SetGradientColor(colorGradient.Color, 0, team.Colors.Main);
                    }
                }
            }

            return newDecal;
        }

        private static void SetGradientColor(Gradient gradient, int colorIndex, Color color) {
            if (gradient != null) {
                GradientColorKey[] colors = gradient.colorKeys;
                if (colorIndex >= 0 && colorIndex < colors.Length) {
                    colors[colorIndex].color = color;
                    gradient.colorKeys = colors;
                }
            }
        }

        private void OnDestroy() {
            if (_dataBase != null) {
                string[] keys = _dataBase.Keys.ToArray();
                foreach (string key in keys) {
                    ObjectPool[] pools = _dataBase[key];
                    if (pools != null) {
                        foreach (ObjectPool pool in pools) {
                            pool.Dispose();
                        }
                    }
                }

                _dataBase.Clear();
                _dataBase = null;
            }

            _decalTags = null;
            _decalPrefabs = null;
            _decalPoolSizes = null;
        }
    }
}