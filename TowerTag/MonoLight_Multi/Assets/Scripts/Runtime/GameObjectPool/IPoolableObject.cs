using UnityEngine;

public interface IPoolableObject
{
    void Init(int id, ObjectPool pool);
    void SetActive(bool active);
    void DestroyObject();
    int GetID();
    void SetID(int newID);
    GameObject GetGameObject();
    ObjectPool GetPool();
}
