using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRigBase : TTSingleton<PlayerRigBase>
{
    private PlayerHeadBase playerHeadBase;
    protected override void Init()
    {
    }

    public bool TryGetPlayerRigTransform (PlayerRigTransformOptions option, out Transform transform)
    {
        transform = null;
        if (playerHeadBase == null)
        {
            if (!PlayerHeadBase.GetInstance(out playerHeadBase))
                return false;
        }

        switch (option)
        {
            case PlayerRigTransformOptions.Head:
                {
                    transform = playerHeadBase.transform;
                    return true;
                }
            case PlayerRigTransformOptions.RightHand:
            case PlayerRigTransformOptions.LeftHand:
                {
                    if (PlayerInputBase.GetInstance(option == PlayerRigTransformOptions.RightHand ? PlayerHand.Right : PlayerHand.Left, out var playerInputBase)) {
                        transform = playerInputBase.transform;
                        return true;
                    }
                    return false;
                };
            case PlayerRigTransformOptions.Root:
                {
                    transform = playerHeadBase.transform;
                    return true;
                };
            default:
                Debug.LogErrorFormat("Unknown transform option: {0}", option);
                break;
        }

        Debug.LogErrorFormat("No instance of: {0} in the scene.", option);
        return false;
    }
}
