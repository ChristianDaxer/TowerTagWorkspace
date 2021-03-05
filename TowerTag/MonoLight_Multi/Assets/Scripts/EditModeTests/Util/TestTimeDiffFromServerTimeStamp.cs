using NUnit.Framework;
using UnityEngine;

public class TestTimeDiffFromServerTimeStamp {
    [Test]
    public void TestTimeDiffFromServerTimeStampSimplePasses() {
        // Use the Assert class to test conditions.

        // min = -1000, max = 1000
        int oldStamp = int.MaxValue; //1000;
        int newStamp = int.MinValue;
        float diff = HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(oldStamp, newStamp);
        Debug.Log("TimeDiff: new: " + newStamp + " old: " + oldStamp + " diff: " + diff);
        Assert.AreEqual(0.001f, diff);

        // min = x max = y
        oldStamp = 1000;
        newStamp = 1500;
        diff = HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(oldStamp, newStamp);
        Debug.Log("TimeDiff: new: " + newStamp + " old: " + oldStamp + " diff: " + diff.ToString("0.0000"));
        Assert.AreEqual(0.5f, diff);

        // min = -1000 max = 1000
        oldStamp = int.MaxValue - 500;
        newStamp = int.MinValue + 500;
        diff = HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(oldStamp, newStamp);
        Debug.Log("TimeDiff: new: " + newStamp + " old: " + oldStamp + " diff: " + diff);
        Assert.AreEqual(1.001f, diff);

        // min = -1000 max = 1000
        oldStamp = int.MaxValue - 500;
        newStamp = int.MinValue + 1500;
        diff = HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(oldStamp, newStamp);
        Debug.Log("TimeDiff: new: " + newStamp + " old: " + oldStamp + " diff: " + diff);
        Assert.AreEqual(2.001f, diff);

        oldStamp = 1500;
        newStamp = 1000;
        diff = HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(oldStamp, newStamp);
        Debug.Log("TimeDiff: new: " + newStamp + " old: " + oldStamp + " diff: " + diff.ToString("0.0000"));
        Assert.AreEqual(-0.5f, diff);

        oldStamp = int.MinValue + 1500;
        newStamp = int.MaxValue - 500;
        diff = HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(oldStamp, newStamp);
        Debug.Log("TimeDiff: new: " + newStamp + " old: " + oldStamp + " diff: " + diff);
        Assert.AreEqual(-2.001f, diff);
    }

    [Test]
    public void TestTimeDiffFromServerTimeStamp_TimeSpans() {
        AddToTimestamp(5000, 2000);
        AddToTimestamp(int.MaxValue, 2000);
        AddToTimestamp(int.MaxValue - 1000, 2000);
        AddToTimestamp(int.MinValue, 2000);
        AddToTimestamp(int.MinValue + 1000, 2000);

        AddToTimestamp(5000, -50000);
        AddToTimestamp(int.MaxValue, -50000);
        AddToTimestamp(int.MaxValue - 1000, -50000);
        AddToTimestamp(int.MinValue, -50000);
        AddToTimestamp(int.MinValue + 1000, -50000);

        SubtractFromTimestamp(5000, 2000);
        SubtractFromTimestamp(int.MaxValue, 2000);
        SubtractFromTimestamp(int.MaxValue - 1000, 2000);
        SubtractFromTimestamp(int.MinValue, 2000);
        SubtractFromTimestamp(int.MinValue + 1000, 2000);

        SubtractFromTimestamp(5000, -50000);
        SubtractFromTimestamp(int.MaxValue, -50000);
        SubtractFromTimestamp(int.MaxValue - 1000, -50000);
        SubtractFromTimestamp(int.MinValue, -50000);
        SubtractFromTimestamp(int.MinValue + 1000, -50000);
    }

    //[Test]
    //public void TestTimeDiffFromServerTimeStamp_TimeSpans_AddToTimestamp_AllValues()
    //{
    //    int minStamp = int.MinValue;
    //    int maxStamp = int.MaxValue;
    //    int minDuration = -3600;
    //    int maxDuration = 3600;
    //    for (int stamp = minStamp; stamp <= maxStamp; stamp++)
    //    {
    //        for (int duration = minDuration; duration <= maxDuration; duration++)
    //        {
    //            float newDuration = HelperFuncs.GetTimeDifferenceInSecFromPhotonServerTimestamps(stamp, HelperFuncs.AddTimeInSecondsToPhotonServerTimestamp(stamp, duration));
    //            if (newDuration != duration)
    //                Debug.LogError("AddToTimestamp: stamp: " + stamp + " duration: " + duration + " calculated duration: " + newDuration);
    //            Assert.IsTrue(newDuration == duration);
    //        }
    //    }
    //}

    void AddToTimestamp(int timestamp, int durationInSec) {
        int oldStamp = timestamp;
        int newStamp = HelperFunctions.AddTimeInSecondsToPhotonServerTimestamp(oldStamp, durationInSec);
        float newDuration = HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(oldStamp, newStamp);
        Debug.Log("AddToTimestamp: new: " + newStamp + " old: " + oldStamp + " diff: " + newDuration);
        Assert.AreEqual(durationInSec, newDuration);
    }

    void SubtractFromTimestamp(int timestamp, int durationInSec) {
        int oldStamp = timestamp;
        int newStamp = HelperFunctions.SubtractTimeInSecondsFromPhotonServerTimestamp(oldStamp, durationInSec);
        float newDuration = HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(oldStamp, newStamp);
        Debug.Log("SubtractFromTimestamp: new: " + newStamp + " old: " + oldStamp + " diff: " + newDuration);
        Assert.AreEqual(-durationInSec, newDuration);
    }
}