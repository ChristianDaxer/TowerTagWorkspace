using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// <author>Anthony Rosenbaum</author>
/// Delegate Input from PlayerInputBase 
/// </summary>
public abstract class PlayerInput : PlayerInputBase
{
    #region Overrides
    public override bool ToggleDown => throw new System.NotImplementedException();

    public override bool ToggleHold => throw new System.NotImplementedException();

    public override bool ToggleUp => throw new System.NotImplementedException();

    public override bool TriggerDown => throw new System.NotImplementedException();

    public override bool TriggerHold => throw new System.NotImplementedException();

    public override bool TriggerUp => throw new System.NotImplementedException();

    public override bool ModeDown => throw new System.NotImplementedException();

    public override bool ModeHold => throw new System.NotImplementedException();

    public override bool ModeUp => throw new System.NotImplementedException();

    public override bool isValid => throw new System.NotImplementedException();

    public override bool isTracking => throw new System.NotImplementedException();

    public override Vector3 Position => throw new System.NotImplementedException();

    public override Vector3 Direction => throw new System.NotImplementedException();

    public override bool isConnected => throw new System.NotImplementedException();

    public override float triggerValue => throw new System.NotImplementedException();

    public override float gripValue => throw new System.NotImplementedException();
    #endregion

    #region Vars
    protected bool connectionState;
    protected bool trackingState;

    public bool calibrationMode = false;
    #endregion

    #region Methods

    private void Start()
    {
        connectionState = isConnected;
        trackingState = isTracking;
    }
    /// <summary>
    /// For Oculus
    /// </summary>
    void Update() => CheckInput();
    #region Input Polling
    /// <summary>
    /// Maybe always send the Vector2 and Vector3 data
    /// </summary>
    public virtual void CheckInput()
    {
        bool currentConnection = isConnected;
        if (currentConnection != connectionState)
        {
            if(OnConnectStatusChanged != null)
                OnConnectStatusChanged(this, currentConnection);
            connectionState = currentConnection;
        }

        bool currentTracking = isTracking;
        if (currentTracking != trackingState)
        {
            if (OnTrackingStateChanged != null)
                OnTrackingStateChanged(this, currentTracking);
            trackingState = currentTracking;
        }

        if (MenuDown)
        {
            if (OnMenuDown != null)
                OnMenuDown(this);
        }
        if (MenuUp)
        {
            if (OnMenuUp != null)
                OnMenuUp(this);
        }

        if (calibrationMode)
        {
            if(!Move.IsNull())
                if (OnMove != null)
                    OnMove(this, Move);
        }

        if (TriggerDown)
        {
            if (OnTriggerDown != null)
                OnTriggerDown(this, true);
        }

        if (TriggerUp)
        {
            if (OnTriggerUp != null)
                OnTriggerUp(this, false);
        }


        if (ToggleDown)
        {
            if (OnToggleDown != null)
                OnToggleDown(this, true);
        }

        if (ToggleUp)
        {
            if (OnToggleUp != null)
                OnToggleUp(this, false);
        }

    }

    public override void Rumble(float frequency, float amplitude, PlayerInputBase controllerMaskfloat, float secondsFromNow = 0, float durationSeconds = 1)
    {
        throw new System.NotImplementedException();
    }
    #endregion
    #endregion

    #region Delegates

    public delegate void Toggled(bool input);
    public delegate void Floated(PlayerInputBase controller, float input);
    public delegate void StatusChanged(PlayerInputBase controller, bool input);
    public delegate void Buttoned(PlayerInputBase controller);
    public delegate void Vectored2(PlayerInputBase controller, Vector2 input);
    
    public StatusChanged OnConnectStatusChanged;
    public StatusChanged OnTrackingStateChanged;
    public StatusChanged OnTriggerUp;
    public StatusChanged OnTriggerDown;
    public StatusChanged OnToggleDown;
    public StatusChanged OnToggleUp;

    public Floated OnTriggerStateValue;
    public Floated OnGripStateValue;

    public Buttoned OnMenuDown;
    public Buttoned OnMenuUp;

    public Vectored2 OnMove;
    #endregion
}
