using UnityEngine;

public class ScaleSphereWithRadius : MonoBehaviour
{
    [SerializeField] private SphereCollider _sphereCollider;

    private void Update() {
        float radius = _sphereCollider.radius;
        Vector3 scale = Vector3.one * (radius * 2);
        transform.localScale = scale;
    }
}
