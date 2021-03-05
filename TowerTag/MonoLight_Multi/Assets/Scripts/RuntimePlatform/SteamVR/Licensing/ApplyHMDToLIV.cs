using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LIV.SDK.Unity.LIV))]
public class ApplyHMDToLIV : MonoBehaviour
{
    private LIV.SDK.Unity.LIV liv;
    private void Awake()
    {
        liv = GetComponent<LIV.SDK.Unity.LIV>();
    }
}
