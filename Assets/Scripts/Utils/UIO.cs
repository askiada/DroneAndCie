using UnityEngine;
using System.Collections;
using System;
using System.IO;
namespace Lexmou.Utils
{
    /**
     * # This is a level 1 header

        ### This is level 3 header #######
     * */
    public static class UIO
    {

        public static string GetCommandLineArguments(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            if(args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == name && args.Length > i + 1)
                    {
                        return args[i + 1];
                    }
                }
            }
            return null;
        }

        public static void CreateDirectory(string path)
        {
            try
            {
                // Determine whether the directory exists.
                if (Directory.Exists(path))
                {
                    return;
                }

                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }

        
        public static byte[] ToBytes<T>(this T[,] array) where T : struct
        {
            var buffer = new byte[array.GetLength(0) * array.GetLength(1) * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))];
            Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
            return buffer;
        }
        public static void FromBytes<T>(this T[,] array, byte[] buffer) where T : struct
        {
            var len = Math.Min(array.GetLength(0) * array.GetLength(1) * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), buffer.Length);
            Buffer.BlockCopy(buffer, 0, array, 0, len);
        }

        public static byte[] ToBytes<T>(this T[] array) where T : struct
        {
            var buffer = new byte[array.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))];
            Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
            return buffer;
        }
        public static void FromBytes<T>(this T[] array, byte[] buffer) where T : struct
        {
            var len = Math.Min(array.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), buffer.Length);
            Buffer.BlockCopy(buffer, 0, array, 0, len);
        }

        /*public static byte[] ConvertFloatToByte(float[] floatArr)
        {
            byte[] byteArr = new byte[floatArr.Length * 4];
            Buffer.BlockCopy(floatArr,0,byteArr,0,byteArr.Length);
            return byteArr;
        }

        public static void ConvertByteToFloat(byte[] byteArr, float[] floatArr)
        {
            floatArr = new float[byteArr.Length / 4];
            Buffer.BlockCopy(byteArr, 0, floatArr, 0, byteArr.Length);
            //return floatArr;
        }*/

        public static void SaveFloatArray(string path, string name, float[,] floatArr)
        {
            CreateDirectory(path);
            File.WriteAllBytes(path + "/" + name, ToBytes(floatArr));
        }


        public static void LoadFloatArray(string path,string name, float[,] floatArr)
        {
            //CreateDirectory(path);
            FromBytes(floatArr, File.ReadAllBytes(path + "/" + name));
        }


        public static void SaveFloatArray(string path, string name, float[] floatArr)
        {
            CreateDirectory(path);

            File.WriteAllBytes(path + "/" + name, ToBytes(floatArr));
        }

        public static void LoadFloatArray(string path, string name, float[] floatArr)
        {
            //CreateDirectory(path);
            FromBytes(floatArr, File.ReadAllBytes(path + "/" + name));

            //FromBytes(floatArr, File.ReadAllBytes(path + "/" + name));
        }


        public static StreamWriter CreateStreamWriter(string path, string fileName, bool append)
        {
            CreateDirectory(path);
            StreamWriter writer = new StreamWriter(path + fileName, append);
            return writer;
        }


        public static void WriteLine(StreamWriter writer, string value)
        {
            writer.WriteLine(value);
        }

        public static void CloseStreamWriter(StreamWriter writer)
        {
            writer.Close();
        }


    }
}
