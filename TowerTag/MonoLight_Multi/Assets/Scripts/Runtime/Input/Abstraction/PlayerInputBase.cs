using System;
using UnityEngine;

public abstract class PlayerInputBase : MonoBehaviour, PlayerInputDevice
{
    public static PlayerInputBase leftHand;
    public static PlayerInputBase rightHand;

    public abstract bool ControllerOn { get; }

    private void Awake()
    {
        if (hand == PlayerHand.Left)
            leftHand = this;
        else rightHand = this;
    }

    public static bool GetInstance (PlayerHand hand, out PlayerInputBase input)
    {
        input = hand == PlayerHand.Left ? leftHand : rightHand;

        if (input == null)
            return TryFindActiveHandController(hand, out input);

        return true;
    }

    protected static bool TryFindActiveHandController(PlayerHand hand, out PlayerInputBase input) 
    {
        // Attempt to locate instance of target hand controller.
        if (!FindInstanceOfHand<PlayerInputBase>(hand, out input))
            return false;

        // If the target hand controller is on, we've found what we were looking for, otherwise
        // fallback to the other hand controller.
        if (input.ControllerOn)
            return true;

        // Attempt to locate instance of the fallback hand controller.
        if (!FindInstanceOfHand<PlayerInputBase>(hand == PlayerHand.Right ? PlayerHand.Left : PlayerHand.Right, out input))
            return false;

        // Return whether that fallback controller is on.
        return input.ControllerOn;
    }

    protected static bool FindInstanceOfHand<T> (PlayerHand hand, out PlayerInputBase input) where T : PlayerInputBase
    {
        input = null;
        if (hand == PlayerHand.Left)
        {
            var hands = FindObjectsOfType<T>();
            if (hands.Length == 0)
            {
                Debug.LogErrorFormat("There are no instances of {0} in the scene.", typeof(T).Name);
                input = null;
                return false;
            }
            if (hands.Length > 2)
            {
                Debug.LogErrorFormat("There more then 2 instances of {0} in the scene.", typeof(T).Name);
                input = null;
                return false;
            }

            for (int i = 0; i < hands.Length; i++)
            {
                if (hands[i].hand != hand)
                    continue;
                input = hands[i];
                if (hand == PlayerHand.Left)
                    leftHand = input;
                else rightHand = input;
                break;
            }
        }

        return input != null;
    }

    public PlayerHand hand;
    public virtual Vector2 Move { get { return Vector2.zero; } }
    public virtual Vector2 Look { get { return Vector2.zero; } }

    public virtual bool MenuDown { get { return false; } }
    public virtual bool MenuHold { get { return false; } }
    public virtual bool MenuUp { get { return false; } }

    public abstract bool ToggleDown { get; }
    public abstract bool ToggleHold { get; }
    public abstract bool ToggleUp { get; }

    public abstract bool TriggerDown { get; }
    public abstract bool TriggerHold { get; }
    public abstract bool TriggerUp { get; }

    public abstract bool ModeDown { get; }
    public abstract bool ModeHold { get; }
    public abstract bool ModeUp { get; }

    public abstract bool isValid { get; }

    public abstract bool isTracking { get; }

    public abstract bool isConnected{ get; }

    public abstract float triggerValue { get; }

    public abstract float gripValue { get; }

    public abstract void Rumble(float frequency, float amplitude, PlayerInputBase controllerMaskfloat, float secondsFromNow=0, float durationSeconds=1);

    public abstract Vector3 Position { get; }
    public abstract Vector3 Direction { get; }
}
