using UnityEngine;

public class TestProjectileSpawner : MonoBehaviour {
    [SerializeField] private Shot _shotPrefab;
    [SerializeField] private float _speed;
    [SerializeField] private float _age;
    [SerializeField] private float _projectileLifetime;

    [ContextMenu("Fire")]
    public void Fire() {
        Shot shot = InstantiateWrapper.InstantiateWithMessage(_shotPrefab);
        Transform thisTransform = transform;
        var shotData = new ShotData("id", thisTransform.position, null, _speed * thisTransform.forward, false, shot.TactSender);
        shot.Fire(shotData, _age);
        Destroy(shot.gameObject, _projectileLifetime);
    }

    private void OnGUI() {
        if(GUILayout.Button("Fire")) Fire();
    }
}
