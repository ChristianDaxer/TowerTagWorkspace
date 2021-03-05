using System;
using UnityEngine;

public abstract class TeleportAlgorithm : IDisposable
{
    public abstract void Init(Vector3 startPosition, Pillar target, float minDistanceToTarget, Transform gunTransform);
    public abstract Vector3 GetPositionAt(float delta);

    public virtual void Dispose() { }
}
