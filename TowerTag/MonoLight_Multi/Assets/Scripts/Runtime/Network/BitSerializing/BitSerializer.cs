using System;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Class to read or write bitwise to a byte "Stream"
///  Attention:  - this class can only be writing OR reading at once
///                  - if you want a new BitSerializer to write to use the BitSerializer(IBitWriter) constructor
///                  - if you want a new BitSerializer to read from use the BitSerializer(IBitReader) constructor, with the stream data to read from as byte array (this should be written with another instance of this Bit serializer)
///              - you can use the unified Serialize functions to read & write values of simple member variables: Serialize(memberVar, ...), does not work on properties (use tmp-Variables or the ReadType/WriteType-Functions instead)
///              - you also can use the simple readType/writeType functions to read and write (you can mix both kind of functions in one stream but can't mix it on a single Variable)
///                  - OK:   Serialize(stream){
///                              Serialize(memberA, ...);
///                              if(stream.isReading){
///                                  memberB = ReadInt(..);}
///                              else{
///                                  WriteInt(memberB, ...);}
///
///                  - !!! false usage (don't do this) !!!
///                         Serialize(stream){
///                              Serialize(memberA, ...);
///                              Serialize(ref memberB, ...);
///                              if(stream.isReading){
///                                  memberB = ReadInt(..);}
/// </summary>
public class BitSerializer {
    public bool IsReading { get; }
    public bool IsWriting => !IsReading;

    private int _serverTimestampOfWrittenData;

    private readonly IBitReader _reader;
    private readonly IBitWriter _writer;

    // 1/sqrt(2) for Quaternion Compression
    private const float OneDividedSqrtOfTwo = 1f / 1.414214f;
    private const float TwoDividedSqrtOfTwo = 2f / 1.414214f;

    // create writing stream
    public BitSerializer(IBitWriter writer) {
        IsReading = false;
        _writer = writer;
    }

    // create reading stream
    public BitSerializer(IBitReader reader) {
        IsReading = true;
        _reader = reader;
    }

    public BitSerializer Clone() {
        if (IsReading) {
            if (_reader == null) {
                Debug.LogError("Cannot clone bit serialize: reader is null");
                return null;
            }

            return new BitSerializer(_reader.Clone()) {
                _serverTimestampOfWrittenData = _serverTimestampOfWrittenData
            };
        }

        Debug.LogError("BitSerializer.Clone: clone is not implemented for writing Streams (yet)");

        return null;
    }

    // bool
    public bool Serialize(ref bool value) {
        if (IsReading) {
            if (_reader.GetBitsRemaining() < 1)
                return false;

            value = _reader.ReadBits(1) != 0;

            return true;
        }

        _writer.WriteBits(value ? 1u : 0u, 1);
        return true;
    }

    public void WriteBool(bool value) {
        _writer.WriteBits(value ? 1u : 0u, 1);
    }

    public bool ReadBool() {
        return _reader.ReadBits(1) != 0;
    }


    // byte
    // numBits: number of bits to read/write from/into this byte
    public bool Serialize(ref byte value, int numBits) {
        Assert.IsTrue(numBits >= 0 && numBits <= 8, "BitSerializer.Serialize byte: NumBits not between [0..8]!");

        if (IsReading) {
            if (_reader.GetBitsRemaining() < numBits)
                return false;

            uint tmp = _reader.ReadBits(numBits);
            value = (byte) tmp;

            return true;
        }

        _writer.WriteBits(value, numBits);
        return true;
    }

    public void WriteByte(byte value, int numBits) {
        Assert.IsTrue(numBits >= 0 && numBits <= 8, "BitSerializer.WriteByte: NumBits not between [0..8]!");
        _writer.WriteBits(value, numBits);
    }

    public byte ReadByte(int numBits) {
        Assert.IsTrue(numBits >= 0 && numBits <= 8, "BitSerializer.ReadByte: NumBits not between [0..8]!");
        Assert.IsTrue(_reader.GetBitsRemaining() >= numBits, "BitSerializer.ReadByte: NumBits > remaining bits!");
        return (byte) _reader.ReadBits(numBits);
    }

    // byte array
    // max. Length of a serializable byte array (so we can determine the number of bits needed to send the actual size of the array before the actual array data)
    public bool Serialize(ref byte[] value, int maxLength) {
        int counterBits = GetBitCount(0, maxLength);

        if (IsReading) {
            if (_reader.GetBitsRemaining() < counterBits)
                return false;

            var numBytes = (int) _reader.ReadBits(counterBits);
            int numBits = numBytes * 8;

            if (_reader.GetBitsRemaining() < numBits)
                return false;

            value = new byte[numBytes];
            for (int i = 0; i < numBytes; i++) {
                uint tmp = _reader.ReadBits(8);
                value[i] = (byte) tmp;
            }

            return true;
        }

        Assert.IsTrue(value != null && value.Length <= maxLength,
            "BitSerializer.Serialize write byte[]: value is null or length > maxLength!");
        _writer.WriteBits((uint) value.Length, counterBits);

        for (var i = 0; i < value.Length; i++) {
            _writer.WriteBits(value[i], 8);
        }

        return true;
    }

