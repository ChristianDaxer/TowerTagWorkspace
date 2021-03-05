using UnityEngine;
using System.Linq;

public static class
    ColorChanger {
    private static readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
    private static bool _stdPropertyIDSet;
    private static int _stdPropertyID = -1;

    // Renderer
    public static void ChangeColorInChildRendererComponents(GameObject rendererParent, Color color,
        bool keepTransparency = false) {
        if (rendererParent == null)
            return;

        // Renderer
        Renderer[] renderer = rendererParent.GetComponentsInChildren<Renderer>();
        ChangeColorInRendererComponents(renderer, color, GetStdPropertyID(), keepTransparency);
    }

    public static void
        ChangeColorInRendererComponents(Renderer[] renderer, Color color, bool keepTransparency = false) {
        ChangeColorInRendererComponents(renderer, color, GetStdPropertyID(), keepTransparency);
    }

    public static void ChangeColorInChildRendererComponents(GameObject rendererParent, Color color, int propertyID,
        bool keepTransparency = false) {
        if (rendererParent == null)
            return;

        // Renderer
        Renderer[] renderer = rendererParent.GetComponentsInChildren<Renderer>();
        ChangeColorInRendererComponents(renderer, color, propertyID, keepTransparency);
    }

    public static void ChangeColorInRendererComponents(Renderer[] renderer, Color color, int propertyID,
        bool keepTransparency = false) {
        if (renderer == null)
            return;

        foreach (Renderer rend in renderer) {
            if (rend == null)
                continue;

            Material tmpMat = !Application.isPlaying ? rend.sharedMaterial : rend.material;
            if (tmpMat == null || !tmpMat.HasProperty(propertyID)) {
                continue;
            }

            rend.GetPropertyBlock(_block);

            if (keepTransparency) {
                color.a = tmpMat.GetColor(propertyID).a;
            }

            _block.SetColor(propertyID, color);
            rend.SetPropertyBlock(_block);
        }
    }

    public static void ChangeColorInRendererComponentsWithMultipleMaterials(
        Renderer[] renderers, Material materialToAccess, Color color, int propertyID, bool keepTransparency = false) {
        if (renderers == null)
            return;

        foreach (Renderer renderer in renderers) {
            if (renderer == null)
                continue;

            Material tmpMat = !Application.isPlaying ? renderer.sharedMaterials.FirstOrDefault(mat => mat == materialToAccess) : renderer.materials.FirstOrDefault(mat => mat == materialToAccess);
            if (tmpMat == null || !tmpMat.HasProperty(propertyID)) {
                continue;
            }

            renderer.GetPropertyBlock(_block);

            if (keepTransparency) {
                color.a = tmpMat.GetColor(propertyID).a;
            }

            _block.SetColor(propertyID, color);
            renderer.SetPropertyBlock(_block);
        }
    }

    public static void ChangeTransparencyInRendererComponents(Renderer[] renderers, float transparency) {
        ChangeTransparencyInRendererComponents(renderers, GetStdPropertyID(), transparency);
    }

    private static void
        ChangeTransparencyInRendererComponents(Renderer[] renderers, int propertyID, float transparency) {
        if (renderers == null)
            return;

        foreach (Renderer renderer in renderers) {
            if (renderer == null)
                continue;

            Material tmpMat = !Application.isPlaying ? renderer.sharedMaterial : renderer.material;
            if (tmpMat == null || !tmpMat.HasProperty(propertyID)) {
                continue;
            }

            renderer.GetPropertyBlock(_block);
            Color color;
            if (_block.isEmpty) {
                color = tmpMat.GetColor(propertyID);
            }
            else {
                color = _block.GetVector(propertyID);
                color = color.gamma;
            }

            color.a = transparency;
            _block.SetColor(propertyID, color);
            renderer.SetPropertyBlock(_block);
        }
    }

    public static void SetCustomMatrixPropertyToRendererLocalToWorld(Renderer[] renderer, string shaderVariableName) {
        if (renderer == null)
            return;

        foreach (Renderer rend in renderer) {
            if (rend == null)
                continue;

            Material tmpMat = !Application.isPlaying ? rend.sharedMaterial : rend.material;
            if (tmpMat == null/* || !tmpMat.HasProperty(propertyID)*/) {
                continue;
            }

            // tmpMat.SetMatrix(shaderVariableName, rend.transform.localToWorldMatrix);
            rend.GetPropertyBlock(_block);
            _block.SetMatrix(shaderVariableName, rend.transform.localToWorldMatrix);
            rend.SetPropertyBlock(_block);
        }
    }

    public static void SetCustomFloatPropertyInRenderers(Renderer[] renderer, int propertyID, float value) {
        if (renderer == null)
            return;

        foreach (Renderer rend in renderer) {
            if (rend == null)
                continue;

            Material tmpMat = !Application.isPlaying ? rend.sharedMaterial : rend.material;
            if (tmpMat == null || !tmpMat.HasProperty(propertyID)) {
                continue;
            }

            // tmpMat.SetFloat(propertyID, value);
            rend.GetPropertyBlock(_block);
            _block.SetFloat(propertyID, value);
            rend.SetPropertyBlock(_block);
        }
    }

    // Lights
    public static void ChangeColorInChildLightComponents(GameObject rendererParent, Color color) {
        if (rendererParent == null)
            return;

        // Lights
        Light[] lights = rendererParent.GetComponentsInChildren<Light>();
        ChangeColorInLightComponents(lights, color);
    }

    public static void ChangeColorInLightComponents(Light[] lights, Color color) {
        if (lights == null)
            return;

        foreach (Light light in lights) {
            if (light != null) {
                light.color = color;
            }
        }
    }

    private static int GetStdPropertyID() {
        if (!_stdPropertyIDSet) {
            _stdPropertyID = Shader.PropertyToID("_TintColor");
            _stdPropertyIDSet = true;
        }

        return _stdPropertyID;
    }
}