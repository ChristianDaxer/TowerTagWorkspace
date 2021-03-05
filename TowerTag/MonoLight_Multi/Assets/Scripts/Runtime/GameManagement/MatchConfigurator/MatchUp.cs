using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "MatchUp")]
public class MatchUp : ScriptableObject {

    [FormerlySerializedAs("name"), SerializeField] private string _name;
    [FormerlySerializedAs("maxPlayers"), SerializeField] private int _maxPlayers;

    public string Name => _name;
    public int MaxPlayers => _maxPlayers;
}