    // int
    // min/max: smallest/biggest value we have to serialize
    public bool Serialize(ref int value, int min, int max) {
        Assert.IsTrue(min <= max, "BitSerializer.Serialize int: min > max!");

        int numBits = GetBitCount(min, max);

        // Debug.Log("Serialize int: " + value + "[" + min + ", " + max + "] -> " + numBits);

        Assert.IsTrue(numBits >= 0 && numBits <= 32, "BitSerializer.Serialize int: numBits not in range [0..32]!");

        if (IsReading) {
            if (_reader.GetBitsRemaining() < numBits)
                return false;

            uint tmp = _reader.ReadBits(numBits);
            value = ((int) tmp) + min;

            return true;
        }

        Assert.IsTrue(value >= min,
            "BitSerializer.Serialize int (write): value(" + value + ") < min(" + min + ")! -> ");
        Assert.IsTrue(value <= max, "BitSerializer.Serialize int (write): value > max!");
        _writer.WriteBits((uint) (value - min), numBits);
        return true;
    }

    public bool SerializeUncompressed(ref int value) {
        const int min = int.MinValue;
        const int numBits = 32;

        if (IsReading) {
            if (_reader.GetBitsRemaining() < numBits)
                return false;

            uint tmp = _reader.ReadBits(numBits);
            value = ((int) tmp) + min;

            return true;
        }

        _writer.WriteBits((uint) (value - min), numBits);
        return true;
    }

    public void WriteInt(int value, int min, int max) {
        if (min > max)
            throw new Exception($"BitSerializer.WriteFloat: min ({min}) > max ({max})!");
        if (value < min)
            throw new Exception($"BitSerializer.WriteInt: {value} < {min}!");
        if (value > max)
            throw new Exception($"BitSerializer.WriteInt: {value} > {max}!");

        int numBits = GetBitCount(min, max);

        if(numBits < 0 || numBits > 32)
            throw new Exception($"BitSerializer.WriteInt: {numBits} not in range [0..32]!");

        _writer.WriteBits((uint) (value - min), numBits);
    }

    public int ReadInt(int min, int max) {
        Assert.IsTrue(min <= max, "BitSerializer.ReadInt: min > max!");

        int numBits = GetBitCount(min, max);

        Assert.IsTrue(numBits >= 0 && numBits <= 32, "BitSerializer.ReadInt: numBits not in range [0..32]!");

        uint tmp = _reader.ReadBits(numBits);
        return (int) tmp + min;
    }


    // int array
    // min/max: smallest/biggest value we have to serialize
    // max. Length of a serializable int array (so we can determine the number of bits needed to send the actual size of the array before the actual array data)
    public bool Serialize(ref int[] value, int min, int max, int maxLength) {
        Assert.IsTrue(min <= max, "BitSerializer.Serialize int[]: min > max!");

        int numBits = GetBitCount(0, maxLength);
        uint length;

        if (IsReading) {
            length = _reader.ReadBits(numBits);
            Assert.IsTrue(length <= maxLength, "BitSerializer.Serialize int[] (read): length > maxLength!");
            value = new int[length];
        }
        else {
            Assert.IsTrue(value != null && value.Length <= maxLength,
                "BitSerializer.Serialize int[] (write): value is null or value.length > maxLength!");
            length = (uint) value.Length;
            _writer.WriteBits(length, numBits);
        }

        for (var i = 0; i < length; i++) {
            if (!Serialize(ref value[i], min, max))
                return false;
        }

        return true;
    }

    // float
    private bool Serialize(ref float value) {
        const int numBits = 32;
        if (IsReading) {
            if (_reader.GetBitsRemaining() < numBits)
                return false;

            uint tmp = _reader.ReadBits(numBits);
            value = BitConverter.ToSingle(BitConverter.GetBytes(tmp), 0);

            return true;
        }

        _writer.WriteBits(BitConverter.ToUInt32(BitConverter.GetBytes(value), 0), numBits);
        return true;
    }

