using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class TestBitSerializerSimpleWithBitWriterNonAllocBuffer {

	[Test]
	public void TestBitSerializerSimpleWithBitWriterNonAllocBufferSimplePasses()
    {
        // Use the Assert class to test conditions.
        BitSerializerSimple writeStream = new BitSerializerSimple(new BitWriterNoAlloc(4000));

        int elementCount = 100;
        int[] intValues = new int[elementCount];
        bool[] boolValues = new bool[elementCount];
        float[] floatValues = new float[elementCount];

        for (int i = 0; i < elementCount; i++)
        {
            intValues[i] = i;
            boolValues[i] = (i % 2) == 1;
            floatValues[i] = ((float)i) / elementCount;

            writeStream.WriteBool(boolValues[i]);
            writeStream.WriteInt(intValues[i], 0, elementCount - 1);
            writeStream.WriteFloat(floatValues[i], 0, 1, (1f/elementCount)*.5f);
        }

        BitSerializerSimple readStream = new BitSerializerSimple(new BitReaderNoAlloc(writeStream.GetData()));

        for (int i = 0; i < elementCount; i++)
        {
            bool b = readStream.ReadBool();
            Assert.IsTrue(boolValues[i] == b, "Bool value (" + i + ") is wrong! o(" + boolValues[i] + ") -> n(" + b +")");

            int m = readStream.ReadInt(0, elementCount - 1);
            Assert.IsTrue(intValues[i] == m, "Int value (" + i + ") is wrong! o(" + intValues[i] + ") -> n(" + m + ")");

            float f = readStream.ReadFloat(0, 1, (1f / elementCount) * .5f);
            Assert.IsTrue(floatValues[i] == f, "Float value (" + i + ") is wrong! o(" + floatValues[i] + ") -> n(" + f + ")");
        }
    }

   

    //// A UnityTest behaves like a coroutine in PlayMode
    //// and allows you to yield null to skip a frame in EditMode
    //[UnityTest]
    //public IEnumerator TestBitSerializerSimpleWithBitWriterNonAllocBufferWithEnumeratorPasses() {
    //	// Use the Assert class to test conditions.
    //	// yield to skip a frame
    //	yield return null;
    //}
}
