using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

// Writes bits in a byte Array (use BitSerializer & BitReader/BitReader_NoAlloc to read values back out)
public class BitWriter : IBitWriter
{
    // stream data
    private readonly List<byte> _data = new List<byte>();

    // buffer has double size of our word size -> word size 32 bit
    private ulong _buffer;

    // index of next bit to write to the buffer
    private int _bitIndex;

    public BitWriter()
    {
        _buffer = 0;
        _bitIndex = 0;
    }

    public void Reset()
    {
        _data.Clear();
        _buffer = 0;
        _bitIndex = 0;
    }

    // write bits to scratch (64 bit buffer) and flush data (32 bit) to stream (if buffer contains mor than 32 bits)
    public void WriteBits(uint value, int bits)
    {
        Assert.IsTrue(bits > 0, "Error: BitWriter.WriteBits: bits <= 0");
        Assert.IsTrue(bits <= 32, "Error: BitWriter.WriteBits: bits > 32");

        // only write ones, if the byte contains only zeros -> just increment _bitIndex & written bits index
        if (value != 0)
        {
            // create bitmask ((uint)(((1UL) << bits) - 1)) to cut off not needed bits from the left
            // [11111111] & [00000111] (if we want to write just 3 bits)
            ulong tmp = ((1UL << bits) - 1) & value;

            // move left (32 - bits) + (32 - bitIndex) so the value begins at the bitIndex field
            // write to buffer from left to right
            // [11111100 | 00000000] -> [11111111 | 10000000]  (if we want to write 3 bits)
            _buffer |= tmp << (64 - bits - _bitIndex);
        }

        // increment indices
        _bitIndex += bits;

        // if the upper half of our buffer is filled flush it to the stream
        if (_bitIndex > 31)
        {
            FlushCurrentBuffer();
        }
    }

    // flush current word buffer (4 bytes)
    private void FlushCurrentBuffer()
    {
        // don't flush when there no bits left in the buffer
        if (_bitIndex == 0)
            return;

        // convert the upper half of the 64 bit buffer to a byte array
        // [11111111 | 11100000] -> [11111111]
        byte[] word = System.BitConverter.GetBytes((uint)(_buffer >> 32));

        // if is little endian -> switch bytes in word array (we want the bits align from left to right in the stream)
        if (System.BitConverter.IsLittleEndian)
        {
            word = SwapBytesInArray(word);
        }

        // add bytes to stream
        _data.AddRange(word);
        // move the remaining bits from the lower part to the upper part and clear lower part of the buffer
        // [11111111 | 11100000] -> [11100000 | 00000000]
        _buffer <<= 32;
        // update bitIndex (if we had less than 32 bits in the buffer to flush bitIndex is zero, else bitIndex equals the number of bits remaining in the buffer (aka the index to write nte next bit to))
        _bitIndex = Mathf.Max(_bitIndex - 32, 0);
    }

    // flush not written bits (1 - 4 bytes)
    private void FlushBits()
    {
        // change this later on to save some bits (don't add a full word at the end, just the number of bytes needed to flush the remaining bits)
        // Attention!!! if you change this behaviour, make sure you change the behaviour of the Bit reader accordingly (now the Bit reader needs the size of the data array (byte[]) as multiple of four)
        if(_bitIndex > 0)
            FlushCurrentBuffer();
    }

    // swap the order of the 4 bytes of a word (BigEndian <-> LittleEndian)
    private static byte[] SwapBytesInArray(byte[] word)
    {
        Assert.IsTrue(word != null && word.Length == 4);

        var tmp = new byte[word.Length];
        for (var i = 0; i < word.Length; i++)
        {
            tmp[i] = word[word.Length - 1 - i];
        }
        return tmp;
    }

    // returns the stream data (if you call this the remaining bits in the buffer will be flushed automatically)
    public byte[] GetData()
    {
        FlushBits();

        Assert.IsTrue(_bitIndex == 0, "getData: " + _bitIndex);
        Assert.IsTrue(_buffer == 0, "Buffer: " + _buffer);

        return _data.ToArray();
    }
}
