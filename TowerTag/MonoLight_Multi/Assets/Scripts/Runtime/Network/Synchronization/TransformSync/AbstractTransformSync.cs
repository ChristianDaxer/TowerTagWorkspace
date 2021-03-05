using UnityEngine;

public class AbstractTransformSync : MonoBehaviour {
    [SerializeField] protected SyncTransforms _sync;

    public SyncTransforms TransformSync {
        set { _sync = value; }
    }

    [SerializeField] protected Transform[] _transformsToSync;
    protected Transform[] TransformsToSync => _transformsToSync;

    private void OnDestroy() {
        _sync = null;
        _transformsToSync = null;
    }
}