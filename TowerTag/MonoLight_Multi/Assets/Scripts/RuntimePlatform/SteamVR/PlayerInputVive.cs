using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve;
using Valve.VR;

public class PlayerInputVive : PlayerInput
{
    private enum PressType
    {
        Axis,
        Down,
        Hold,
        Up
    }

    private enum Hand
    {
        Left,
        Right
    }

    public SteamVR_Behaviour_Pose _controller = null;
    [SerializeField] private SteamVR_Action_Vibration _controllerRumble;
    public SteamVR_Action_Boolean shootAction;
    public SteamVR_Action_Boolean toggleAction;
    public SteamVR_Action_Boolean menuAction;

    private void Start()
    {
        if (_controller == null)
            _controller = GetComponent<SteamVR_Behaviour_Pose>();
    }

    private readonly Dictionary<PressType, Func<EVRButtonId, object>> axis = new Dictionary<PressType, Func<EVRButtonId, object>>();

    // public override Vector2 Move { get { return (Vector2)axis[PressType.Axis](EVRButtonId.k_EButton_SteamVR_Touchpad); } }
    // public override Vector2 Look { get { return (Vector2)axis[PressType.Axis](EVRButtonId.k_EButton_SteamVR_Touchpad); } }

    // public override bool ToggleDown { get { return (bool)axis[PressType.Down](EVRButtonId.k_EButton_SteamVR_Touchpad); } }
    // public override bool ToggleHold { get { return (bool)axis[PressType.Hold](EVRButtonId.k_EButton_SteamVR_Touchpad); } }
    // public override bool ToggleUp { get { return (bool)axis[PressType.Up](EVRButtonId.k_EButton_SteamVR_Touchpad); } }

    // public override bool MenuDown { get { return (bool)axis[PressType.Down](EVRButtonId.k_EButton_ApplicationMenu); } }
    // public override bool MenuHold { get { return (bool)axis[PressType.Hold](EVRButtonId.k_EButton_ApplicationMenu); } }
    // public override bool MenuUp { get { return (bool)axis[PressType.Up](EVRButtonId.k_EButton_ApplicationMenu); } }

    // public override bool TriggerDown { get { return (bool)axis[PressType.Down](EVRButtonId.k_EButton_SteamVR_Trigger); } }
    // public override bool TriggerHold { get { return (bool)axis[PressType.Hold](EVRButtonId.k_EButton_SteamVR_Trigger); } }
    // public override bool TriggerUp { get { return (bool)axis[PressType.Up](EVRButtonId.k_EButton_SteamVR_Trigger); } }

    // public override bool ModeDown { get { return (bool)axis[PressType.Down](EVRButtonId.k_EButton_Grip); } }
    // public override bool ModeHold { get { return (bool)axis[PressType.Hold](EVRButtonId.k_EButton_Grip); } }
    // public override bool ModeUp { get { return (bool)axis[PressType.Up](EVRButtonId.k_EButton_Grip); } }

    // public override bool isValid { get { return (bool)_controller.isValid;  } }
    // public override bool isTracking { get { return (bool) (_controller.poseAction.trackingState == ETrackingResult.Running_OK); } }
    // public override bool isConnected { get { return (bool)_controller.poseAction.deviceIsConnected; } }
    // public override float triggerValue { get { return SteamVR_Input.GetFloat("GrabPinch", _controller.inputSource); } }
    // public override float gripValue { get { return SteamVR_Input.GetFloat("GrabGrip", _controller.inputSource); } }

    // public override Vector3 Position { get { return controllerTransform == null ? Vector3.zero : controllerTransform.position; } }
    // public override Vector3 Direction { get { return controllerTransform == null ? Vector3.forward : controllerTransform.forward; } } 

    public override Vector2 Move { get { return Vector2.zero; } }
    public override Vector2 Look { get { return Vector2.zero; } }

    public override bool ToggleDown { get { return toggleAction.stateDown; } }
    public override bool ToggleHold { get { return toggleAction.state; } }
    public override bool ToggleUp { get { return toggleAction.stateUp; } }

    public override bool MenuDown { get { return menuAction.stateDown; } }
    public override bool MenuHold { get { return menuAction.state; } }
    public override bool MenuUp { get { return menuAction.stateUp; } }

    public override bool TriggerDown { get { return shootAction.stateDown; } }
    public override bool TriggerHold { get { return shootAction.state; } }
    public override bool TriggerUp { get { return shootAction.stateUp; } }

    public override bool ModeDown { get { return false; } }
    public override bool ModeHold { get { return false; } }
    public override bool ModeUp { get { return false; } }

    public override bool isValid { get { return true;  } }
    public override bool isTracking { get { return true; } }
    public override bool isConnected { get { return true; } }
    public override float triggerValue { get { return 0.0f; } }
    public override float gripValue { get { return 0.0f; } }

    public override Vector3 Position { get { return controllerTransform == null ? Vector3.zero : controllerTransform.position; } }
    public override Vector3 Direction { get { return controllerTransform == null ? Vector3.forward : controllerTransform.forward; } }

    // TODO
    public override bool ControllerOn => true;

    public override void Rumble(float frequency, float amplitude, PlayerInputBase controllerMask, float secondsFromNow, float durationSeconds)
    {
        PlayerInputVive vive = (PlayerInputVive)controllerMask;
        _controllerRumble.Execute( secondsFromNow,  durationSeconds,  frequency,  amplitude, vive._controller.inputSource);
    }


    protected static new bool FindPlatformInstanceOfHand(PlayerHand hand, out PlayerInputBase input) 
    { 
        return FindInstanceOfHand<PlayerInputVive>(hand, out input);
    }

    public override void CheckInput()
    {
        transform.localPosition = _controller.poseAction.localPosition;
        transform.localRotation = _controller.poseAction.localRotation;
        base.CheckInput();
    }

    private Transform controllerTransform;
}
