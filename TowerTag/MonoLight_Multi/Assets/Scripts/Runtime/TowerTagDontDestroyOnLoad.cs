using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerTagDontDestroyOnLoad : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
