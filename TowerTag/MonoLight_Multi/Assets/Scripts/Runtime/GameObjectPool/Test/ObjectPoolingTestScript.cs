using UnityEngine;

public class ObjectPoolingTestScript : MonoBehaviour {
    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _poolSize = 10;
    [SerializeField] private bool _reuseActive;

    private ObjectPool _pool;

    // Use this for initialization
    private void Start() {
        Init();
    }

    // Update is called once per frame
    private void OnGUI() {
        string guiString = "LastElement: " + _pool.LastElement + " LastActive: " + _pool.LastActiveElement;
        GUILayout.Label(guiString);

        GUILayout.Space(10);

        if (GUILayout.Button("Init")) {
            Init();
        }

        if (GUILayout.Button("Add")) {
            _pool.CreateGameObject(Random.onUnitSphere * 10f, Quaternion.identity);
            PrintArray();
        }

        if (GUILayout.Button("Remove")) {
            int randomIndex = Random.Range(0, _pool.LastActiveElement);
            //int randomIndex = Random.Range(-1, poolSize + 2);

            Debug.Log("Destroy at " + randomIndex);

            _pool.Destroy(randomIndex);
            PrintArray();
        }
    }

    private void PrintArray() {
        for (var i = 0; i < _pool.LastActiveElement + 1; i++) {
            Debug.Log(i + ": objID -> " + _pool.PooledObjects[i].GetID());
        }
    }

    private void Init() {
        _pool = new ObjectPool(_poolSize, _prefab, transform, _reuseActive);
    }
}