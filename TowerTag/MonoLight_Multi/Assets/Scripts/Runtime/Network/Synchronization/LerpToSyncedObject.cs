using UnityEngine;

public class LerpToSyncedObject : MonoBehaviour {
    [SerializeField] private GameObject _syncedObject;
    [SerializeField] private int _positionLerpSpeed = 15;
    [SerializeField] private int _rotationLerpSpeed = 30;

    // Update is called once per frame
    private void Update()
    {
        Transform t = transform;
        t.position = Vector3.Lerp(t.position, _syncedObject.transform.position, Time.deltaTime * _positionLerpSpeed);
        t.rotation = Quaternion.Lerp(t.rotation, _syncedObject.transform.rotation, Time.deltaTime * _rotationLerpSpeed);
    }
}