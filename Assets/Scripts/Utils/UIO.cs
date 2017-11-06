using UnityEngine;
using System.Collections;
using System;
using System.IO;
namespace Lexmou.Utils
{
    public class UIO
    {
        public byte[] ConvertFloatToByte(float[] floatArr)
        {
            byte[] byteArr = new byte[floatArr.Length * 4];
            Buffer.BlockCopy(floatArr,0,byteArr,0,byteArr.Length);
            return byteArr;
        }

        public float[] ConvertByteToFloat(byte[] byteArr)
        {
            float[] floatArr = new float[byteArr.Length / 4];
            Buffer.BlockCopy(byteArr, 0, floatArr, 0, byteArr.Length);
            return floatArr;
        }

        public void SaveFloatArray(string path, float[] floatArr)
        {
            File.WriteAllBytes(path, ConvertFloatToByte(floatArr));
        }


        public float[] LoadFloatArray(string path)
        {
            return ConvertByteToFloat(File.ReadAllBytes(path));
        }

    }
}
