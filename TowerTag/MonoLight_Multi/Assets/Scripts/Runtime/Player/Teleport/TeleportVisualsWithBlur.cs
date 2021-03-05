using OwnUnityStandardAssets.ImageEffects;
using UnityEngine;

public class TeleportVisualsWithBlur : TeleportVisuals
{
    [SerializeField] private CameraMotionBlur _blur;

    protected override void Awake() {
        base.Awake();

        if (PlayerRigBase.GetInstance(out var playerRig) && playerRig.TryGetPlayerRigTransform(PlayerRigTransformOptions.Head, out var head)) {
            _blur = head.GetComponent<CameraMotionBlur>();
        }
    }

    protected override void ActivateEffects(bool setActive)
    {
        base.ActivateEffects(setActive);

        if (_blur != null)
        {
            _blur.enabled = setActive;
        }
    }
}
