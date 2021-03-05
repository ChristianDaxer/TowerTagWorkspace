using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Play Animation as Coroutine to fade in/out image effects.
/// </summary>
/// <returns>IEnumerator for Coroutine.</returns>
public static class LerpAnimationHelper {
    /// <summary>
    /// Play Animation as Coroutine to fade in/out image effects.
    /// </summary>
    /// <param name="fadeIn">If true the effects fade in, if false the effects will fade out.</param>
    /// <param name="duration">Time to fade effects in or out.</param>
    /// <param name="fadeValueCallback">FadeValueCallback will be triggered in every frame. Gives you the normalized vale of the animation (runs from 0 to 1 (fadein) or 1 to 0 (fadeout)).</param>
    /// <param name="enableCallback">EnableCallback is triggered if effects should be enabled before fade in or disabled after fadeout. Set this to null if you don't need to enable/disable</param>
    /// <returns></returns>
    public static IEnumerator PlayFadeAnimation(bool fadeIn, float duration, Action<float> fadeValueCallback,
        Action<bool> enableCallback = null) {
        // enable effects before fadeIn
        if (enableCallback != null && fadeIn)
            enableCallback(true);

        // time this animation started
        float startTime = Time.realtimeSinceStartup;

        // normalized animation time: runs from 0 to 1 (independent of fadeIn/fadeOut)
        float delta = 0;

        // normalized position in Animation: runs from fromValue to toValue (0 -> 1 (fade in effect) or 1 -> 0 (fadeout))
        float normalizedAnimationPosition;

        while (delta <= 1) {
            // normalized animation time: runs from 0 to 1 (always)
            delta = (Time.realtimeSinceStartup - startTime) / duration;

            // normalized position in Animation: runs from fromValue to toValue (0 -> 1 (fade in effect) or 1 -> 0 (fadeout))
            normalizedAnimationPosition = fadeIn ? delta : 1 - delta;

            // apply animation value
            fadeValueCallback?.Invoke(normalizedAnimationPosition);

            yield return new WaitForEndOfFrame();
        }

        // finish animation (set final (target) value)
        normalizedAnimationPosition = fadeIn ? 1 : 0;
        fadeValueCallback?.Invoke(normalizedAnimationPosition);

        // disable effects after fadeOut
        if (enableCallback != null && !fadeIn)
            enableCallback(false);
    }
}