using System.Collections.Generic;
using System.Linq;
using SOEventSystem.Shared;
using UnityEngine;

namespace SOEventSystem.Addressable {
    /// <summary>
    /// A Database to hold all enabled <see cref="AddressableAsset"/> assets for reference.
    /// </summary>
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    [CreateAssetMenu(menuName = "AddressableAssetDatabase")]
    public class AddressableAssetDatabase : SharedSingleton<AddressableAssetDatabase>, ISerializationCallbackReceiver {
        [SerializeField] private List<AddressableAsset> _addressableAssets = new List<AddressableAsset>();
        private Dictionary<string, AddressableAsset> _assetMap = new Dictionary<string, AddressableAsset>();

        public List<AddressableAsset> AddressableAssets => _addressableAssets;

        /// <summary>
        /// Registers an <see cref="AddressableAsset"/>.
        /// </summary>
        public void Register(AddressableAsset addressableAsset) {
            if (!_addressableAssets.Contains(addressableAsset)) _addressableAssets.Add(addressableAsset);
            _assetMap[addressableAsset.AssetGuid] = addressableAsset;
        }

        /// <summary>
        /// Unregisters an <see cref="AddressableAsset"/>.
        /// </summary>
        public void Unregister(AddressableAsset addressableAsset) {
            if (_addressableAssets.Contains(addressableAsset)) _addressableAssets.Remove(addressableAsset);
            _assetMap.Remove(addressableAsset.AssetGuid);
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() {
            _assetMap = _addressableAssets.Where(asset => asset != null).ToDictionary(asset => asset != null ? asset.AssetGuid : "", asset => asset);
        }

        public AddressableAsset Get(string guid) {
            return !_assetMap.ContainsKey(guid) ? null : _assetMap[guid];
        }
    }
}