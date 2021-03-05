using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class PlatformReconfigureTaskScriptableObject : ScriptableObject
{
    [SerializeField] protected bool skip = false;
}
