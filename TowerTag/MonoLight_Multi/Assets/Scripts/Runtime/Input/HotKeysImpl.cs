using SOEventSystem.Shared;
using System;
using System.Collections;
using UnityEngine;

public abstract class HotKeys : ScriptableObject {
    public event Action OperatorUIToggled;
    public event Action SpectatorUIToggled;
    public event Action MuteToggled;

    protected void ToggleOperatorUI() {
        OperatorUIToggled?.Invoke();
    }

    protected void ToggleSpectatorUI() {
        SpectatorUIToggled?.Invoke();
    }

    protected void ToggleMute() {
        MuteToggled?.Invoke();
    }

    /// <summary>
    /// Coroutine for processing hot keys input and invoking the respective events.
    /// </summary>
    public abstract IEnumerator Listen();
}

/// <summary>
/// Event system for hot keys input.
/// Use a <see cref="HotKeysController"/> to run the <see cref="Listen"/> coroutine.
///
/// <author>Ole Jürgensen (ole@vrnerds.de)</author>
/// </summary>
[CreateAssetMenu(menuName = "Hot Keys")]
public class HotKeysImpl : HotKeys {
    [SerializeField, Tooltip("True when the in-game bug reporting tool is currently being used.")]
    private SharedBool _reportingBug;

    [SerializeField, Tooltip("True when a player name is currently being edited.")]
    private SharedBool _editingPlayerName;

    [SerializeField, Tooltip("True when a team name is currently being edited.")]
    private SharedBool _editingTeamName;

    [SerializeField, Tooltip("True when the operator is waiting for a QR code scan")]
    private SharedBool _scanningQRCode;

    [SerializeField, Tooltip("Hot Key to toggle operator UI")]
    private KeyCode _toggleOperatorUI = KeyCode.G;

    [SerializeField, Tooltip("Hot Key to toggle spectator UI")]
    private KeyCode _toggleSpectatorUI = KeyCode.H;

    [SerializeField, Tooltip("Hot Key to mute/unmute the game")]
    private KeyCode _mute = KeyCode.M;

    private bool HotKeysEnabled => !_reportingBug && !_editingPlayerName && !_editingTeamName && !_scanningQRCode;

    private bool _enabled;

    private void OnEnable() {
        _enabled = true;
    }

    private void OnDisable() {
        _enabled = false;
    }

    /// <inheritdoc />
    public override IEnumerator Listen() {
        while (_enabled) {
            yield return null;
            if (!HotKeysEnabled)
                continue;
            if (Input.GetKeyDown(_toggleOperatorUI))
                ToggleOperatorUI();
            if (Input.GetKeyDown(_toggleSpectatorUI))
                ToggleSpectatorUI();
            if (Input.GetKeyDown(_mute))
                ToggleMute();
        }
    }
}