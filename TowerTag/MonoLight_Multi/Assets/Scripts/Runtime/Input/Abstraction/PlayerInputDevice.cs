using System;
using UnityEngine;

public enum PlayerInputState
{
    Down,
    Hold,
    Up
}

public enum PlayerInputType
{
    Toggle,
    Trigger,
    Mode,
    Menu
}

public enum PlayerHand
{
    Left,
    Right
}

public interface PlayerInputDevice
{
    Vector2 Move { get; }
    Vector2 Look { get; }

    bool ToggleDown { get; }
    bool ToggleHold { get; }
    bool ToggleUp { get; }

    bool TriggerDown { get; }
    bool TriggerHold { get; }
    bool TriggerUp { get; }

    bool ModeDown { get; }
    bool ModeHold { get; }
    bool ModeUp { get; }

    bool MenuDown { get; }
    bool MenuHold { get; }
    bool MenuUp { get; }

    Vector3 Position { get; }
    Vector3 Direction { get; }
}

