using TowerTag;
using static OperatorCamera.CameraManager;

namespace OperatorCamera {
    public delegate void CameraChangeEventHandler(CameraManager sender, CameraMode oldMode, CameraMode newMode);
    public delegate void BoolChangedEventHandler(CameraManager sender, bool value);
    public delegate void PlayerFocusEventHandler(CameraManager sender, IPlayer player);

    public interface ICameraManager{
        TargetGroupManager TargetGroupManager { get; }
        CameraMode CurrentCameraMode { get; set; }

        bool HardFocusOnPlayer { get; }
        IPlayer PlayerToFocus { get; }

        event CameraChangeEventHandler CameraModeChanged;
        event BoolChangedEventHandler HardFocusOnPlayerChanged;
        event PlayerFocusEventHandler PlayerToFocusChanged;

        void SwitchCameraMode(int newModeInt);
        void SwitchCameraMode(int newModeInt, IPlayer playerToFocus, bool hardFocus = false);
        void SetHardFocusOnPlayer(IPlayer player, bool active);
    }
}
