using UnityEngine;

public class DeactivateInBasicMode : MonoBehaviour
{
    private void Start()
    {
        if(TowerTagSettings.BasicMode) gameObject.SetActive(false);
    }
}
