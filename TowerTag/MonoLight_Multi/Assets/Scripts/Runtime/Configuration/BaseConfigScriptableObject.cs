using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseConfigScriptableObject : ScriptableObject
{
    [SerializeField] private Configuration configuration;
    public Configuration Config => configuration;
    public void ApplyConfig (Configuration configuration)
    {
        this.configuration = configuration;
    }
}
