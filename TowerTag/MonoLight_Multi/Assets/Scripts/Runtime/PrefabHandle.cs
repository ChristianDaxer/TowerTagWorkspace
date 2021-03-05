using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPrefabHandle", menuName = "ScriptableObjects/New Prefab Handle", order = 1)]
public class PrefabHandle : ScriptableObject
{
    [SerializeField]
    public GameObject steamvrPrefab;

    [SerializeField]
    public GameObject oculusPrefab;

    public GameObject GetPlatfromPrefab ()
    {
#if !UNITY_ANDROID
        return steamvrPrefab;
#else
        return oculusPrefab;
#endif
    }
}
