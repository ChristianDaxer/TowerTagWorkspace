using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ModelImporterPostprocess : AssetPostprocessor
{
    private static Shader ttStandardShader;
    private const string ttStandardShaderName = "TTStandard";
    public void OnPostprocessModel (GameObject gameObject)
    {
        if (ttStandardShader == null)
        {
            ttStandardShader = Shader.Find("TTStandard");
            if (ttStandardShader == null)
            {
                Debug.LogErrorFormat("Unable to find shader: \"{0}\".", ttStandardShaderName);
                return;
            }
        }

        gameObject.GetComponentsInChildren<Renderer>()
            .ForEach(renderer => renderer.sharedMaterials
                .Where(material => material.shader.name == "Standard")
                    .ForEach(material => material.shader = ttStandardShader));
    }
}
