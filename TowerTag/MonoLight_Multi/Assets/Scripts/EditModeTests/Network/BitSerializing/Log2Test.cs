using UnityEngine;
using UnityEngine.Assertions;

public class Log2Test : MonoBehaviour {

    void Start() {
        // TestDeBruijnCorrectness(minValue, maxValue);
        TestDeBruijnCorrectness_2();

        int min = BitCompressionConstants.MinPlayerID;
        int max = BitCompressionConstants.MaxPlayerID;
        int bits = GetBitCount_Debruijn(min, max);
        Debug.Log("GetBitCount: min: " + min + " max " + max + " diff " + (max - min) + " -> bits: " + bits);
    }

    void TestDeBruijnCorrectness_2() {
        int numTests = 32;
        for (int i = 0; i < numTests; i++) {
            CheckBits(-1, i);
        }
    }
    void CheckBits(int minValue, int maxValue) {
        int diff = maxValue - minValue;
        int check = GetBitCount(minValue, maxValue);
        int deBruijn = GetBitCount_Debruijn(minValue, maxValue);
        Assert.IsTrue(deBruijn == check, "" + check + " -> " + deBruijn);
        Debug.Log("CheckBits: " + diff + check + " -> " + deBruijn);
    }

    int GetBitCount(int min, int max) {
        Assert.IsTrue(max > min);
        long diff = max - (long)min;
        Assert.IsTrue(diff < float.MaxValue);

        if (diff > int.MaxValue)
            return 32;

        return diff == 0 ? 0 : (int)Mathf.Log(diff, 2) + 1;
    }

    int GetBitCount_Debruijn(int min, int max) {
        Assert.IsTrue(max > min);
        long diff = max - (long)min;
        Assert.IsTrue(diff < float.MaxValue);

        if (diff > int.MaxValue)
            return 32;

        return diff == 0 ? 0 : Log2_DeBruijn((int)diff) + 1;
    }

    private readonly int[] _multiplyDeBruijnBitPosition = {
        0,  9,  1, 10, 13, 21,  2, 29, 11, 14, 16, 18, 22, 25,  3, 30,
        8, 12, 20, 28, 15, 17, 24,  7, 19, 27, 23,  6, 26,  5,  4, 31
    };

    int Log2_DeBruijn(int v) {
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;

        return _multiplyDeBruijnBitPosition[(uint)(v * 0x07C4ACDDU) >> 27];
    }

}
