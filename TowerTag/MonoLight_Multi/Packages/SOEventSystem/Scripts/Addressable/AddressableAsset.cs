using System;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

#endif

namespace SOEventSystem.Addressable {
    /// <summary>
    /// An addressable asset that serializes its asset id and registers at a respective database.
    /// </summary>
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    public abstract class AddressableAsset : ScriptableObject {
        [SerializeField, HideInInspector] private string _assetGuid;

        public string AssetGuid => _assetGuid;

#if UNITY_EDITOR
        public void OnValidate() {
            _assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this));
        }
#endif

        protected void OnEnable() {
#if UNITY_EDITOR
            _assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this));
#else
            if (_assetGuid == null) _assetGuid = Guid.NewGuid().ToString();
#endif
            if (AddressableAssetDatabase.Singleton != null) AddressableAssetDatabase.Singleton.Register(this);
        }

        protected void OnDisable() {
            if (AddressableAssetDatabase.Singleton != null) AddressableAssetDatabase.Singleton.Unregister(this);
        }
    }
}