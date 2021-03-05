using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputOculus : PlayerInput
{
    public OVRInput.Controller Controller { get { return hand == PlayerHand.Right ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;} }
    
    public override Vector2 Move { get { return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, Controller); } }
    public override Vector2 Look { get { return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, Controller); } }

    public override bool MenuDown { get { return OVRInput.GetDown(OVRInput.Button.Two, Controller); } }
    public override bool MenuHold { get { return OVRInput.Get(OVRInput.Button.Two, Controller); } }
    public override bool MenuUp { get { return OVRInput.GetUp(OVRInput.Button.Two, Controller); } }

    public override bool ToggleDown { get { return OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, Controller); } }
    public override bool ToggleHold { get { return OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, Controller); } }
    public override bool ToggleUp { get { return OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, Controller); } }

    public override bool TriggerDown { get { return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, Controller); } }
    public override bool TriggerHold { get { return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, Controller); } }
    public override bool TriggerUp { get { return OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, Controller); } }

    public override bool ModeDown { get { return OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, Controller); } }
    public override bool ModeHold { get { return OVRInput.Get(OVRInput.Button.PrimaryThumbstick, Controller); } }
    public override bool ModeUp { get { return OVRInput.GetUp(OVRInput.Button.PrimaryThumbstick, Controller); } }

    public override bool isValid { get { return OVRInput.GetControllerOrientationValid(Controller); } }

    public override bool isTracking { get { return OVRInput.GetControllerOrientationTracked(Controller); } }

    public override bool isConnected { get { return OVRInput.IsControllerConnected(Controller); } }

    public override float triggerValue { get { return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, Controller); } }

    public override float gripValue { get { return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, Controller); } }

    public override void Rumble(float frequency, float amplitude, PlayerInputBase controllerMask,
            float secondsFromNow, float durationSeconds)
    {
        OVRInput.SetControllerVibration(frequency, amplitude, Controller);
        StartCoroutine(EndVibration(durationSeconds));
    }

    IEnumerator EndVibration(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        OVRInput.SetControllerVibration(0, 0, Controller);
    }

    public override Vector3 Position { get { return  player ? player.localToWorldMatrix.MultiplyPoint(OVRInput.GetLocalControllerPosition(Controller)) : Vector3.zero; } }
    public override Vector3 Direction
    {
        get
        {
            Quaternion rot = OVRInput.GetLocalControllerRotation(Controller);
            return player ? player.rotation * rot * Vector3.forward: Vector3.zero;
        }
    }

    public override bool ControllerOn => 
        ((OVRInput.GetConnectedControllers() & (hand == PlayerHand.Right ? 
                OVRInput.Controller.RHand : 
                OVRInput.Controller.LHand))
        != OVRInput.Controller.None) || 
        ((OVRInput.GetConnectedControllers() & (hand == PlayerHand.Right ? 
                OVRInput.Controller.RTouch : 
                OVRInput.Controller.LTouch))
        != OVRInput.Controller.None);

    private Transform player;
    private Transform controllerTransform;

    protected static new bool FindPlatformInstanceOfHand(PlayerHand hand, out PlayerInputBase input) 
    { 
        return FindInstanceOfHand<PlayerInputOculus>(hand, out input);
    }

    public void Initialize (Transform transform, Transform controllerTransform)
    {
        this.player = transform;
        this.controllerTransform = controllerTransform;
    }
}
