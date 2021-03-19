using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Crey.Misc;

namespace Crey.Extensions
{
    public static class ByteArrayExtensions
    {



        public static uint GetUInt(this byte[] data, int position = 0)
        {
            uint result;
            unsafe { fixed (byte* p = data) { result = *(uint*)(p + position); } }
            return result;
        }


        public static int GetInt(this byte[] data, int position = 0)
        {
            int result;
            unsafe { fixed (byte* p = data) { result = *(int*)(p + position); } }
            return result;
        }



        public static byte GetByte(this byte[] data, int position = 0)
        {
            byte result;
            unsafe
            {
                fixed (byte* p = data)
                {
                    result = *(p + position);
                }
            }
            return result;
        }


        public static void SetUInt(this byte[] data, uint value, int position = 0)
        {
            unsafe
            {
                fixed (byte* p = data)
                {
                    *(uint*)(p + position) = value;
                }
            }
        }

        public static void SetInt(this byte[] data, int value, int position = 0)
        {
            unsafe
            {
                fixed (byte* p = data)
                {
                    *(int*)(p + position) = value;
                }
            }
        }

        // pos: 0 1 2 ..
        // val: 1 2 3 .. 
        public static byte[] MakeNiceStreamKey(this byte[] data)
        {
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(i + 1);
            }

            return data;
        }

        //public const string TempPath = @"q:\tmp";

        public static string ToUtf8String(this byte[] array)
        {
            return Encoding.UTF8.GetString(array);
        }


        public static string ToBase64String(this byte[] array)
        {
            return Convert.ToBase64String(array);
        }

        public static string ToHexString(this byte[] bytes, bool lowerCaseLetters = true)
        {
            return ToPrettyHexString(bytes, lowerCaseLetters, bytes.Length, "", "", "");
        }

        public static string ToPrettyHexString(this byte[] bytes, bool lowerCaseLetters = true, int columnCount = 8, string rowItemPrefix = "0x", string rowItemDelimiter = ", ", string rowDelimiter = ",\n")
        {
            var hexString = new StringBuilder(bytes.Length * (rowItemPrefix.Length + 2 + rowItemDelimiter.Length));

            for (var counter = 0; counter < bytes.Length; counter++)
            {
                hexString.Append($"{rowItemPrefix}");

                if (lowerCaseLetters)
                {
                    hexString.Append($"{bytes[counter]:x2}");
                }
                else
                {
                    hexString.Append($"{bytes[counter]:X2}");
                }

                var isLastInRow = (counter + 1) % columnCount == 0;
                hexString.Append($"{(isLastInRow ? rowDelimiter : rowItemDelimiter)}");
            }

            return hexString.ToString();
        }
    }
}