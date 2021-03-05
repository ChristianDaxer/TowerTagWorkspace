using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ChargeableCollider))]
public class PillarClaimColliderSphere : Editor {

    public float distance = 0.75f;
    public Color color = Color.blue;

    void OnSceneGUI() {
        Handles.color = color;
        ChargeableCollider pillar = (ChargeableCollider) target;
        Handles.DrawWireArc(pillar.transform.position, pillar.transform.up, pillar.transform.right, 360, distance);
    }

}
