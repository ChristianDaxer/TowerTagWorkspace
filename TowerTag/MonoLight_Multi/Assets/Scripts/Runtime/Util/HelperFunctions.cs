using UnityEngine;
using System.Collections;


/// <summary>
/// Class with HelperFunctions (for convenience only).
/// </summary>
public static class HelperFunctions
{
    /// <summary>
    /// Check if a given layer is set in the given Layer mask (corresponding bit is 1).
    /// </summary>
    /// <param name="layerMask">Layer mask to check bits in.</param>
    /// <param name="layer">Layer (bit) to check. [0..31]</param>
    /// <returns>If the Layer is set (corresponding bit is 1).</returns>
    public static bool IsLayerInLayerMask(LayerMask layerMask, int layer)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    /// <summary>
    /// Simple Helper Coroutine which waits for a given duration and calls the pauseFinishedCallback afterwards (when finished waiting).
    /// </summary>
    /// <param name="duration">Duration to wait in seconds.</param>
    /// <param name="pauseFinishedCallback">Callback function to call (could also be null if no function needs to be called) when finished waiting.</param>
    /// <returns>IEnumerator for yield.</returns>
    public static IEnumerator Wait(float duration, System.Action pauseFinishedCallback)
    {
        yield return new WaitForSecondsRealtime(duration);

        pauseFinishedCallback?.Invoke();
    }

    /// <summary>
    /// Simple Helper Coroutine which waits for a given duration and calls the pauseFinishedCallback afterwards (when finished waiting).
    /// This wait function count down the waiting time only if the given timer is not paused or resuming from pause.
    /// </summary>
    /// <param name="duration">Duration to wait in seconds.</param>
    /// <param name="timer">Timer to check if it is paused.</param>
    /// <param name="pauseFinishedCallback">Callback function to call (could also be null if no function needs to be called) when finished waiting.</param>
    /// <returns>IEnumerator for yield.</returns>
    public static IEnumerator WaitWithMatchTimerPause(float duration, MatchTimer timer, System.Action pauseFinishedCallback)
    {
        while (duration > 0)
        {
            if (timer == null || !timer.IsPaused && !timer.IsResumingFromPause)
            {
                duration -= Time.deltaTime;
            }

            yield return null;
        }

        pauseFinishedCallback?.Invoke();
    }

    /// <summary>
    /// De Bruijn coefficients to calculate the Log2_DeBruijn
    /// </summary>
    private static readonly int[] _multiplyDeBruijnBitPosition = {
        0,  9,  1, 10, 13, 21,  2, 29, 11, 14, 16, 18, 22, 25,  3, 30,
        8, 12, 20, 28, 15, 17, 24,  7, 19, 27, 23,  6, 26,  5,  4, 31
    };

    /// <summary>
    /// Fast Log2 function (using the de Bruijn sequence) -> Log2(value).
    /// </summary>
    /// <param name="value">Value to calculate the Log2 from.</param>
    /// <returns>The Logarithm dualis (base 2) of value.</returns>
    public static int Log2_DeBruijn(int value)
    {
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;

        return _multiplyDeBruijnBitPosition[(uint)(value * 0x07C4ACDDU) >> 27];
    }

    /// <summary>
    /// Calculates the timespan (in seconds) between to PhotonServer timestamps. (Photons Server timestamps are in a range of [int.minValue..int.maxValue] and jump from max int value (+) to min int value (-), it starts at an arbitrary value -> so never use the absolute values, only use timespans!!!)
    /// </summary>
    /// <param name="oldPhotonServerTimeStamp">The older Server timestamp.</param>
    /// <param name="newPhotonServerTimestamp">The newer Server timestamp.</param>
    /// <returns>Timespan between old and new Server timestamp in seconds (can be negative if older > newer, but ensures right values if the timestamps jumps around from positive to negative by int overflow).</returns>
    public static float GetTimeDifferenceInSecFromPhotonServerTimestamps(int oldPhotonServerTimeStamp, int newPhotonServerTimestamp)
    {
        return (newPhotonServerTimestamp - oldPhotonServerTimeStamp) / 1000f;
    }

    /// <summary>
    /// Add a given timespan in seconds to the given timestamp (to handle timespans with PhotonServerTimestamps).
    /// </summary>
    /// <param name="timestamp">Timestamp to add timespan to.</param>
    /// <param name="timeInSeconds">Timespan to add in seconds.</param>
    /// <returns>Returns new timestamp.</returns>
    public static int AddTimeInSecondsToPhotonServerTimestamp(int timestamp, int timeInSeconds)
    {
        return timestamp + timeInSeconds * 1000;
    }

