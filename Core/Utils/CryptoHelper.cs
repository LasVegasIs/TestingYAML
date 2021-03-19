using System;
using System.Security.Cryptography;
using System.Text;

namespace Crey.Utils
{
    public static class ByteArrayExtensions
    {
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

        public static byte[] ToUtf8Bytes(this string source)
        {
            return Encoding.UTF8.GetBytes(source);
        }
    }

    public class CryptoHelper
    {
        public static string CalculateMd5Key(string input, string key1, string key2)
        {
            var md_p1 = CalculateMd5Hash($"{key1}{input}");
            return CalculateMd5Hash($"{key2}{md_p1}");

        }

        public static string CalculateMd5Hash(string input)
        {
            using (var md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        public static byte[] CalculateSha256(string source)
        {
            var bytes = Encoding.UTF8.GetBytes(source);
            return CalculateSha256(bytes);
        }

        public static byte[] CalculateSha256(byte[] source)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(source);
            }
        }

        /// <summary>
        ///     Teh method to use for strong hashing - for example passwords.
        ///     Repro of the js code below!
        ///     This must be used crey-wide
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string CreyCryptoHash(string source)
        {
            return CalculateSha256(source).ToHexString().ToUtf8Bytes().ToBase64String();
        }
    }
}