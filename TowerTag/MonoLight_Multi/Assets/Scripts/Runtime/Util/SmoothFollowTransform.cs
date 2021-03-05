using UnityEngine;

public class SmoothFollowTransform : MonoBehaviour
{
    [SerializeField] private Transform _objectToFollow;
    [SerializeField] private float _lerpSpeed = 5;
    [SerializeField] private bool _applyPosition;
    [SerializeField] private bool _applyRotation;

    private void Awake()
    {
        if (_objectToFollow == null)
        {
            Debug.LogError("No Object to follow found. Deactivating script");
            enabled = false;
        } else if (!_applyPosition && !_applyRotation)
        {
            Debug.LogWarning("Neither rotation nor position is true. Deactivating script");
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(_applyPosition)
            transform.position = Vector3.Lerp(transform.position, _objectToFollow.position, Time.deltaTime * _lerpSpeed);
        if(_applyRotation)
            transform.rotation = Quaternion.Lerp(transform.rotation, _objectToFollow.rotation, Time.deltaTime * _lerpSpeed);
    }
}
