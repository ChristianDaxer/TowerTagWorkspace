using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public sealed class AfterEffect : MonoBehaviour {
    [SerializeField] private Material _material;

    public Material Material {
        get => _material;
        set => _material = value;
    }

    private void Start() {
        // Disable the image effect if the shader can't
        // run on the users graphics card
        if (Material == null || !Material.shader || !Material.shader.isSupported)
            enabled = false;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (Material == null) {
            Graphics.Blit(source, destination);
            return;
        }

        Graphics.Blit(source, destination, Material, -1);
    }
}