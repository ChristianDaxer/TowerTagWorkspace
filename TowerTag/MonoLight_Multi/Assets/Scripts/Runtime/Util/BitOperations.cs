public static class BitOperations {
    // 32 bit mask (uint)
    public static uint SetBitInMask(uint bitMask, int bit, bool value) {
        if (bit >= 0 && 32 >= bit) {
            uint maskToWrite = 1u << bit;

            if (value) {
                return bitMask | maskToWrite;
            }

            return bitMask & ~maskToWrite;
        }

        Debug.LogError("BitOperations.SetBitInMask(uint): Operation can not process, " +
                       "the bit to change is not in range of the bitMask!");
        return bitMask;
    }

    public static bool CheckBitInMask(uint bitMask, int bit) {
        if (bit >= 0 && 32 >= bit) {
            uint maskToWrite = 1u << bit;
            return (bitMask & maskToWrite) != 0;
        }

        Debug.LogError("BitOperations.CheckBitInMask(uint): Operation can not process, " +
                       "the bit to change is not in range of the bitMask!");

        return false;
    }

    // 32 bit mask (uint)
    public static byte SetBitInMask(byte bitMask, int bit, bool value) {
        if (bit >= 0 && 8 >= bit) {
            var maskToWrite = (byte) (1u << bit);

            if (value) {
                return (byte) (bitMask | maskToWrite);
            }

            return (byte) (bitMask & ~maskToWrite);
        }

        Debug.LogError("BitOperations.SetBitInMask(uint): Operation can not process, " +
                       "the bit to change is not in range of the bitMask!");
        return bitMask;
    }

    public static bool CheckBitInMask(byte bitMask, int bit) {
        if (bit >= 0 && 8 >= bit) {
            var maskToWrite = (byte) (1u << bit);
            return (bitMask & maskToWrite) != 0;
        }

        Debug.LogError("BitOperations.CheckBitInMask(uint): Operation can not process, " +
                       "the bit to change is not in range of the bitMask!");

        return false;
    }

    // byte array mask
    public static byte[] SetBitInMask(byte[] bitMask, int bit, bool value) {
        if (bit >= 0 && bitMask.Length * 8 > bit) {
            int byteToWrite = bit / 8;
            int bitToWrite = bit % 8;
            var maskToWrite = (byte) (1 << bitToWrite);

            if (value) {
                bitMask[byteToWrite] |= maskToWrite;
            }
            else {
                bitMask[byteToWrite] &= (byte) ~maskToWrite;
            }
        }
        else {
            Debug.LogError("BitOperations.SetBitInMask(byte[]): Operation can not process, " +
                           "the bit to change is not in range of the bitMask!");
        }

        return bitMask;
    }

    public static bool CheckBitInMask(byte[] bitMask, int bit) {
        if (bit >= 0 && bitMask.Length * 8 > bit) {
            int byteToWrite = bit / 8;
            int bitToWrite = bit % 8;
            var maskToWrite = (byte) (1 << bitToWrite);
            return (bitMask[byteToWrite] & maskToWrite) != 0;
        }

        Debug.LogError("BitOperations.CheckBitInMask(byte[]): Operation can not process, " +
                       "the bit to change is not in range of the bitMask!");
        return false;
    }
}