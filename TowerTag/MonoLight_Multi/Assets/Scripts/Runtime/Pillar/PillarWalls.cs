using UnityEngine;

/// <summary>
/// Class to hold all walls of a Pillar (to ensure the walls get serialized in correct order at runtime).
/// </summary>
public class PillarWalls : MonoBehaviour
{
    [SerializeField, Tooltip("Add all walls here so they can get serialized (every wall needs a WallDamageHandler_Base or derived component)!")]
    private PillarWall[] _walls;

    public PillarWall[] Walls => _walls;

    /// <summary>
    /// Initializes this class and calls Init on all walls.
    /// </summary>
    public void Init() {
        if (_walls == null) return;
        for(var i = 0; i < _walls.Length; i++)
        {
            if (_walls[i] == null) {
                Debug.LogError("PillarWalls.Init: Can't Init wall(" + i + ") because it is null!");
                continue;
            }

            _walls[i].Init();
        }
    }
}
