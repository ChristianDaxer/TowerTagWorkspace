using UnityEngine;

public class FloatVisuals : MonoBehaviour
{
    public virtual void SetValue(float newValue)
    {
        Debug.Log("FloatVisuals:FloatValueChanged: " + newValue);
    }
}
