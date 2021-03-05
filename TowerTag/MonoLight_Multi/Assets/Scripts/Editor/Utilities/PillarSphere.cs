using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CanEditMultipleObjects]
[CustomEditor(typeof(Pillar))]
public class PillarSphere : Editor {

    public float distance = 15.66f;
    public Color color = Color.magenta;

    void OnSceneGUI() {
        Handles.color = color;
        Pillar pillar = (Pillar) target;
        Handles.DrawWireArc(pillar.transform.position, pillar.transform.up, pillar.transform.right, 360, distance);
        Handles.DrawWireArc(pillar.transform.position, -pillar.transform.right, pillar.transform.forward, 360, distance);
        Handles.DrawWireArc(pillar.transform.position, pillar.transform.forward, pillar.transform.up, 360, distance);
    }
}