    /// <summary>
    /// Add a given timespan in seconds to the given timestamp (to handle timespans with PhotonServerTimestamps).
    /// </summary>
    /// <param name="timestamp">Timestamp to add timespan to.</param>
    /// <param name="timeInSeconds">Timespan to add in seconds.</param>
    /// <returns>Returns new timestamp.</returns>
    public static int AddTimeInSecondsToPhotonServerTimestamp(int timestamp, float timeInSeconds)
    {
        return timestamp + (int)(timeInSeconds * 1000);
    }

    /// <summary>
    /// Subtracts a given timespan in seconds from the given timestamp (to handle timespans with PhotonServerTimestamps).
    /// </summary>
    /// <param name="timestamp">Timestamp to subtract timespan from.</param>
    /// <param name="timeInSeconds">Timespan to subtract in seconds.</param>
    /// <returns>Returns new timestamp.</returns>
    public static int SubtractTimeInSecondsFromPhotonServerTimestamp(int timestamp, int timeInSeconds)
    {
        return timestamp - timeInSeconds * 1000;
    }
    /// <summary>
    /// Subtracts a given timespan in seconds from the given timestamp (to handle timespans with PhotonServerTimestamps).
    /// </summary>
    /// <param name="timestamp">Timestamp to subtract timespan from.</param>
    /// <param name="timeInSeconds">Timespan to subtract in seconds.</param>
    /// <returns>Returns new timestamp.</returns>
    public static int SubtractTimeInSecondsFromPhotonServerTimestamp(int timestamp, float timeInSeconds)
    {
        return timestamp - (int)(timeInSeconds * 1000);
    }


    /// <summary>
    /// Security check of null pointer (if a normal null check just not is enough)!
    /// </summary>
    /// <param name="obj">Object to check for null.</param>
    /// <returns>true if object is null, false otherwise</returns>
    public static bool IsNull(this object obj)
    {
        return ReferenceEquals(obj, null) || obj.Equals(null);
    }

    /// <summary>
    /// Calculates incremental averages of values instead of (X1 + ...+ Xn)/n.
    /// Attention this function is not accurate -> little errors accumulate over time so it can introduce
    /// significant error if used over a long period!
    /// </summary>
    /// <param name="oldValue">Old average (average of values till now).</param>
    /// <param name="oldSampleCount">Number of added values in average (till now).</param>
    /// <param name="newValue">New value to add to the average.</param>
    /// <returns>New average of values (including new value).</returns>
    public static float CalculateIncrementalAverage(float oldValue, int oldSampleCount, float newValue)
    {
        return oldValue * ((float)oldSampleCount / (oldSampleCount + 1)) + newValue / (oldSampleCount + 1);
    }

    /// <summary>
    /// Converts a linear volume scale from 0-1 to it's decibel value
    /// </summary>
    /// <param name="linear">the linear volume</param>
    /// <returns>the volume in decibel</returns>
    public static float LinearVolumeToDecibel(float linear) {
        float dB;

        if( linear > 0.0f )
            dB = 20.0f * Mathf.Log10(linear);
        else
            dB = -144.0f;

        return dB;
    }

    /// <summary>
    /// Converts a decibel volume from to it's linear value
    /// </summary>
    /// <param name="dB">the decibel volume</param>
    /// <returns>the linear volume</returns>
    public static float DecibelVolumeToLinear(float dB) {
        float linear = Mathf.Pow(10.0f, dB / 20.0f);

        return linear;
    }

    /// <summary>
    /// Prints content of the given array to a string separated with separator (default is " | ").
    /// </summary>
    /// <typeparam name="T">Type of the elements of the array.</typeparam>
    /// <param name="array">The array which elements should be printed to string.</param>
    /// <param name="separator">The string that should be used as separator between array elements.</param>
    /// <returns>String with elements, empty string if array is null or empty.</returns>
    public static string PrintArrayToString<T>(T[] array, string separator = " | ")
    {
        if (array == null)
            return "";

        var builder = new System.Text.StringBuilder();

        int lastIndex = array.Length - 1;
        for (var i = 0; i < array.Length; i++)
        {
            builder.Append(array[i]);

            if (i < lastIndex)
                builder.Append(separator);
        }

        return builder.ToString();
    }
}
