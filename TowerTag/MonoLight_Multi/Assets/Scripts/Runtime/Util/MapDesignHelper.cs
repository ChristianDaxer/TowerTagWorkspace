using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Simple Script that helps to position the pillars exactly in respect to the teleport distance
/// </summary>
public class MapDesignHelper : MonoBehaviour {

    [FormerlySerializedAs("drawOnlyIfSelected"), SerializeField]
    private bool _drawOnlyIfSelected;
    [FormerlySerializedAs("fillArea"), SerializeField]
    private bool _fillArea = true;

    [FormerlySerializedAs("color"), SerializeField]
    private Color _color = Color.green;
    [FormerlySerializedAs("radius"), SerializeField]
    private float _radius = 14.0f;

    private void OnDrawGizmos() {
        if( !_drawOnlyIfSelected ) {
            DrawSphere();
        }
    }

    private void OnDrawGizmosSelected() {
        if( _drawOnlyIfSelected ) {
            DrawSphere();
        }
    }

    private void DrawSphere() {
        //color.a = alpha;
        Gizmos.color = _color;
        if( _fillArea ) {
            Gizmos.DrawSphere(transform.position, _radius);
        } else {
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }

}
