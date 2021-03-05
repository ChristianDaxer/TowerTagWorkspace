using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallLocalPositionDistributor : MaterialDataDistributor
{
    [HideInInspector] [SerializeField] protected Vector4[] _wallLocalPosition;
    [SerializeField] private string localPositionPropertyName = "_IndexedLocalPositions";

    public void SetLocalPosition (int index, Vector4 transform)
    {
        if (index > _wallLocalPosition.Length - 1)
            return;
        _wallLocalPosition[index] = transform;
    }

    protected override void OnLateUpdate(Material material)
    {
        material.SetVectorArray(localPositionPropertyName, _wallLocalPosition);
    }

    protected override void OnAwake(int intanceCount)
    {
        _wallLocalPosition = new Vector4[intanceCount];
    }
}
