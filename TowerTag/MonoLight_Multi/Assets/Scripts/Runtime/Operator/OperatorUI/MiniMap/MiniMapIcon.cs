using TowerTag;
using UnityEngine;
using UnityEngine.UI;

public abstract class MiniMapIcon : MonoBehaviour
{
    [SerializeField] protected Image _controlledImage;
    [SerializeField] protected Transform _objectToFollow;
    private IPlayer _player;
    protected IPlayer Player => _player ?? (_player = GetComponentInParent<IPlayer>());
    private void Awake() {
        _player = GetComponentInParent<IPlayer>();
    }

    protected void OnEnable() {
        GameManager.Instance.MatchHasFinishedLoading += PaintIcons;
        PaintIcons(GameManager.Instance.CurrentMatch);
    }

    protected void OnDisable() {
        GameManager.Instance.MatchHasFinishedLoading -= PaintIcons;
    }

    protected void Update() {
        if (_objectToFollow != null)
            _controlledImage.rectTransform.rotation = Quaternion.LookRotation(Vector3.up, _objectToFollow.forward);
    }

    protected virtual void PaintIcons(IMatch match) {
        if (_controlledImage != null) {
            _controlledImage.color = TeamManager.Singleton.Get(Player.TeamID).Colors.UI;
        }
    }
}