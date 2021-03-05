using UnityEngine;

/// <summary>
/// The EgoCamera will always look at this object
/// </summary>
public class LookAtTargetEgoView : MonoBehaviour {
    public Transform PlayerHead {
        private get { return _playerHead; }
        set {
            _playerHead = value;
            _isActive = _playerHead != null;
        }
    }

    //distance to the head of the RemoteClient we are spectating
    private const float Distance = 3;

    private bool _isActive;
    private Transform _playerHead;

    private void Update()
    {
        if (!_isActive || PlayerHead == null) return;

        //Keep object in front og the head
        Vector3 newPosition = PlayerHead.position + PlayerHead.forward * Distance;
        gameObject.transform.position = newPosition;
    }
}
