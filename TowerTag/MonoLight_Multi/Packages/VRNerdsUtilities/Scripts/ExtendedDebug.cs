using System;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Extended version of <see cref="UnityEngine.Debug"/>.
/// Most importantly, log statements are dressed with a timestamp prefix.
/// </summary>
/// <author>Ole Jürgensen (ole@vrnerds.de)</author>
// ReSharper disable once CheckNamespace - Has to be in global namespace to hide UnityEngine.Debug
public static class Debug {
    // ReSharper disable once InconsistentNaming - consistency with UnityEngine.Debug
    public static bool developerConsoleVisible {
        get { return UnityEngine.Debug.developerConsoleVisible; }
        set { UnityEngine.Debug.developerConsoleVisible = value; }
    }

    // ReSharper disable once InconsistentNaming - consistency with UnityEngine.Debug
    public static bool isDebugBuild {
        get { return UnityEngine.Debug.isDebugBuild; }
    }

    // ReSharper disable once InconsistentNaming - consistency with UnityEngine.Debug
    public static ILogger unityLogger {
        get { return UnityEngine.Debug.unityLogger; }
    }

    public static void Log(object obj, Object context = null) {
        if (context == null)
            UnityEngine.Debug.Log(GetTimestampPrefix() + obj);
        else
            UnityEngine.Debug.Log(GetTimestampPrefix() + obj, context);
    }
    
    public static void LogInColors(object obj, Color color, Object context = null) {
        if (context == null)
            UnityEngine.Debug.Log(GetTimestampPrefix() + $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}> {obj}</color>");
        else
            UnityEngine.Debug.Log(GetTimestampPrefix() + $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}> {obj}</color>", context);
    }

    public static void LogFormat(string format, params object[] args) {
        UnityEngine.Debug.LogFormat(GetTimestampPrefix() + format, args);
    }

    public static void LogFormat(Object context, string format, params object[] args) {
        UnityEngine.Debug.LogFormat(context, GetTimestampPrefix() + format, args);
    }

    public static void LogWarning(object obj, Object context = null) {
        if (context == null)
            UnityEngine.Debug.LogWarning(GetTimestampPrefix() + obj);
        else
            UnityEngine.Debug.LogWarning(GetTimestampPrefix() + obj, context);
    }

    public static void LogWarningFormat(string format, params object[] args) {
        UnityEngine.Debug.LogWarningFormat(GetTimestampPrefix() + format, args);
    }

    public static void LogWarningFormat(Object context, string format, params object[] args) {
        UnityEngine.Debug.LogWarningFormat(context, GetTimestampPrefix() + format, args);
    }

    public static void LogError(object obj, Object context = null) {
        if (context == null)
            UnityEngine.Debug.LogError(GetTimestampPrefix() + obj);
        else
            UnityEngine.Debug.LogError(GetTimestampPrefix() + obj, context);
    }

    public static void LogErrorFormat(string format, params object[] args) {
        UnityEngine.Debug.LogErrorFormat(GetTimestampPrefix() + format, args);
    }

    public static void LogErrorFormat(Object context, string format, params object[] args) {
        UnityEngine.Debug.LogErrorFormat(context, GetTimestampPrefix() + format, args);
    }

    public static void LogException(Exception exception) {
        UnityEngine.Debug.LogException(exception);
    }

    public static void LogException(Exception exception, Object context) {
        UnityEngine.Debug.LogException(exception, context);
    }

    public static void LogAssertion(object obj, Object context = null) {
        if (context == null)
            UnityEngine.Debug.LogAssertion(GetTimestampPrefix() + obj);
        else
            UnityEngine.Debug.LogAssertion(GetTimestampPrefix() + obj, context);
    }

    public static void LogAssertionFormat(string format, params object[] args) {
        UnityEngine.Debug.LogAssertionFormat(GetTimestampPrefix() + format, args);
    }

    public static void LogAssertionFormat(Object context, string format, params object[] args) {
        UnityEngine.Debug.LogAssertionFormat(context, GetTimestampPrefix() + format, args);
    }

    #region Assert

    public static void Assert(bool condition) {
        UnityEngine.Debug.Assert(condition);
    }

    public static void Assert(bool condition, Object context) {
        UnityEngine.Debug.Assert(condition, context);
    }

    public static void Assert(bool condition, object message) {
        UnityEngine.Debug.Assert(condition, message);
    }

    public static void Assert(bool condition, object message, Object context) {
        UnityEngine.Debug.Assert(condition, message, context);
    }

    public static void Assert(bool condition, string message) {
        UnityEngine.Debug.Assert(condition, message);
    }

    public static void Assert(bool condition, string message, Object context) {
        UnityEngine.Debug.Assert(condition, message, context);
    }

    public static void AssertFormat(bool condition, string format, params object[] args) {
        UnityEngine.Debug.AssertFormat(condition, format, args);
    }

    public static void AssertFormat(bool condition, Object context, string format, params object[] args) {
        UnityEngine.Debug.AssertFormat(condition, context, format, args);
    }

    #endregion

    public static void Break() {
        UnityEngine.Debug.Break();
    }

    public static void ClearDeveloperConsole() {
        UnityEngine.Debug.ClearDeveloperConsole();
    }

    public static void DebugBreak() {
        UnityEngine.Debug.DebugBreak();
    }

    public static void DrawLine(Vector3 start, Vector3 dir, Color color, float duration = 0f, bool depthTest = true) {
        UnityEngine.Debug.DrawLine(start, dir, color, duration, depthTest);
    }

    public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration = 0f, bool depthTest = true) {
        UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
    }

    public static void DrawLine(Vector3 start, Vector3 dir) {
        DrawLine(start, dir, Color.white);
    }

    public static void DrawRay(Vector3 start, Vector3 dir) {
        DrawRay(start, dir, Color.white);
    }

    private static string GetTimestampPrefix() {
        return GetTimestampInUTC() + "UTC: ";
    }

    private static string GetTimestampInUTC() {
        return DateTime.UtcNow.ToString("HH:mm:ss.fff");
    }
}