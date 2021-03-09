using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMaterialTiling : MonoBehaviour
{
    private MeshRenderer myRenderer;

    private void Start()
    {
        myRenderer = GetComponent<MeshRenderer>();
    }


    [ContextMenu("SetTileAmount")]
    void SetTileAmount()
    {
        myRenderer.material.SetTextureScale("_MainTex", new Vector2(Mathf.RoundToInt((transform.localScale.x + transform.localScale.y)/2f), Mathf.RoundToInt(transform.localScale.z)));
    }
}
