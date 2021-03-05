using UnityEngine;

/// <summary>
/// Class that will move the object it is attached to according to the defined animation curves in a ping-pong manner.
/// </summary>
public class MovementCurves : MonoBehaviour {
    [SerializeField] private AnimationCurve _x;

    [SerializeField] private AnimationCurve _y;

    [SerializeField] private AnimationCurve _z;

    [SerializeField] private float _speed;

    [SerializeField] private Vector3 _scale = Vector3.one;

    private Vector3 _position;
    private float _t;

    private int _sign = 1;

    private void Start() {
        _position = transform.position;
    }

    private void Update() {
        _t += _sign * _speed * Time.deltaTime;
        _t = Mathf.Clamp01(_t);

        transform.position = _position + new Vector3(
                                 _x.Evaluate(_t) * _scale.x,
                                 _y.Evaluate(_t) * _scale.y,
                                 _z.Evaluate(_t) * _scale.z);

        if (_t >= 1) {
            _sign = -1;
        }
        else if (_t <= 0) {
            _sign = 1;
        }
    }
}