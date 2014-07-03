﻿using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NtImageProcessor.MetaData.Misc;
using NtImageProcessor.MetaData.Structure;

namespace NtImageProcessorTest.MetaData.Misc
{
    [TestClass]
    public class UtilTest
    {
        public static Dictionary<UInt32, byte[]> IntByteData = new Dictionary<UInt32, byte[]>()
        {
            // expected array is in Big endian.
            { 0, new byte[]{0,0,0,0}},
            { 1, new byte[]{0,0,0,1}},
            { 0xFF, new byte[]{0,0,0,0xFF}},
            { 0x1FF, new byte[]{0,0,1,0xFF}},
            { 0xFFFF, new byte[]{0,0,0xFF, 0xff}},
            { 0x1FFFFFF, new byte[]{1,0xff, 0xff, 0xff}},
            { 0xFFFFFFFF, new byte[]{0xFF, 0xff, 0xff,0xff}},
        };

        [TestMethod]
        public void IntToByte()
        {
            foreach (UInt32 key in IntByteData.Keys)
            {
                for (int length = 0; length < 6; length++)
                {
                    if (length == 0 || length == 5)
                    {
                        // Invalid length.
                        Assert.ThrowsException<InvalidCastException>(() =>
                        {
                            var e = IntByteData[key];
                            var a = Util.ToByte(key, length, Definitions.Endian.Big);
                        }, "");
                        continue;
                    }

                    UInt64 maxNum = (UInt64)Math.Pow(2, (length * 8));
                    if (key >= maxNum)
                    {
                        Assert.ThrowsException<OverflowException>(() =>
                        {
                            var a = Util.ToByte(key, length, Definitions.Endian.Big);
                            a = Util.ToByte(key, length, Definitions.Endian.Little);
                        }, "Overflow exception in big endian. length: " + length);
                        Assert.ThrowsException<OverflowException>(() =>
                        {
                            var a = Util.ToByte(key, length, Definitions.Endian.Little);
                        }, "Overflow exception in little endian. length: " + length);
                        continue;
                    }

                    var expected_big = TestUtil.GetLastElements(IntByteData[key], length);
                    var actual_big = Util.ToByte(key, length, Definitions.Endian.Big);
                    TestUtil.AreEqual(expected_big, actual_big, "Big endian Int->Byte test.");

                    var expected_Little = TestUtil.GetLastElements(IntByteData[key], length);
                    Array.Reverse(expected_Little);
                    var actual_little = Util.ToByte(key, length, Definitions.Endian.Little);
                    TestUtil.AreEqual(expected_Little, actual_little, "Little endian Int->Byte test.");

                }
            }
        }

        public static Dictionary<UInt32, byte[]> IntByteData2 = new Dictionary<UInt32, byte[]>()
        {
            // big endian.
            {0, new byte[]{0}},
            {1, new byte[]{1}},
            {0xFF, new byte[]{0, 0xFF}},
            {0xFF01, new byte[] {0xFF, 1}},
            {0xFFFF, new byte[] {0,0,0xFF, 0xFF}},
            {0xFFFF0000, new byte[] {0xFF, 0xFF, 0,0}},
        };

        [TestMethod]
        public void ByteToInt_ShortArray()
        {
            // upto 4 bytes array
            foreach (UInt32 key in IntByteData2.Keys)
            {      
                var Array_Big = new byte[IntByteData2[key].Length];
                IntByteData2[key].CopyTo(Array_Big, 0);
                var Array_Little = new byte[IntByteData2[key].Length];
                IntByteData2[key].CopyTo(Array_Little, 0);

                Array.Reverse(Array_Little);

                Assert.AreEqual(key, Util.GetUIntValue(Array_Little, 0, Array_Little.Length, Definitions.Endian.Little));
                Assert.AreEqual(key, Util.GetUIntValue(Array_Big, 0, Array_Big.Length, Definitions.Endian.Big));

                Assert.ThrowsException<InvalidCastException>(() =>
                {
                    var a = Util.GetUIntValue(Array_Little, 0, 0, Definitions.Endian.Little);
                }, "Invalid cast exception in Big endian. key: " + key);
                Assert.ThrowsException<InvalidCastException>(() =>
                {
                    var a = Util.GetUIntValue(Array_Big, 0, 0, Definitions.Endian.Big);
                }, "Invalid cast exception in Little endian. key: " + key);

                if (Array_Big.Length >= 4)
                {
                    Assert.ThrowsException<InvalidCastException>(() =>
                    {
                        var a = Util.GetUIntValue(Array_Little, 0, Array_Big.Length + 1, Definitions.Endian.Big);
                    }, "InvalidCastException in Big endian. key: " + key);
                    Assert.ThrowsException<InvalidCastException>(() =>
                    {
                        var a = Util.GetUIntValue(Array_Little, 0, Array_Big.Length + 1, Definitions.Endian.Little);
                    }, "InvalidCastException in Little endian. key: " + key);
                }
                else
                {
                    Assert.ThrowsException<IndexOutOfRangeException>(() =>
                    {
                        var a = Util.GetUIntValue(Array_Little, 0, Array_Big.Length + 1, Definitions.Endian.Big);
                    }, "IIndexOutOfRangeException in Big endian. key: " + key);

                    Assert.ThrowsException<IndexOutOfRangeException>(() =>
                    {
                        var a = Util.GetUIntValue(Array_Little, 0, Array_Little.Length + 1, Definitions.Endian.Little);
                    }, "IIndexOutOfRangeException in Little endian. key: " + key);
                }
            }
        }

        [TestMethod]
        public void ByteToInt_LongArray()
        {
            var array1 = new byte[] { 0, 0, 0, 1, 2, 3, 0, 0, 0 };
            Assert.AreEqual((UInt32)0, Util.GetUIntValue(array1, 0, 1, Definitions.Endian.Little));
            Assert.AreEqual((UInt32)0, Util.GetUIntValue(array1, 0, 1, Definitions.Endian.Big));
            Assert.AreEqual((UInt32)1, Util.GetUIntValue(array1, 3, 1, Definitions.Endian.Big));
            Assert.AreEqual((UInt32)1, Util.GetUIntValue(array1, 3, 1, Definitions.Endian.Little));
            Assert.AreEqual((UInt32)0x102, Util.GetUIntValue(array1, 3, 2, Definitions.Endian.Big));
            Assert.AreEqual((UInt32)0x201, Util.GetUIntValue(array1, 3, 2, Definitions.Endian.Little));
            Assert.AreEqual((UInt32)0x10203, Util.GetUIntValue(array1, 3, 3, Definitions.Endian.Big));
            Assert.AreEqual((UInt32)0x30201, Util.GetUIntValue(array1, 3, 3, Definitions.Endian.Little));
        }
    }
}
