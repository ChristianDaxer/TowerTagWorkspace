using UnityEngine;

public class FloatingTextMesh : MonoBehaviour {
    [Header("Text Properties")] //
    [SerializeField, Tooltip("TextMesh component to render the text.")]
    private TextMesh _textField;

    [SerializeField, Tooltip("Transform that should be moved when transformToShowTextTo moves. " +
                             "This could be the TextMesh component or one of its parents.")]
    private Transform _transformToMoveText;

    [SerializeField, Tooltip("Text that should be rendered by Text mesh component.")]
    private string _textToShow = "Text..";

    [Header("Positioning")] //
    [SerializeField, Tooltip("Transform of object you want to show Text in front of " +
                             "(typically your camera or VR-HMD).")]
    private Transform _transformToShowTextTo;

    [SerializeField, Tooltip("Distance from transformToShowTextTo Transform along it's z-axis " +
                             "(typically distance from the camera).")]
    private float _distanceFromObject = 4;

    [SerializeField, Tooltip("Distance from transformToShowTextTo Transform along it's x/y-axis " +
                             "(typically distance from the viewpoint of the camera).")]
    private Vector2 _positionOffset;

    [SerializeField, Range(0, 1), Tooltip("Influences how fast the Text reaches it's calculated target position. " +
                                          "A value of zero means never, 1 means it's always on the calculated position " +
                                          "(like a sticker in the middle of the camera view. " +
                                          "A value between zero and on influences how 'flow' the animation feels.")]
    private float _lerpFactor = 0.05f;

    private void Start() {
        if (_transformToShowTextTo == null || _textField == null || _transformToMoveText == null) {
            Debug.LogWarning($"Disabling {name}, because of missing references");
            enabled = false;
        }

        _textField.text = _textToShow;
    }

    private void Update() {
        UpdateTextPosition();
    }

    private void UpdateTextPosition() {
        _transformToMoveText.position =
            Vector3.Lerp(_transformToMoveText.position, CalculateTargetPosition(), _lerpFactor);

        _transformToMoveText.LookAt(_transformToShowTextTo, Vector3.up);
    }

    private Vector3 CalculateTargetPosition() {
        return _transformToShowTextTo.position + _transformToShowTextTo.forward * _distanceFromObject +
               _transformToShowTextTo.right * _positionOffset.x + _transformToShowTextTo.up * _positionOffset.y;
    }
}