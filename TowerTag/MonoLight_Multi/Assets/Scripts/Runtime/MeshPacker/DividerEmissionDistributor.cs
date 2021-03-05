using UnityEngine;

public class DividerEmissionDistributor : MaterialDataDistributor
{
    [HideInInspector] [SerializeField] protected Color[] _emissionColors;
    [SerializeField] private string emissionColorsPropertyName = "_IndexedEmissionColors";

    public void SetEmissionColor(int index, Color color)
    {
        if (index > _emissionColors.Length - 1)
            return;
        _emissionColors[index] = color;
    }

    protected override void OnLateUpdate(Material material)
    {
        material.SetColorArray(emissionColorsPropertyName, _emissionColors);
    }

    protected override void OnAwake(int intanceCount)
    {
        _emissionColors = new Color[intanceCount];
    }
}
