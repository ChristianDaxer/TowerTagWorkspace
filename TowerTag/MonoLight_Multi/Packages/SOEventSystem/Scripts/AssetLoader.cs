using System.Collections.Generic;
using UnityEngine;

namespace SOEventSystem {
    /// <summary>
    /// Holds a list of references to assets to have them loaded by the Unity engine.
    /// Put this into a scene to load assets and thereby trigger their OnEnable callbacks.
    ///
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    /// </summary>
    public class AssetLoader : MonoBehaviour {
        [SerializeField] private List<Object> _assets;
    }
}