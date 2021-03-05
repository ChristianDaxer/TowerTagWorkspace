using TowerTag;

public class RotatePlayspaceHandler {
    public delegate void RotatePlaySpaceDelegate(object sender, RotatePlaySpaceHook target);

    public event RotatePlaySpaceDelegate RotatingPlaySpace;

    /// <summary>
    /// the owner of this playspace rotation handler.
    /// </summary>
    private IPlayer _player;

    private GunController _gunController;


    public GunController GunController {
        set {
            if (_gunController != null)
                _gunController.RotationTriggered -= RotatePlaySpace;
            _gunController = value;
            if (_gunController != null)
                _gunController.RotationTriggered += RotatePlaySpace;
        }
    }

    private void RotatePlaySpace(IPlayer player, RotatePlaySpaceHook target) {
        RotatingPlaySpace?.Invoke(this, target);
    }
}