using UnityEngine;

public class TowerDistributionBackground : MonoBehaviour {
    [SerializeField, Tooltip("Background Image of the pillar distribution")]
    private RectTransform _backgroundImage;

    [SerializeField] private RectTransform _fillTransformFire;
    [SerializeField] private RectTransform _fillTransformIce;
    [SerializeField] private float _offsetToTeam = 15;


    private void Update() {
        _backgroundImage.anchorMin = new Vector2(_fillTransformFire.anchorMax.x, 0);
        _backgroundImage.offsetMin = new Vector2(_offsetToTeam, 0);
        _backgroundImage.anchorMax = new Vector2(_fillTransformIce.anchorMin.x, 1);
        _backgroundImage.offsetMax = new Vector2(-_offsetToTeam, 0);
    }
}
