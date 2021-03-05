using System;
using System.Text;

public static class StringEncoder {
    public static string EncodeString(string text) {
        byte[] bytesToEncode = Encoding.UTF8.GetBytes(text);
        return Convert.ToBase64String(bytesToEncode);
    }

    public static string DecodeString(string text) {
        byte[] decodedBytes = Convert.FromBase64String(text);
        return Encoding.UTF8.GetString(decodedBytes);
    }
}