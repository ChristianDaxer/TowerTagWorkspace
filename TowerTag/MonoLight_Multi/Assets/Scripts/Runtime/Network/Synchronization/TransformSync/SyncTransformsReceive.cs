public class SyncTransformsReceive : AbstractTransformSync {
    private void Update() {
        if(_sync != null)
            _sync.LerpRemoteTransforms(TransformsToSync);
        else
            Debug.LogError("Transform to sync is null");
    }
}