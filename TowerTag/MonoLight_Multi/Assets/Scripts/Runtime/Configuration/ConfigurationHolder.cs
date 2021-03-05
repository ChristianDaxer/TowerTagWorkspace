using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurationHolder : TTSingleton<ConfigurationHolder>
{
    public BaseConfigScriptableObject configScriptableObject;

    protected override void Init() {}
}
