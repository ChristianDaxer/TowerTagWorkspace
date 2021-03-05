using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeadOculus : PlayerHeadBase
{
    public override Vector3 Position { get { return transform.position; } }

    public override Quaternion Rotation { get { return transform.rotation; } }

    protected override void Init()
    {
    }
}
