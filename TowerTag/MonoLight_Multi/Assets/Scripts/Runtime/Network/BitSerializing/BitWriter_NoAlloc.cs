using UnityEngine;
using UnityEngine.Assertions;

// Writes bits in a byte Array (use BitSerializer & BitReader/BitReader_NoAlloc to read values back out)
// this class has the same functionality as the normal BitWriter but you can reuse it without allocating/or freeing memory on reset
// is also optimized for GC & Performance (Block copy)
public class BitWriterNoAlloc : IBitWriter
{
    // stream data
    private readonly byte[] _data;
    // index of the next byte to write in data byte array
    private int _dataWriteIndex;

    // word buffer to write to (has double size of our word size -> word size 32 bit
    private ulong _buffer;

    // index of next bit to write to the buffer
    private int _bitIndex;

    public BitWriterNoAlloc(int numBytes)
    {
        _buffer = 0;
        _bitIndex = 0;

        Assert.IsTrue(numBytes % 4 == 0);

        _data = new byte[numBytes];
        _dataWriteIndex = 0;
    }

    public BitWriterNoAlloc(byte[] buffer)
    {
        _buffer = 0;
        _bitIndex = 0;

        Assert.IsNotNull(buffer);
        Assert.IsTrue(buffer.Length % 4 == 0);

        _data = buffer;
        _dataWriteIndex = 0;
    }

    public void Reset()
    {
        _dataWriteIndex = 0;
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
            ulong tmp = (((1UL) << bits) - 1) & value;

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

        if (_data.Length < _dataWriteIndex + 4) {
            Debug.LogError("BitWriter_NoAlloc.FlushCurrentBuffer: data buffer is too small to flush current word to it!");
            return;
        }

        //// copy the upper half of the 64 bit buffer to the byte array
        //// [11111111 | 11100000] -> [11111111]
        var tmp = (uint)(_buffer >> 32);
        // if is little endian -> switch bytes in word array (we want the bits align from left to right in the stream)
        if (System.BitConverter.IsLittleEndian)
        {
            for (var i = 3; i >= 0; i--)
            {
                System.Buffer.SetByte(_data, _dataWriteIndex, (byte)(tmp >> i * 8));
                _dataWriteIndex++;
            }
        }
        else
        {
            for (var i = 0; i < 4; i++)
            {
                System.Buffer.SetByte(_data, _dataWriteIndex, (byte)(tmp >> i * 8));
                _dataWriteIndex++;
            }
        }

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

    // returns the stream data (if you call this the remaining bits in the buffer will be flushed automatically)
    public byte[] GetData()
    {
        FlushBits();

        //Assert.IsTrue(_bitIndex == 0, "getData: " + _bitIndex);
        //Assert.IsTrue(_buffer == 0, "Buffer: " + _buffer);
        Assert.IsTrue(_bitIndex == 0);
        Assert.IsTrue(_buffer == 0);

        Assert.IsTrue(_data != null);

        if (_data == null) {
            Debug.LogError("BitWriter_NoAlloc.GetData: Could not fetch data (data buffer is not initialized (is null))!");
            return null;
        }

        if (_data.Length < _dataWriteIndex) {
            Debug.LogError("BitWriter_NoAlloc.GetData: Could not fetch data (data buffer is smaller then bytes written (did you forget to reset Buffer after last write?");
            return null;
        }

        var b = new byte[_dataWriteIndex];
        System.Buffer.BlockCopy(_data, 0, b, 0, _dataWriteIndex);
        return b;
    }
}
