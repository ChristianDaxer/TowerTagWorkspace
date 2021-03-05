using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Any MonoBehaviours that are attached to a GameObject with 
// a MeshFilter & MeshRenderer components will receive the
// index of the mesh within the combined mesh.
public interface IIndexedMesh : ICombinedMesh 
{
    int MeshIndex { get; set; }
    int GameObjectInstanceID { get; }
}