    public void WriteFloat(float value, float min, float max, float resolutionStep) {
        if (min > max)
            throw new Exception($"BitSerializer.WriteFloat: min ({min}) > max ({max})!");
        if (resolutionStep <= 0)
            throw new Exception($"Resolution step 0");

        float distance = max - min;
        int maxValue = Mathf.CeilToInt(distance * (1f / resolutionStep));
        int numBits = GetBitCount(0, maxValue);
        if(value < min || value > max)
            throw new Exception($"BitSerializer.WriteFloat: out of bounds: ${value} not in interval [{min}, {max}]!");

        float normalizedValue = Mathf.Clamp01((value - min) / distance);
        var v = (uint) Mathf.FloorToInt(normalizedValue * maxValue + 0.5f);
        _writer.WriteBits(v, numBits);
    }

    public float ReadFloat(float min, float max, float resolutionStep) {
        if (min > max)
            throw new Exception($"BitSerializer.ReadFloat: min ({min}) > max ({max})!");
        if (resolutionStep <= 0)
            throw new Exception("Resolution step 0");

        float distance = max - min;
        int maxValue = Mathf.CeilToInt(distance * (1f / resolutionStep));
        int numBits = GetBitCount(0, maxValue);

        float normalizedValue = _reader.ReadBits(numBits) / ((float) maxValue);
        return normalizedValue * distance + min;
    }


    // lossy compressed (quantized) float
    // min/max: smallest/biggest value we have to serialize
    // resolution: number of distinguishable values in the range [min..max]
    public bool Serialize(ref float value, float min, float max, float resolutionStep) {
        if (min > max)
            throw new Exception($"BitSerializer.Serialize: min ({min}) > max ({max})!");
        if (resolutionStep <= 0)
            throw new Exception("Resolution step 0");

        float distance = max - min;
        int maxValue = Mathf.CeilToInt(distance * (1f / resolutionStep));
        int numBits = GetBitCount(0, maxValue);

        if (IsReading) {
            if (_reader.GetBitsRemaining() < numBits)
                return false;

            float normalizedValue = _reader.ReadBits(numBits) / ((float) maxValue);
            value = normalizedValue * distance + min;
            return true;
        }
        else {
            Assert.IsTrue(value >= min && value <= max,
                "BitSerializer.Serialize float (write): value < min or value > max!");
            float normalizedValue = Mathf.Clamp01((value - min) / distance);
            var v = (uint) Mathf.FloorToInt(normalizedValue * maxValue + 0.5f);
            _writer.WriteBits(v, numBits);
            return true;
        }
    }

    // Vector3
    public bool Serialize(ref Vector3 value) {
        if (!Serialize(ref value.x))
            return false;

        if (!Serialize(ref value.y))
            return false;

        if (!Serialize(ref value.z))
            return false;

        return true;
    }

    // lossy compressed (quantized) Vector3
    // min/max: smallest/biggest value we have to serialize
    // resolution: number of distinguishable values in the range [min..max]
    public bool Serialize(ref Vector3 value, float min, float max, float resolutionStep) {
        if (!Serialize(ref value.x, min, max, resolutionStep))
            return false;

        if (!Serialize(ref value.y, min, max, resolutionStep))
            return false;

        if (!Serialize(ref value.z, min, max, resolutionStep))
            return false;

        return true;
    }

    // Quaternion
    public bool Serialize(ref Quaternion value) {
        if (!Serialize(ref value.x))
            return false;

        if (!Serialize(ref value.y))
            return false;

        if (!Serialize(ref value.z))
            return false;

        if (!Serialize(ref value.w))
            return false;

        return true;
    }

