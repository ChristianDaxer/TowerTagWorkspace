using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class TestBitSerializer {

	[Test]
    public void TestBitSerializerwithStrings()
    {
        string s = "Welcome back to earth!";
        BitSerializer write = new BitSerializer(new BitWriterNoAlloc(new byte[1000]));
        write.Serialize(ref s, 100);

        BitSerializer read = new BitSerializer(new BitReaderNoAlloc(write.GetData()));
        string s2 = null;
        read.Serialize(ref s2, 100);

        Assert.IsTrue(s.Equals(s2));
    }

    [Test]
    public void TestBitSerializerwithUnCompressedInt()
    {
        BitSerializer write = new BitSerializer(new BitWriterNoAlloc(new byte[1000]));
        
        // minInt
        TestBitSerializerwithUnCompressedInt_Help(write, int.MinValue);

        // maxInt
        TestBitSerializerwithUnCompressedInt_Help(write, int.MaxValue);

        // -1
        TestBitSerializerwithUnCompressedInt_Help(write, - 1);

        // 0
        TestBitSerializerwithUnCompressedInt_Help(write, 0);

        // 1
        TestBitSerializerwithUnCompressedInt_Help(write, 1);

        for (int i = 0; i < 1000000; i++)
        {
            TestBitSerializerwithUnCompressedInt_Help(write, Random.Range(int.MinValue, int.MaxValue));
        }
    }
    void TestBitSerializerwithUnCompressedInt_Help(BitSerializer write, int originalValue)
    {
        int deserializedValue = -2;
        write.Reset();
        write.SerializeUncompressed(ref originalValue);

        BitSerializer read = new BitSerializer(new BitReaderNoAlloc(write.GetData()));
        read.SerializeUncompressed(ref deserializedValue);

        Assert.IsTrue(originalValue.Equals(deserializedValue));
    }
}
