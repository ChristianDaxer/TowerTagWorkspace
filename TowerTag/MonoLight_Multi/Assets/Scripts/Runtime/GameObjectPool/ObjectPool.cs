using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[Serializable]
public sealed class ObjectPool : IDisposable {
    public IPoolableObject[] PooledObjects { get; private set; }

    public int LastElement { get; private set; }
    public int LastActiveElement { get; private set; }

    private readonly bool _reuseActiveObjects;
    private int _reuseIndex;

    private GameObject _prefab;

    private Transform _parentTransform;

    public ObjectPool(int maxSize, GameObject prefab, Transform parent, bool reuseActiveObjects = false) {
        _prefab = prefab;
        if (parent != null) {
            _parentTransform = parent;
        }

        _reuseActiveObjects = reuseActiveObjects;

        SceneManager.sceneUnloaded += OnSceneUnloaded;

        Init(maxSize);
    }

    private void Init(int maxSize) {
        PooledObjects = new IPoolableObject[maxSize];
        LastElement = -1;
        LastActiveElement = -1;
        _reuseIndex = 0;
    }

    public GameObject CreateGameObject(Vector3 position, Quaternion rotation) {
        GameObject gameObject;
        // activate Object
        if (ObjectsToActivateAvailable()) {
            LastActiveElement++;
            gameObject = ActivatePooledObject(LastActiveElement, position, rotation);

            return gameObject;
        }

        // no Object to activate -> create a new one
        if (CanANewObjectGetInstantiated()) {
            LastActiveElement++;
            LastElement++;

            gameObject = InstantiateNewPooledObject(LastActiveElement, position, rotation);

            return gameObject;
        }


        // no Object available (to activate or create)
        if (_reuseActiveObjects) {
            gameObject = ActivatePooledObject(_reuseIndex, position, rotation);
            _reuseIndex = (_reuseIndex + 1) % (LastActiveElement + 1);
            return gameObject;
        }

        return null;
    }

    // to override Behaviour if IPoolableObject-Script is already on GameObject
    private IPoolableObject GetPoolableScriptFromGameObject(GameObject gameObject) {
        PoolableObject pO = gameObject.GetComponent<PoolableObject>();
        if (pO == null) {
            pO = gameObject.AddComponent<PoolableObject>();
        }

        return pO;
    }

    GameObject InstantiateNewPooledObject(int index, Vector3 position, Quaternion rotation) {
        GameObject go = Object.Instantiate(_prefab, position, rotation);
        go.transform.parent = _parentTransform;
        IPoolableObject po = GetPoolableScriptFromGameObject(go);

        po.Init(index, this);
        po.SetActive(true);
        PooledObjects[index] = po;

        return go;
    }

    private GameObject ActivatePooledObject(int index, Vector3 position, Quaternion rotation) {
        IPoolableObject poolable = PooledObjects[index];

        // in case the object was destroyed (be loadNewScene ...)
        if (!poolable.Equals(null)) {
            GameObject gameObject = poolable.GetGameObject();
            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;

            poolable.SetID(index);
            poolable.SetActive(true);

            return gameObject;
        }

        return InstantiateNewPooledObject(index, position, rotation);
    }

    public void Destroy(int objectID) {
        if (objectID < 0) return;
        if (objectID > LastElement) return;
        if (objectID > LastActiveElement) return;

        // switch current with last Active
        // set new IDs for deactivated und lastActive
        SwitchObjectsInPool(objectID, LastActiveElement);

        // decrement _lastActive counter
        GameObject gameObject = PooledObjects[LastActiveElement].GetGameObject();
        gameObject.transform.SetParent(_parentTransform);
        PooledObjects[LastActiveElement].SetActive(false);
        LastActiveElement--;
    }

    private void ClearPool() {
        if (PooledObjects == null)
            return;

        for (var i = 0; i < PooledObjects.Length; i++) {
            IPoolableObject pooledObject = PooledObjects[i];
            if (!pooledObject.IsNull() && pooledObject.GetGameObject() != null) {
                Object.Destroy(pooledObject.GetGameObject());
                PooledObjects[i] = null;
            }
        }

        int maxSize = PooledObjects.Length;
        PooledObjects = null;

        Init(maxSize);
    }

    private void SwitchObjectsInPool(int a, int b) {
        IPoolableObject tmp = PooledObjects[a];
        PooledObjects[a] = PooledObjects[b];
        PooledObjects[b] = tmp;

        PooledObjects[a].SetID(a);
        PooledObjects[b].SetID(b);
    }

    private bool ObjectsToActivateAvailable() {
        return LastActiveElement < LastElement && LastElement < PooledObjects.Length;
    }

    private bool CanANewObjectGetInstantiated() {
        return LastActiveElement < PooledObjects.Length - 1;
    }

    public void Dispose() {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;

        if (PooledObjects != null) {
            foreach (IPoolableObject pooledObject in PooledObjects) {
                if (!pooledObject.IsNull()) {
                    GameObject gameObject = pooledObject.GetGameObject();
                    if (gameObject != null) {
                        Object.Destroy(gameObject);
                    }
                }
            }
        }

        PooledObjects = null;
        _prefab = null;
        _parentTransform = null;
    }

    // cleanup all created/pooled objects when unloading scene
    private void OnSceneUnloaded(Scene scene) {
        ClearPool();
    }
}