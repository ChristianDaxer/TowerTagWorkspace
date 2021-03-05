using UnityEngine;

/// <summary>
/// Tag-Class to assign UI-Elements that needs some Badaboom-Pointer action
/// </summary>
public class BadaboomHyperactionPointerNeeded : MonoBehaviour {

    private void OnEnable() {
        BadaboomHyperactionPointer.RegisterAvailableInterface(this);
    }

    private void OnDisable()
    {
        BadaboomHyperactionPointer.UnregisterAvailableInterface(this);
    }
}