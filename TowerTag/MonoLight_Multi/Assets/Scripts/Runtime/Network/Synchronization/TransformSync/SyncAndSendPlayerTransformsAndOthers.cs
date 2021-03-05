using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncAndSendPlayerTransformsAndOthers : AbstractTransformSync
{
    /*public PlayerRigTransformOptions options;
    private PlayerRigBase playerRigBase;*/

    private void Awake()
    {
        /*if (playerRigBase == null)
        {
            if (!PlayerRigBase.GetInstance(out playerRigBase))
                return;
        }

        if (!playerRigBase.TryGetPlayerRigTransform(options, out var rigTransform))
            return;

        if (rigTransform == null)
            return;

        List<Transform> transforms = new List<Transform>();
        transforms.AddRange(TransformsToSync);
        transforms.Add(rigTransform);
        _transformsToSync = transforms.ToArray();*/
    }

    private void Update() {
        if (_sync != null)
            _sync.ReadDataFromTransforms(TransformsToSync);
    }
}

