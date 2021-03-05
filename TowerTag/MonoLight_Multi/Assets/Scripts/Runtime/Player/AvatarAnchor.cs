using UnityEngine;

public class AvatarAnchor : MonoBehaviour {
    [SerializeField] private Transform _headTransform;

    [SerializeField] private Transform _bodyTransform;

    public Transform HeadTransform => _headTransform;

    public Transform BodyTransform => _bodyTransform;
}