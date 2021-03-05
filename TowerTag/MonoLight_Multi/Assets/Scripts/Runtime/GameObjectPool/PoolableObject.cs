using UnityEngine;

public sealed class PoolableObject : MonoBehaviour, IPoolableObject {
    private int _id;
    private ObjectPool _pool;

    public void Init(int id, ObjectPool pool) {
        _pool = pool;
        _id = id;
    }

    public void SetActive(bool active) {
        if (!this.IsNull())
            gameObject.SetActive(active);
    }

    public void DestroyObject() {
        _pool.Destroy(_id);
    }

    public void SetID(int newID) {
        _id = newID;
    }

    public int GetID() {
        return _id;
    }

    public GameObject GetGameObject() {
        if (this.IsNull())
            return null;

        return gameObject;
    }

    public ObjectPool GetPool() {
        return _pool;
    }

    private void OnDestroy() {
        _pool = null;
    }
}