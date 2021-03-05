using UnityEngine;
using UnityEngine.Serialization;

public class RotateAvatarsBody : MonoBehaviour {
    [FormerlySerializedAs("avatarsNeck")]
    [SerializeField]
    private Transform _avatarsNeck;

    // Update is called once per frame
    private void Update() {
        Transform thisTransform = transform;
        Vector3 localEulerAngles = thisTransform.localEulerAngles;
        Vector3 neckLocalEulerAngles = _avatarsNeck.localEulerAngles;
        float smoothXRotation = Mathf.LerpAngle(localEulerAngles.x, Mathf.DeltaAngle(0, neckLocalEulerAngles.x) * 0.25f, 0.025f);
        float smoothYRotation = Mathf.LerpAngle(localEulerAngles.y, neckLocalEulerAngles.y, 0.025f);
        float smoothZRotation = Mathf.LerpAngle(localEulerAngles.z, Mathf.DeltaAngle(0, neckLocalEulerAngles.z) * 0.75f, 0.025f);

        var newRotation = new Vector3(smoothXRotation, smoothYRotation, smoothZRotation);
        localEulerAngles = newRotation;
        thisTransform.localEulerAngles = localEulerAngles;
        thisTransform.position = _avatarsNeck.position;
    }
}
