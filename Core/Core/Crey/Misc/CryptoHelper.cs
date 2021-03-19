using Crey.Extensions;
using Crey.Utils;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Crey.Misc
{
    public class CryptoHelper
    {
        private const uint Seed = 13371337;

        /// <summary>
        ///     Generates a key cryptographically strong key string
        /// </summary>
        /// <param name="numBytes"></param>
        /// <returns></returns>
        public static string CreateKey(int numBytes)
        {
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[numBytes];

            rng.GetBytes(buff);
            return buff.ToHexString();
        }

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

        /*

          function hash(text)
          {
            let hashed = CryptoJS.SHA256(text);
            let hashResult = btoa(hashed);
            return hashResult;
          }

         */

        /// <summary>
        ///     Reproduction of the js code above
        ///     pro: supernice thing on client side and cryptostrong
        ///     !! LOWER CASE HEXSTRING !!
        ///     clear -> bytes -> hash -> bytes -> hexstring -> bytes -> tobase64
        /// </summary>
        /// <param name="clearText"></param>
        public static void CryptoHashDemo(string clearText = @"anyadpicsajat")
        {
            var clearBytes = Encoding.ASCII.GetBytes(clearText);
            var encBytes = CalculateSha256(clearBytes);
            var encBase64 = encBytes.ToBase64String();

            var hexString = encBytes.ToHexString();
            var hexStringBytes = Encoding.ASCII.GetBytes(hexString);
            var hexBytesBase64 = hexStringBytes.ToBase64String();

            Console.WriteLine($"pure: {encBase64}");
            Console.WriteLine($"clear:{clearText} clearBytes:[{clearBytes.Length}]");
            Console.WriteLine($"encrypted length:{encBytes.Length}");
            Console.WriteLine($"hex len:{hexString.Length} hex encrypted:[{hexString}]");
            Console.WriteLine($"hex bytes len:{hexStringBytes.Length}");
            Console.WriteLine($"result base64:[{hexBytesBase64}]");
        }

        // stable rather fast version
        public static unsafe ulong MurmurHash64BUnsafe(byte[] source, int len)
        {
            ulong result = 0;

            const uint m = 0x5bd1e995;
            const int r = 24;
            var h1 = Seed ^ (uint)len;
            uint h2 = 0;
            uint k1 = 0;
            uint k2 = 0;

            fixed (byte* sourceP = source)
            {
                var dataPtr = (uint*)sourceP;

                while (len >= 8)
                {
                    k1 = *dataPtr++;
                    k1 *= m;
                    k1 ^= k1 >> r;
                    k1 *= m;
                    h1 *= m;
                    h1 ^= k1;
                    len -= 4;

                    k2 = *dataPtr++;
                    k2 *= m;
                    k2 ^= k2 >> r;
                    k2 *= m;
                    h2 *= m;
                    h2 ^= k2;
                    len -= 4;
                }

                if (len >= 4)
                {
                    k1 = *dataPtr++;
                    k1 *= m;
                    k1 ^= k1 >> r;
                    k1 *= m;
                    h1 *= m;
                    h1 ^= k1;
                    len -= 4;
                }

                if (len >= 3) // last in source
                    h2 ^= (uint)((byte*)dataPtr)[2] << 16;

                if (len >= 2) // last -1
                    h2 ^= (uint)((byte*)dataPtr)[1] << 8;

                if (len >= 1) // last -2
                {
                    h2 ^= ((byte*)dataPtr)[0];
                    h2 *= m;
                }

                h1 ^= h2 >> 18;
                h1 *= m;
                h2 ^= h1 >> 22;
                h2 *= m;
                h1 ^= h2 >> 17;
                h1 *= m;
                h2 ^= h1 >> 19;
                h2 *= m;
                result = h1;
                result = (result << 32) | h2;
            }

            return result;
        }

        public static unsafe ulong MurmurHash64B_Hybrid(byte[] source, int len, int start = 0)
        {
            ulong result = 0;

            fixed (byte* sourceP = &source[start])
            {
                var dataPtr = (uint*)sourceP;

                const uint m = 0x5bd1e995;
                const int r = 24;
                var h1 = Seed ^ (uint)len;
                uint h2 = 0;
                uint k1 = 0;
                uint k2 = 0;
                var offs = 0;
                var lastIndex = len - 1;

                while (len >= 8)
                {
                    k1 = *dataPtr++;
                    k1 *= m;
                    k1 ^= k1 >> r;
                    k1 *= m;
                    h1 *= m;
                    h1 ^= k1;
                    len -= 4;

                    k2 = *dataPtr++;
                    k2 *= m;
                    k2 ^= k2 >> r;
                    k2 *= m;
                    h2 *= m;
                    h2 ^= k2;
                    len -= 4;
                }

                if (len >= 4)
                {
                    k1 = *dataPtr;
                    k1 *= m;
                    k1 ^= k1 >> r;
                    k1 *= m;
                    h1 *= m;
                    h1 ^= k1;
                    len -= 4;
                }

                if (len >= 3) h2 ^= (uint)sourceP[lastIndex - offs++] << 16;

                if (len >= 2) h2 ^= (uint)sourceP[lastIndex - offs++] << 8;

                if (len >= 1)
                {
                    h2 ^= sourceP[lastIndex - offs];
                    h2 *= m;
                }

                h1 ^= h2 >> 18;
                h1 *= m;
                h2 ^= h1 >> 22;
                h2 *= m;
                h1 ^= h2 >> 17;
                h1 *= m;
                h2 ^= h1 >> 19;
                h2 *= m;
                result = h1;
                result = (result << 32) | h2;

                return result;
            }
        }

        public static string CalculateMurmurHash(DataSpan data)
        {
            return MurmurHash64B_Hybrid(data.Buffer, data.Length, data.Start).ToString();
        }

        public static string CalculateMurmurHash(byte[] source)
        {
            // a bit faster, but the lib is not complete, so it is what it is
            return MurmurHash64B_Hybrid(source, source.Length).ToString();
        }

        // test case
        public static string CalculateMurmurHash_STABLE(byte[] source)
        {
            // older version kicks ass for sure
            return MurmurHash64BUnsafe(source, source.Length).ToString();
        }

        // test case - just a few percent faster - but closer to span approach
        public static string CalculateMurmurHash_FRESH(byte[] source)
        {
            // a bit faster, but the lib is not complete, so it is what it is
            // this is the directon with spans
            return MurmurHash64B_Hybrid(source, source.Length).ToString();
        }

        public static void RsaDemo()
        {
            var inputStr = "KolbaszosUbroka";
            var inputBytes = inputStr.ToUtf8Bytes();

            var encodedBytes = Rsa.Encrypt(inputBytes);
            var decodedBytes = Rsa.Decrypt(encodedBytes);

            var outputStr = decodedBytes.ToUtf8String();

            Console.WriteLine($"i:[{inputStr}] o:[{outputStr}] success:{inputStr == outputStr}");
        }
    }
}