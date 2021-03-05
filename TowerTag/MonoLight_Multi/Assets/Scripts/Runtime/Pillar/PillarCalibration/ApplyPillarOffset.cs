using Hologate;
using UnityEngine;

public class ApplyPillarOffset : MonoBehaviour {
    private Quaternion _localStartRotation;

    private void Awake() {
        if (TowerTagSettings.Hologate) {
            HologateRoomSetup hgRoomSetup = gameObject.AddComponent<HologateRoomSetup>();
            hgRoomSetup.PillarOffset = this;
            hgRoomSetup.SetPlaySpaceOffset();
        }
    }

    private void Start() {
        _localStartRotation = transform.localRotation;
        if (!TowerTagSettings.Hologate)
            ApplyOffsetFromConfigurationFile();
    }

    public void ApplyOffsetFromConfigurationFile() {
        // read offsetValues from ConfigFile
        Vector3 positionOffset = ConfigurationManager.Configuration.PillarPositionOffset;
        Quaternion rotationOffset = Quaternion.AngleAxis(-ConfigurationManager.Configuration.PillarRotationOffsetAngle, Vector3.up);

        // apply offsetValues
        ApplyOffset(positionOffset, rotationOffset);
    }

    public void SetOffsetConfigurationValues(Vector3 positionOffset, float rotationOffset)
    {
        ConfigurationManager.Configuration.PillarPositionOffset = positionOffset;
        ConfigurationManager.Configuration.PillarRotationOffsetAngle = rotationOffset;
        ConfigurationManager.Configuration.IngamePillarOffset = true;
        ConfigurationManager.WriteConfigToFile();
    }

    public void ApplyOffset(Vector3 positionOffset, Quaternion rotationOffset) {
        var thisTransform = transform;
        thisTransform.localPosition = rotationOffset * positionOffset;
        thisTransform.localRotation = rotationOffset * _localStartRotation;
    }

    public static Quaternion RotationAngleToRotationOffset(float angle) {
        return Quaternion.AngleAxis(-angle, Vector3.up);
    }
}

