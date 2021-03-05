using TowerTag;
using UnityEngine;

public enum ShieldType
{
    Outside,
    Inside
}

public class ReadyTowerShield : MonoBehaviour
{
    [SerializeField] private MeshRenderer _shieldInsideRenderer;
    [SerializeField] private MeshRenderer _shieldOutsideRenderer;
    [SerializeField] private GameObject _shieldInside;
    [SerializeField] private GameObject _shieldOutside;

    public void Activate(ITeam team, ShieldType shieldType)
    {
        ColorChanger.ChangeColorInRendererComponents(new Renderer[] { _shieldInsideRenderer, _shieldOutsideRenderer }, team.Colors.Effect, Shader.PropertyToID("_EmissionColor"));

        if (shieldType.Equals(ShieldType.Inside)) _shieldInside.SetActive(true);
        else _shieldOutside.SetActive(true);
    }
}
