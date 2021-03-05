using UnityEngine;

public class LogoProjectorManager : MonoBehaviour
{
    [SerializeField, Tooltip("The Logo Projectors")]
    private GameObject[] _projectors;

    [SerializeField, Tooltip("Allows customization even if there is no license meta key")]
    private bool _testing;

    private void Awake() {
        if (_testing)
            Customization.Instance.CustomizeLogos(true);
    }

    void Start()
    {
        if (Customization.UseCustomLogos) {
            _projectors.ForEach(projector => projector.SetActive(true));
        }
    }
}
