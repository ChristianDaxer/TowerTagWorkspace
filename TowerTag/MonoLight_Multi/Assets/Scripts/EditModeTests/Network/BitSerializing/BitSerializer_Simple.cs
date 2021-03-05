using UnityEngine;
using UnityEngine.Assertions;

public class BitSerializerSimple {
    // bool, float, int

    private bool IsReading { get; }

    private readonly IBitReader _reader;
    private readonly IBitWriter _writer;

    // create writing stream
    public BitSerializerSimple(IBitWriter writer) {
        IsReading = false;
        _writer = writer;
    }

    // create reading stream
    public BitSerializerSimple(IBitReader reader) {
        IsReading = true;
        _reader = reader;
    }

    // internal function to calculate number of bits needed to represent all integer values in range
    private static int GetBitCount(int min, int max) {
        Assert.IsTrue(max > min);
        long diff = max - (long)min;

        if (diff > int.MaxValue)
            return 32;

        return (diff == 0) ? 0 : HelperFunctions.Log2_DeBruijn((int)diff) + 1;
    }

    // returns: if isWriting    : the written (and flushed) data stream
    //          if isReading    : the original data stream as byte array
    public byte[] GetData() {
        if (IsReading && _reader != null) {
            return _reader.GetData();
        }

        if (!IsReading && _writer != null) {
            return _writer.GetData();
        }
        return null;
    }

    // Serialize Functions

    // bool
    public void WriteBool(bool value) {
        _writer.WriteBits((value ? 1u : 0u), 1);
    }

    public bool ReadBool() {
        return _reader.ReadBits(1) != 0;
    }

    // int
    public void WriteInt(int value, int min, int max) {
        Assert.IsTrue(min <= max);
        Assert.IsTrue(value >= min);
        Assert.IsTrue(value <= max);

        int numBits = GetBitCount(min, max);

        Assert.IsTrue(numBits >= 0 && numBits <= 32);

        _writer.WriteBits((uint)(value - min), numBits);
    }

    public int ReadInt(int min, int max) {
        Assert.IsTrue(min <= max);

        int numBits = GetBitCount(min, max);

        Assert.IsTrue(numBits >= 0 && numBits <= 32);

        uint tmp = _reader.ReadBits(numBits);
        return (int)tmp + min;
    }

    // float
    public void WriteFloat(float value, float min, float max, float resolutionStep) {
        Assert.IsTrue(min <= max);
        Assert.IsTrue(resolutionStep > 0);

        float distance = max - min;
        int maxValue = Mathf.CeilToInt(distance * (1f / resolutionStep));
        int numBits = GetBitCount(0, maxValue);

        Assert.IsTrue(value >= min && value <= max);

        float normalizedValue = Mathf.Clamp01((value - min) / distance);
        uint v = (uint)Mathf.FloorToInt(normalizedValue * maxValue + 0.5f);
        _writer.WriteBits(v, numBits);
    }

    public float ReadFloat(float min, float max, float resolutionStep) {
        Assert.IsTrue(min <= max);
        Assert.IsTrue(resolutionStep > 0);

        float distance = max - min;
        int maxValue = Mathf.CeilToInt(distance * (1f / resolutionStep));
        int numBits = GetBitCount(0, maxValue);

        float normalizedValue = _reader.ReadBits(numBits) / ((float)maxValue);
        return normalizedValue * distance + min;
    }
}
