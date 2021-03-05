using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMultiIndexedMesh : ICombinedMesh
{
    int[] MeshIndices { get; }
    int GameObjectInstanceID { get; }
    void AppendMeshIndex(int meshIndex);
    void ResetMeshIndices();
}
