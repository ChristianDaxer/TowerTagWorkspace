using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshPackerAsset))]
public class MeshPackerAssetEditor : Editor
{
    [MenuItem("GameObject/Pack Object", priority = 48)]
    public static void Pack ()
    {
        MeshPackerAsset.Pack();
    }

    [MenuItem("GameObject/New Pack Object", priority = 48)]
    public static void NewPack ()
    {
        MeshPackerAsset.Pack(true);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
