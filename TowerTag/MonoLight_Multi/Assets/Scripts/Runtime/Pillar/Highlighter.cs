using System;
using TowerTag;
using UnityEngine;

public abstract class Highlighter : MonoBehaviour {
    public event Action<bool> Toggled;
    private bool _active;

    public void ShowHighlight(bool active, IPlayer highlightRequester) {
        if (active != _active) {
            _active = active;
            ChangeHighlight(active, highlightRequester);
        }
    }

    protected abstract void ChangeHighlight(bool highlight, IPlayer highlightRequester);

    protected void Toggle(bool active) {
        Toggled?.Invoke(active);
    }

    public virtual bool IsAllowedToHighlight(IPlayer highlightRequester) {
        return true;
    }

    private void OnDestroy() {
        Toggled = null;
    }
}
