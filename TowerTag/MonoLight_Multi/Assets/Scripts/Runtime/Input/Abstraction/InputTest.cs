using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTest : MonoBehaviour
{
    public PlayerInput controller;

    void Start()
    {
        if (controller)
        {  
            controller.OnConnectStatusChanged += OnConnectStatusChanged;
            controller.OnTrackingStateChanged += OnTrackingStateChanged;


            controller.OnTriggerStateValue += OnTriggerStateValue;
            controller.OnGripStateValue += OnGripStateValue;

        } 
    }

    void OnDestroy()
    {
        if (controller)
        {

            controller.OnConnectStatusChanged -= OnConnectStatusChanged;
            controller.OnTrackingStateChanged -= OnTrackingStateChanged;

            controller.OnTriggerStateValue -= OnTriggerStateValue;
            controller.OnGripStateValue -= OnGripStateValue;
        }
    }

    public void OnConnectStatusChanged(PlayerInputBase controller, bool input) { Debug.Log(" OnConnectStatusChanged " + input.ToString()); }
    public void OnTrackingStateChanged(PlayerInputBase controller, bool input) { Debug.Log(" OnTrackingStateChanged " + input.ToString()); }

    public void OnTriggerStateValue(PlayerInputBase controller, float input) { Debug.Log(" OnTriggerStateValue " + input.ToString()); }
    public void OnGripStateValue(PlayerInputBase controller, float input) { Debug.Log(" OnGripStateValue " + input.ToString()); }


    public void OnToggleDown(bool input) { Debug.Log(" OnToggleDown " + input.ToString()); }
    public void OnToggleHold(bool input) { Debug.Log(" OnToggleHold " + input.ToString()); }
    public void OnToggleUp(bool input) { Debug.Log(" OnToggleUp " + input.ToString()); }

    public void OnMenuDown(bool input) { Debug.Log(" OnMenuDown " + input.ToString()); }
    public void OnMenuHold(bool input) { Debug.Log(" OnMenuHold " + input.ToString()); }
    public void OnMenuUp(bool input) { Debug.Log(" OnMenuUp " + input.ToString()); }

    public void OnTriggerDown(bool input) { Debug.Log(" OnTriggerDown " + input.ToString()); }
    public void OnTriggerHold(bool input) { Debug.Log(" OnTriggerHold " + input.ToString()); }
    public void OnTriggerUp(bool input) { Debug.Log(" OnTriggerUp " + input.ToString()); }

    public void OnModeDown(bool input) { Debug.Log(" OnModeDown " + input.ToString()); }
    public void OnModeHold(bool input) { Debug.Log(" OnModeHold " + input.ToString()); }
    public void OnModeUp(bool input) { Debug.Log(" OnModeUp " + input.ToString()); }

}
