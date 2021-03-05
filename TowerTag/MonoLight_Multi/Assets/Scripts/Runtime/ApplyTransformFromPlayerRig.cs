using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerRigTransformOptions
{
    Head,
    RightHand,
    LeftHand,
    Root
}

public class ApplyTransformFromPlayerRig : MonoBehaviour
{
    public PlayerRigTransformOptions _option;
    [SerializeField] private bool _applyPosition;
    [SerializeField] private bool _applyRotation;

    private PlayerRigBase playerRigBase;

    private void Start() {
        ApplyValues();
    }

    private void LateUpdate() {
        ApplyValues();
    }

    private bool TryGetPlayerRigTransform(out Transform transform)
    {
        transform = null;
        if (playerRigBase == null)
            if (!PlayerRigBase.GetInstance(out playerRigBase))
                return false;

        if (_option == PlayerRigTransformOptions.LeftHand || _option == PlayerRigTransformOptions.RightHand) {
            if (InputControllerVR.Instance == null)
                return false;

            var activeController = InputControllerVR.Instance.ActiveController;
            if (activeController == null)
                return false;

            transform = activeController.transform;
            return true;
        }

        return playerRigBase.TryGetPlayerRigTransform(_option, out transform);
    }

    private void ApplyValues() {
        if (!TryGetPlayerRigTransform(out var rigTransform))
            return;

        if (_applyPosition)
            transform.position = rigTransform.position;

        if (_applyRotation)
            transform.rotation = rigTransform.rotation;
    }
}
