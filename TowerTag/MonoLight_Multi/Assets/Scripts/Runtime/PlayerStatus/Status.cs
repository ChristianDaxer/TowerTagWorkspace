using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
[CreateAssetMenu(menuName = "Shared/TowerTag/PlayerStatus")]
public class Status : ScriptableObject {
    [SerializeField, FormerlySerializedAs("StatusText")] private string _statusText;
    [SerializeField, FormerlySerializedAs("StatusColor")] private Color _statusColor;

    public string StatusText => _statusText;
    public Color StatusColor => _statusColor;
}
