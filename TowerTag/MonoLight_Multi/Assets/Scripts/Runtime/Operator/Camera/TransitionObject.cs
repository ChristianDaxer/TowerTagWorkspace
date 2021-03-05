using System.Collections;
using TowerTag;
using UnityEngine;

/// <summary>
/// The transition object is the object which is focused from the follow cam
/// </summary>
public class TransitionObject : MonoBehaviour {
    [SerializeField] private LookToNearestTarget _lookTo;

    public IPlayer CurrentlyLookingAt { get; set; }

    private Coroutine _rotatingCoru;

    private void Update() {
        GameObject lookingAtObject = CurrentlyLookingAt?.GameObject;
        if (lookingAtObject != null) {
            transform.position = lookingAtObject.transform.position;
        }
        else if (_lookTo.CurrentlyFollowingPlayer?.GameObject != null)
            transform.position = _lookTo.CurrentlyFollowingPlayer.GameObject.transform.forward;
    }

    public void StartRotateToNextTarget(IPlayer newTarget) {
        GameObject newTargetGameObject = newTarget?.GameObject;
        if (newTargetGameObject != null && transform.position != newTargetGameObject.transform.position) {
            if (_rotatingCoru != null)
                StopCoroutine(_rotatingCoru);

            //Lerp or jump to new target
            _rotatingCoru = StartCoroutine(RotateToNextTarget(newTarget));
        }
    }

    /// <summary>
    /// Rotates the camera smooth to the new target
    /// </summary>
    /// <param name="newTarget"></param>
    /// <returns></returns>
    private IEnumerator RotateToNextTarget(IPlayer newTarget) {
        GameObject newTargetGameObject = newTarget.GameObject;
        if (newTargetGameObject == null)
            yield break;
        Vector3 startPos = transform.position;
        Vector3 endPos = newTargetGameObject.transform.position;
        float time = 0;
        //While transitioning this object should not be parent of a TT_Player
        while (time <= 1) {
            //Moves the transition object to the new target and rotates around the current LookAtTarget object of the TTPlayer prefab which is followed by the camera
            time += Time.deltaTime;
            Vector3 position = _lookTo.transform.position;
            transform.position = position + Vector3.Slerp(startPos - position, endPos - position, time);
            yield return null;
        }
        CurrentlyLookingAt = newTarget;
        _rotatingCoru = null;
        yield return null;
    }
}
