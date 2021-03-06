﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polenter.Serialization.Core;

namespace Polenter.Serialization
{
    /// <summary>
    /// It is possible to shorten the stream or replace some bytes and no exception is thrown.
    /// To fix this issue, it would be necessary to implement a checking, maybe a check sum?
    /// But it increases the overhead.
    /// </summary>
    [TestClass]
    public class CorruptedSourceStreamTests
    {
        [TestMethod]
        [ExpectedException(typeof(DeserializingException))]
        public void CorruptedXmlStreamTest()
        {
            var myArray = new[] {"ala", "ma", null, "kota"};
            serialize(myArray, new SharpSerializer(), replaceSomeBytesInData);
        }

        /// <summary>
        /// ArgumentOutOfRangeException occures
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(DeserializingException))]
        public void CorruptedSizeOptimizedBinaryStreamTest()
        {
            var myArray = new[] { "ala", "ma", null, "kota" };
            serialize(myArray, new SharpSerializer(true), replaceSomeBytesInData);
        }

        /// <summary>
        /// No exception!
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(DeserializingException))]
        public void CorruptedBurstBinaryStreamTest()
        {
            var myArray = new[] { "ala", "ma", null, "kota" };
            var settings = new SharpSerializerBinarySettings(BinarySerializationMode.Burst);
            serialize(myArray, new SharpSerializer(settings), replaceSomeBytesInData);
        }


        [TestMethod]
        [ExpectedException(typeof(DeserializingException))]
        public void TooShortXmlStreamTest()
        {
            var myArray = new[] { "ala", "ma", null, "kota" };
            serialize(myArray, new SharpSerializer(), shortenData);
        }


        [TestMethod]
        [ExpectedException(typeof(DeserializingException))]
        public void TooShortSizeOptimizedBinaryStreamTest()
        {
            var myArray = new[] { "ala", "ma", null, "kota" };
            var settings = new SharpSerializerBinarySettings(BinarySerializationMode.Burst);
            serialize(myArray, new SharpSerializer(settings), shortenData);
        }

        [TestMethod]
        [ExpectedException(typeof(DeserializingException))]
        public void TooShortBurstBinaryStreamTest()
        {
            var myArray = new[] { "ala", "ma", null, "kota" };
            var settings = new SharpSerializerBinarySettings(BinarySerializationMode.Burst);
            serialize(myArray, new SharpSerializer(settings), shortenData);
        }

        private static void serialize(object source, SharpSerializer serializer, Func<byte[], byte[]> dataCallback)
        {
            byte[] data;

            // Serializing
            using (var stream = new MemoryStream())
            {
                // serialize
                serializer.Serialize(source, stream);

                data = stream.ToArray();
            }


            // Modifying data
            if (dataCallback!=null)
            {
                data = dataCallback(data);
            } 

            // Deserializing
            using (var stream = new MemoryStream(data))
            {
                // deserialize
                var result = serializer.Deserialize(stream);

                // it comes never here
            }
        }

        byte[] replaceSomeBytesInData(byte[] data)
        {
            int startIndex = Convert.ToInt32(data.Length*0.7);
            int endindex = Convert.ToInt32(data.Length * 0.9);
            for (int i = startIndex; i <= endindex; i++)
            {
                unchecked
                {
                    data[i] = Convert.ToByte(new Random().Next(255));     
                }
            }
            return data;
        }

        byte[] shortenData(byte[] data)
        {
            var result = new byte[Convert.ToInt32(data.Length * 0.9)];
            using(var stream = new MemoryStream(data))
            {
                stream.Read(result, 0, result.Length);
            }
            return result;
        }
    }
}
