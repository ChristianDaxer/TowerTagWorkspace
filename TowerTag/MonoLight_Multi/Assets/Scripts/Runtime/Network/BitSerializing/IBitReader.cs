public interface IBitReader {
    // reset internal state
    void Reset();

    // Read a number of bits from the stream and returns them in a word buffer (uint) (from left to right, aligned at the rightest bit (2^0))
    uint ReadBits(int bits);

    // returns number of bits left in the stream
    // Attention: this vale also contains the empty bits flushed at the end of the stream to resize it to a multiple of four
    int GetBitsRemaining();

    // returns the original data stream
    byte[] GetData();

    // sets data to read & resets internal state
    void SetData(byte[] data);

    // creates deep copy of this BitReader instance
    IBitReader Clone();
}