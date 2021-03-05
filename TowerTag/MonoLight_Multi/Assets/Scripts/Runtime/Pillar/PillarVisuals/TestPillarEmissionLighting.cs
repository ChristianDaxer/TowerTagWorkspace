using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TestPillarEmissionLighting : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private string shaderVariableName = "_ClaimLocalToWorld";
#if UNITY_ANDROID
    private void Update()
    {
        ColorChanger.SetCustomMatrixPropertyToRendererLocalToWorld(renderers, shaderVariableName);
    }
#endif
}
