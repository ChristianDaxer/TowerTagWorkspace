public interface IBitWriter {
    // reset internal state
    void Reset();

    // write bits to buffer
    void WriteBits(uint value, int bits);

    // returns the stream data (if you call this the remaining bits in the buffer will be flushed automatically)
    byte[] GetData();
}