using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputMouse : PlayerInput {
    public override Vector2 Move { get { return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); } }
    public override Vector2 Look { get { return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); } }

    public override bool ToggleDown { get { return Input.GetMouseButtonDown(1); } }
    public override bool ToggleHold { get { return Input.GetMouseButton(1); } }
    public override bool ToggleUp { get { return Input.GetMouseButtonUp(1); } }

    public override bool TriggerDown { get { return Input.GetMouseButtonDown(0); } }
    public override bool TriggerHold { get { return Input.GetMouseButton(0); } }
    public override bool TriggerUp { get { return Input.GetMouseButtonUp(0); } }

    public override bool ModeDown { get { return Input.GetKeyDown(KeyCode.Space); } }
    public override bool ModeHold { get { return Input.GetKey(KeyCode.Space); } }
    public override bool ModeUp { get { return Input.GetKeyUp(KeyCode.Space); } }

    public override bool isValid { get { return true; } }

    public override bool isTracking { get { return true; } }

    public override Vector3 Position { get { return transform.position; } }
    public override Vector3 Direction { get { return transform.forward; } }

    public override bool isConnected { get { return true; } }

    public override float triggerValue { get { return Convert.ToSingle(Input.GetMouseButton(0)); } }

    public override float gripValue { get { return Convert.ToSingle(Input.GetMouseButton(1)); } }

    public override bool ControllerOn => true;

    public override void Rumble(float frequency, float amplitude, PlayerInputBase controllerMaskfloat, float secondsFromNow = 0, float durationSeconds = 1)
    {
    }
}