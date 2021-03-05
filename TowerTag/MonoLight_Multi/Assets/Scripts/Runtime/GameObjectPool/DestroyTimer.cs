using UnityEngine;

public class DestroyTimer : MonoBehaviour {
    private enum Mode {
        Destroy,
        Disable
    }

    [SerializeField] private Mode _mode = Mode.Destroy;
    [SerializeField] private float _lifeTime = 2;

    private IPoolableObject _poolableObject;
    private float _lifeTimeLeft;

    private void OnEnable() {
        _lifeTimeLeft = _lifeTime;
    }

    private void Update() {
        if (_lifeTimeLeft > Time.deltaTime) {
            _lifeTimeLeft -= Time.deltaTime;
            return;
        }

        switch (_mode) {
            case Mode.Destroy:
                DestroyThisObject();
                break;
            case Mode.Disable:
                gameObject.SetActive(false);
                break;
            default:
                throw new UnityException($"Unknown mode {_mode}");
        }
    }

    private void DestroyThisObject() {
        if (_poolableObject == null) {
            _poolableObject = GetComponent<IPoolableObject>();
        }

        if (_poolableObject != null) {
            _poolableObject.DestroyObject();
        }
        else {
            Destroy(gameObject);
        }
    }
}