using UnityEngine;

public class EnumFlagAttribute : PropertyAttribute
{
    public string Name;

    public EnumFlagAttribute() { }

    public EnumFlagAttribute(string name)
    {
        this.Name = name;
    }
}