    //// lossy compressed (quantized) Quaternion (with smallest three)
    // resulting compressed value is:   either 3 bits (one component is one and the rest zero)
    //                                  or (3 + 3 * bitsPerElement) bits for an arbitrary compressed Quaternion
    public bool Serialize(ref Quaternion value, int bitsPerElement) {
        Assert.IsTrue(bitsPerElement > 0 && bitsPerElement <= 32,
            "BitSerializer.Serialize Quaternion: bitsPerElement <= 0 or bitsPerElement > 32!");
        var maxValueWithGivenBits = (float) ((1 << bitsPerElement) - 1);

        if (IsReading) {
            var largestIndex = (int) _reader.ReadBits(3);
            Vector4 v = Vector4.zero;

            // 2) check if only one value (largest Index) was sent
            if ((largestIndex & (1 << 2)) != 0) {
                largestIndex &= (1 << 2) - 1;
                v[largestIndex] = 1;
                value = new Quaternion(v.x, v.y, v.z, v.w);
                return true;
            }

            // read in the values from stream
            Vector3 smallestThree = Vector3.zero;
            for (var i = 0; i < 3; i++) {
                smallestThree[i] =
                    _reader.ReadBits(bitsPerElement) * TwoDividedSqrtOfTwo / maxValueWithGivenBits -
                    OneDividedSqrtOfTwo;
            }

            v[largestIndex] = Mathf.Sqrt(1 - (smallestThree.x * smallestThree.x) - (smallestThree.y * smallestThree.y) -
                                         (smallestThree.z * smallestThree.z));

            var k = 0;
            for (var i = 0; i < 3; i++) {
                if (largestIndex == i)
                    k++;

                v[k + i] = smallestThree[i];
            }

            v.Normalize();
            value = new Quaternion(v.x, v.y, v.z, v.w);

            return true;
        }
        else {
            // smallest three:
            var absValues = new Vector4(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z),
                Mathf.Abs(value.w));

            // 1) find largest (absolute) value
            float largestValue = absValues.x;
            var largestIndex = 0;
            float largestSign = Mathf.Sign(value.x);

            for (var i = 1; i < 4; i++) {
                if (absValues[i] > largestValue) {
                    largestValue = absValues[i];
                    largestIndex = i;
                    largestSign = Mathf.Sign(value[i]);
                }
            }

            // 2) check if we have to send only one value (largest Index)
            if (Mathf.Approximately(largestValue, 1)) {
                _writer.WriteBits((uint) (largestIndex | (1 << 2)), 3);
                return true;
            }

            // 3) set smallest three
            Vector3 smallestThreeNormalized = Vector3.zero;
            var k = 0;
            for (var i = 0; i < 3; i++) {
                if (largestIndex == i)
                    k++;

                // normalize value in [-1/sqrt(2), 1/sqrt(2)]
                smallestThreeNormalized[i] = ((value[i + k] * largestSign) + OneDividedSqrtOfTwo) / TwoDividedSqrtOfTwo;
            }

            Assert.IsTrue(smallestThreeNormalized.x >= 0 && smallestThreeNormalized.x <= 1,
                "BitSerializer.Serialize Quaternion (write): smallestThreeNormalized.x < 0 or > 1!");
            Assert.IsTrue(smallestThreeNormalized.y >= 0 && smallestThreeNormalized.y <= 1,
                "BitSerializer.Serialize Quaternion (write): smallestThreeNormalized.y < 0 or > 1!");
            Assert.IsTrue(smallestThreeNormalized.z >= 0 && smallestThreeNormalized.z <= 1,
                "BitSerializer.Serialize Quaternion (write): smallestThreeNormalized.z < 0 or > 1!");

            // 4) send values
            _writer.WriteBits((uint) largestIndex, 3);
            _writer.WriteBits((uint) Mathf.FloorToInt(smallestThreeNormalized.x * maxValueWithGivenBits + 0.5f),
                bitsPerElement);
            _writer.WriteBits((uint) Mathf.FloorToInt(smallestThreeNormalized.y * maxValueWithGivenBits + 0.5f),
                bitsPerElement);
            _writer.WriteBits((uint) Mathf.FloorToInt(smallestThreeNormalized.z * maxValueWithGivenBits + 0.5f),
                bitsPerElement);
        }

        return true;
    }

    // string
    // max. Length of a serializable string (so we can determine the number of bits needed to send the actual size of the string before the actual string data)
    public bool Serialize(ref string value, int maxLength) {
        int characterCountBits = GetBitCount(0, maxLength);
        if (IsWriting) {
            Assert.IsTrue(value != null && value.Length <= maxLength,
                "BitSerializer.Serialize string (write): value is null or value.Length > maxLength!");
            _writer.WriteBits((uint) value.Length, characterCountBits);

            char[] c = value.ToCharArray();
            for (var i = 0; i < value.Length; i++) {
                _writer.WriteBits(c[i], 16);
            }

            return true;
        }
        else {
            Assert.IsTrue(_reader.GetBitsRemaining() > characterCountBits,
                "BitSerializer.Serialize Quaternion (read): characterCountBits >= remainingBits");
            uint count = _reader.ReadBits(characterCountBits);
            Assert.IsTrue(_reader.GetBitsRemaining() >= count * 16,
                "BitSerializer.Serialize Quaternion (read): character count * 16 > remainingBits");

            var c = new char[count];
            for (var i = 0; i < count; i++) {
                c[i] = (char) _reader.ReadBits(16);
            }

            value = new string(c);
            return true;
        }
    }

    private static int GetBitCount(int min, int max) {
        Assert.IsTrue(max > min, "BitSerializer.GetBitCount: min <= max!");
        long diff = max - (long) min;

        if (diff > int.MaxValue)
            return 32;

        return diff == 0 ? 0 : HelperFunctions.Log2_DeBruijn((int) diff) + 1;
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

    public void SetData(byte[] data) {
        if (IsReading) {
            _reader?.SetData(data);
        }
        else {
            Debug.LogWarning("BitSerializer.SetData: This is an writing Stream, you can't set new data!");
        }
    }

    public void Reset() {
        if (IsReading && _reader != null) {
            _reader.Reset();
        }
        else if (!IsReading) {
            _writer?.Reset();
        }
    }
}