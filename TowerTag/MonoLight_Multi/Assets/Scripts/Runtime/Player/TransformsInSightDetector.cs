using System.Collections;
using TowerTagSOES;
using Unity.Collections;
using UnityEngine;

public class TransformsInSightDetector : MonoBehaviour {
    [SerializeField, Range(0, 360)]
    [Tooltip("The view angle of the local player, visibility just gets evaluated when the player is in the view angle of the local player")]
    private float _viewAngle;
    [SerializeField, Tooltip("LayerMask for possible obstacles that can hide a player")]
    private LayerMask _obstacleMask;
    [SerializeField, Tooltip("Anchor points which check if the player can see them")]
    private Transform[] _headVisibilityAnchors;
    [SerializeField]
    private NameBadge _nameBadge;
    [SerializeField, Tooltip("Toggle ray casts in scene view")]
    private bool _debug;

    private bool _isVisible;
    private Transform _ownPlayerHead;

    private void OnEnable() {
        if (SharedControllerType.IsAdmin || SharedControllerType.Spectator) {
            enabled = false;
            return;
        }

        _ownPlayerHead = PlayerManager.Instance.GetOwnPlayer()?.PlayerAvatar.AvatarMovement.HeadSourceTransform;
        if (_ownPlayerHead == null && !SharedControllerType.IsAdmin) {
            Debug.LogWarning("No local player found -> Player name hiding disabled!");
            enabled = false;
            return;
        }

        StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    private IEnumerator FindTargetsWithDelay(float delay) {
        WaitUntil waitUntil = new WaitUntil(() => { return checkedAllVisibilities; });
        while (enabled) {
            yield return waitUntil;
            //In Commendation scene the name badges are disabled! So i don't check visibility when the name badge is disabled
            if (_nameBadge.gameObject.activeSelf)
                StartCoroutine(CheckVisibilityOfTransforms());
        }
    }

    private Ray[] rays = new Ray[8];
    private float[] distances = new float[8];
    private bool checkedAllVisibilities = false;

    /// <summary>
    /// Checks if the transforms are hidden from the local player by objects or not and toggle the visibility of the name badge
    /// If at least one transform is visible then the name badge is visible.
    /// </summary>
    private IEnumerator CheckVisibilityOfTransforms() {
        checkedAllVisibilities = false;
        //if the game object is not in field of view of the local player there is no reason to evaluate if the player is hidden
        if (Vector3.Angle(_ownPlayerHead.forward, transform.position - _ownPlayerHead.position) >= _viewAngle / 2)
            yield break;

        NativeArray<RaycastHit> hits = new NativeArray<RaycastHit>(_headVisibilityAnchors.Length, Allocator.TempJob);
        if (rays.Length < _headVisibilityAnchors.Length)
        {
            rays = new Ray[_headVisibilityAnchors.Length];
            distances = new float[_headVisibilityAnchors.Length];
        }

        bool currentlyVisible = false;
        // Distribute raycasts across multiple frames.
        if (!RaycastScheduler.GetInstance(out var instance))
            yield break;

        for (int i = 0; i < _headVisibilityAnchors.Length; i++)
        {
            rays[i].origin = _headVisibilityAnchors[i].position;
            rays[i].direction = (_ownPlayerHead.position - _headVisibilityAnchors[i].position);
            distances[i] = rays[i].direction.magnitude;
            yield return null;
        }

        instance.Schedule(rays, _headVisibilityAnchors.Length, hits, distances, _obstacleMask, () =>
        {
            for (int i = 0; i < _headVisibilityAnchors.Length; i++)
            {
                if (hits[i].transform == null)
                {
                    currentlyVisible |= false;
                    Debug.LogFormat("Ray: {0} did not hit anything.", i);
                    continue;
                }

                Debug.LogFormat("Ray: {0} collided with: .", hits[i].transform.name);
                currentlyVisible |= true;

                if (_debug)
                    DrawDebugLine(_headVisibilityAnchors[i], false);
            }

            if (_isVisible != currentlyVisible) {
                _isVisible = currentlyVisible;
                _nameBadge.ToggleVisibility(_isVisible);
            }
            checkedAllVisibilities = true;
        });
    }


    private void DrawDebugLine(Transform starTransform, bool visible) {
        Color rayColor = visible ? Color.green : Color.red;

        var position = _ownPlayerHead.position;
        Debug.DrawLine(starTransform.position, position, rayColor);
    }
}

