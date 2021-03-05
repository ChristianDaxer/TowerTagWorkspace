using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerHeadBase : TTSingleton<PlayerHeadBase>
{
    private Camera headCamera;
    public Camera HeadCamera
    {
        get
        {
            if (headCamera == null)
            {
                headCamera = GetComponent<Camera>();
                if (headCamera == null)
                {
                    Debug.LogErrorFormat("{0} attached to: \"{1}\" should be attached to a GameObject in the player rig with a camera component.", typeof(PlayerHeadBase).Name, gameObject.name);
                    return null;
                }
            }

            return headCamera;
        }
    }
    public abstract Vector3 Position { get; }
    public abstract Quaternion Rotation { get; }
}
