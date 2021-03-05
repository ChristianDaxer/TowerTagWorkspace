using UnityEngine;

[DefaultExecutionOrder(-1)]
public abstract class TTSingleton<T> : MonoBehaviour where T : TTSingleton<T> {
    // should this script be persistent between scenes?
    [SerializeField] private bool _isPersistent = true;

    private static T instance;
    public static T Instance { get {
        GetInstance(out instance);
        return instance;
    } }

    public static bool GetInstance (out T inst) {
        if (instance == null) {
            if (SingletonInstanceRegistory.SearchForInstanceInRegistory<T>(out instance)) {
                inst = instance;
                return inst != null;
            }

            inst = null;
            return false;
        }

        inst = instance;
        return inst != null;
    }

    protected void Awake() {
        if (instance == null) {

            SingletonInstanceRegistory.AddToCache(typeof(T), this);
            instance = (T)this;

            Init();

            if (_isPersistent && transform.parent == null)
                DontDestroyOnLoad(gameObject);
            return;
        }

        else Debug.LogErrorFormat("There are multiple instances of: {0} loaded in the scene.", typeof(T).FullName);
    }

    protected abstract void Init();

    private void OnDestroy() => SingletonInstanceRegistory.RemoveFromCache(typeof(T)); 
}