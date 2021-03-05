public class SyncTransformsSend : AbstractTransformSync {
    private void Update() {
        if (_sync != null)
            _sync.ReadDataFromTransforms(TransformsToSync);
    }
}