using System;
using UnityEngine.Assertions;

// Reads bits out of a given byte array (which was created with BitSerializer & BitWriter/BitWriter_NoAlloc)
// this class has the same functionality as the normal BitReader but you can reuse it without allocating/or freeing memory on reset
// is also optimized for GC & Performance (Block copy)
public class BitReaderNoAlloc : IBitReader
{
    // stream data
    private byte[] _data;
    // buffer has double size of our word size -> word size 32 bit
    private ulong _buffer;

    // size of this stream in bits
    private int _numBits;

    // size of this stream in bytes
    private int _numBytes;

    // number of bits read from the stream already
    private int _bitsRead;

    // index of the next bit to read from the buffer
    private int _bitIndex;
    // index of the next byte to read from the stream
    private int _byteIndex;
    // if we try to read more bits from the stream than exist, this member returns true
    private bool _overflow;

    public BitReaderNoAlloc(byte[] data)
    {
        Init(data);
    }

    // ctor to allow Clone
    private BitReaderNoAlloc(byte[] data, ulong buffer, int numBits, int numBytes, int bitsRead, int bitIndex, int byteIndex, bool overflow)
    {
        _data = data;
        _buffer = buffer;
        _numBits = numBits;
        _numBytes = numBytes;
        _bitsRead = bitsRead;
        _bitIndex = bitIndex;
        _byteIndex = byteIndex;
        _overflow = overflow;
    }

    // creates deep copy of this BitReader instance
    public IBitReader Clone()
    {
        var clonedData = new byte[_data.Length];
        Buffer.BlockCopy(_data, 0, clonedData, 0, _data.Length);
        return new BitReaderNoAlloc(clonedData, _buffer, _numBits, _numBytes, _bitsRead, _bitIndex, _byteIndex, _overflow);
    }

    private void Init(byte[] data)
    {
        Assert.IsTrue(data != null, "BitReader_NoAlloc.Init: data is null!");

        // IMPORTANT: buffer size must be a multiple of four! (because we read in chunks of 32 bit)
        Assert.IsTrue(data.Length >= 4 && (data.Length % 4) == 0, "BitReader_NoAlloc.Init: data length is not a multiple of 4!");
        //Assert.IsTrue(data.Length >= 4 && (data.Length % 4) == 0, "BitReader: constructor data not valid: " + data.Length);

        // init data to read
        _data = data;
        _numBits = data.Length * 8;
        _numBytes = data.Length;

        // init buffer & indices
        _byteIndex = 0;
        _buffer = 0;
        _bitsRead = 0;
        _bitIndex = 0;

        _overflow = false;

        ReadDataFromStreamToScratchBuffer();
    }
    public void Reset()
    {
        // init buffer & indices
        _byteIndex = 0;
        _buffer = 0;
        _bitsRead = 0;
        _bitIndex = 0;
        _overflow = false;
        ReadDataFromStreamToScratchBuffer();
    }

    // Read a number of bits from the stream and returns them in a word buffer (uint) (from left to right, aligned at the rightest bit (2^0))
    public uint ReadBits(int bits)
    {
        Assert.IsTrue(bits > 0);
        Assert.IsTrue(bits <= 32);
        //Assert.IsTrue(_bitsRead + bits <= _numBits, "Not enough bits to read! (" + _bitsRead + " + " + bits + " <=" + _numBits);
        Assert.IsTrue(_bitsRead + bits <= _numBits);

        // are enough bits left to read?
        if (_bitsRead + bits > _numBits)
        {
            _overflow = true;
            return 0;
        }

        _bitsRead += bits;
        Assert.IsTrue(_bitIndex < 32);

        // enough bits in scratch buffer to read
        // [00000000 | 11110000] (if we want to read 3 bits)
        if (_bitIndex + bits < 32)
        {
            // push bits to the upper half of the buffer so we can return the upper half as word
            // [00000111 | 10000000] (if we want to read 3 bits)
            _buffer <<= bits;
            _bitIndex += bits;
        }
        // if we have not enough data in the buffer to read all requested bits
        //  [00000000 | 11000000] (if we want to read 3 bits)
        else
        {
            //Assert.IsTrue(_byteIndex <= _numBytes, "Error: " + _byteIndex + " < " + _numBytes);
            Assert.IsTrue(_byteIndex <= _numBytes);

            // number of bits left to read in the buffer
            int restBits = (32 - _bitIndex);
            // number of bits we need from the stream to return the requested number of bits
            int remainingBits = bits - restBits;
            _bitIndex = 0;
            // move the rest of the buffer bits to the upper half of the buffer so we can fill the lower half with a new word
            // [00000011 | 00000000]
            _buffer <<= restBits;

            // read new word in the lower half of the buffer
            // [00000011 | 11111111]
            if (ReadDataFromStreamToScratchBuffer())
            {
                // move the remaining bits to the upper half
                // [00000111 | 11111110] (if we want to read 3 bits)
                _buffer <<= remainingBits;
                _bitIndex = remainingBits;
            }
        }

        // return output from upper half of the 64 bit buffer
        // [00000111]
        uint output = (uint)(_buffer >> 32);

        // empty upper half of the buffer
        // [00000000 | 11111110]
        _buffer &= 0xFFFFFFFF;

        return output;
    }

    // read the next word from stream to the buffer
    bool ReadDataFromStreamToScratchBuffer()
    {
        if (_byteIndex + 4 > _numBytes)
        {
            _overflow = true;
            return false;
        }

        //Assert.IsTrue((_byteIndex + 4) <= _numBytes, "Error: " + _byteIndex + " + 4 <= " + _numBytes);
        Assert.IsTrue((_byteIndex + 4) <= _numBytes);
        Assert.IsTrue(_bitIndex == 0);

        // read next 4 bytes from stream into the buffer [00000001 | 00000000] -> [00000001 | 11111111]
        // if is little endian -> switch bytes in word array (the bits in the stream are aligned from left to right)
        if (BitConverter.IsLittleEndian)
        {
            for (int i = 3; i >= 0; i--)
            {
                _buffer |= ((uint)Buffer.GetByte(_data, _byteIndex)) << (i * 8);
                _byteIndex++;
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                _buffer |= ((uint)Buffer.GetByte(_data, _byteIndex)) << (i * 8);
                _byteIndex++;
            }
        }
        return true;
    }

    // returns number of bits left in the stream
    // Attention: this vale also contains the empty bits flushed at the end of the stream to resize it to a multiple of four
    public int GetBitsRemaining()
    {
        return _numBits - _bitsRead;
    }

    // returns the original data stream
    public byte[] GetData()
    {
        return _data;
    }

    //sets new data to read from (resets readIndices)
    public void SetData(byte[] data)
    {
        Init(data);
    }
}
