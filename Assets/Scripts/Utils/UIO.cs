using UnityEngine;
using System.Collections;
using System;
using System.IO;
namespace Lexmou.Utils
{
    /**
     * \class UIO
     * \brief Util class that allow reading and writing to files and data streams
     * \details This UIO static class provides a set of utility static methods that can be used to perform operations on I/O streams
     */
    public static class UIO
    {
        /**
        * \brief Retrieve the command line argument based on the name of the command
        * \details Go through the command-line arguments and return the command value
        * \param[in]   name  Name of the command
        * \return A \e string when the command exits and \e null otherwise
        */

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

        /**
        * \brief Create a directory
        * \details Determines whether the directory exists before creation
        * \param[in]   path  Relative Path of the directory
        */

        public static void CreateDirectory(string path)
        {
            try
            {
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

        /**
        * \brief Convert a 2D generic array to a \e byte array
        * \param[in]   array  2D generic array to convert
        * \return A 1D \e byte array
        */

        public static byte[] ToBytes<T>(this T[,] array) where T : struct
        {
            var buffer = new byte[array.GetLength(0) * array.GetLength(1) * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))];
            Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
            return buffer;
        }

        /**
        * \brief Convert a 1D \e byte array to a 2D generic array
        * \param[in]   array  1D \e byte to convert
        * \return A 2D generic array
        */

        public static void FromBytes<T>(this T[,] array, byte[] buffer) where T : struct
        {
            var len = Math.Min(array.GetLength(0) * array.GetLength(1) * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), buffer.Length);
            Buffer.BlockCopy(buffer, 0, array, 0, len);
        }

        /**
        * \brief Convert a 1D generic array to a \e byte array
        * \param[in]   array  1D generic array to convert
        * \return A 1D \e byte array
        */

        public static byte[] ToBytes<T>(this T[] array) where T : struct
        {
            var buffer = new byte[array.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))];
            Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
            return buffer;
        }

        /**
        * \brief Convert a 1D \e byte array to a 1D generic array
        * \param[in]   array  1D \e byte to convert
        * \return A 1D generic array
        */
        public static void FromBytes<T>(this T[] array, byte[] buffer) where T : struct
        {
            var len = Math.Min(array.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), buffer.Length);
            Buffer.BlockCopy(buffer, 0, array, 0, len);
        }

        /**
        * \brief Save a 2D \e float array in a \e byte file
        * \details Use a relative path to save a 2D \e float array in a \e byte file. if necessary, create the target directory.
        * \param[in]   path  Relative path
        * \param[in]   name  Storage file name
        * \param[in]   floatArr  2D \e float array to save    
        */

        public static void SaveFloatArray(string path, string name, float[,] floatArr)
        {
            CreateDirectory(path);
            File.WriteAllBytes(path + "/" + name, ToBytes(floatArr));
        }

        /**
        * \brief Load a \e byte file in a 2D \e float array
        * \details Use a relative path to load a \e byte file in 2D \e float array.
        * \param[in]   path  Relative path
        * \param[in]   name  Storage file name
        * \param[in]   floatArr  2D \e float array where to save data  
        */


        public static void LoadFloatArray(string path,string name, float[,] floatArr)
        {
            FromBytes(floatArr, File.ReadAllBytes(path + "/" + name));
        }

        /**
        * \brief Save a 1D \e float array in a \e byte file
        * \details Use a relative path to save a 1D \e float array in a \e byte file. if necessary, create the target directory.
        * \param[in]   path  Relative path
        * \param[in]   name  Storage file name
        * \param[in]   floatArr  1D \e float array to save    
        */

        public static void SaveFloatArray(string path, string name, float[] floatArr)
        {
            CreateDirectory(path);

            File.WriteAllBytes(path + "/" + name, ToBytes(floatArr));
        }

        /**
        * \brief Load a \e byte file in a 1D \e float array
        * \details Use a relative path to load a \e byte file in 1D \e float array.
        * \param[in]   path  Relative path
        * \param[in]   name  Storage file name
        * \param[in]   floatArr  1D \e float array where to save data  
        */

        public static void LoadFloatArray(string path, string name, float[] floatArr)
        {
            FromBytes(floatArr, File.ReadAllBytes(path + "/" + name));
        }


        /**
        * \brief Implement a \e TextWriter for writing characters to a stream
        * \details if necessary, create the target directory. Opens a file in write mode.
        * \param[in]   path  Relative path
        * \param[in]   fileName  Storage file name
        * \param[in]   append  When true, the file is not overwritten  
        * \returns  A \e StreamWriter linked with the file
        */


        public static StreamWriter CreateStreamWriter(string path, string fileName, bool append)
        {
            CreateDirectory(path);
            StreamWriter writer = new StreamWriter(path + fileName, append);
            return writer;
        }


        /**
        * \brief Write data followed by a line terminator to the stream.
        * \param[in] writer The \e StreamWriter used
        * \param[in] value Line to write
        */

        public static void WriteLine(StreamWriter writer, string value)
        {
            writer.WriteLine(value);
        }

        /**
        * \brief Close a \e StreamWriter
        * \param[in] writer The \e StreamWriter to close
        */

        public static void CloseStreamWriter(StreamWriter writer)
        {
            writer.Close();
        }


    }
}
