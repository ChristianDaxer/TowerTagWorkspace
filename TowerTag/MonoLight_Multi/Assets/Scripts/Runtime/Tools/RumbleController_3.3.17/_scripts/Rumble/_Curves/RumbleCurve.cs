using UnityEngine;

[System.Serializable]
public abstract class RumbleCurve : MonoBehaviour
{
    public abstract void Init();
    public abstract void UpdateCurve(float delta);
    public abstract void Exit();

}