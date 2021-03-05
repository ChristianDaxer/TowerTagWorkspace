using UnityEngine;

/// <summary>
/// Custom Logger class to get conventional log messages for better tracing
/// </summary>
public class Logger : MonoBehaviour {

    /// <summary>
    /// prefix to make traceable log outputs
    /// </summary>
    private string _defaultLogPrefix;

    protected void Awake() {
        _defaultLogPrefix = name + ":" + GetType().Name + " - ";
    }

    protected void Log(string message) {
        Debug.Log(_defaultLogPrefix + message);
    }

    protected void LogWarning(string message) {
        Debug.LogWarning(_defaultLogPrefix + message);
    }

    protected void LogError(string message) {
        Debug.LogError(_defaultLogPrefix + message);
    }
}
