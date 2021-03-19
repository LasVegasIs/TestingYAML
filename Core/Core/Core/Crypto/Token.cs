using System;
using System.Security.Cryptography;
using System.Text;

namespace Core.Crypto
{
    public static class Token
    {
        public static string Generate()
        {
            Guid guid = Guid.NewGuid();
            return Generate(guid.ToByteArray());
        }

        public static string Generate(string id)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] hashedGuid = sha256.ComputeHash(Encoding.ASCII.GetBytes(id));
            return GetStringFromHash(hashedGuid);
        }

        public static string Generate(byte[] id)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] hashedGuid = sha256.ComputeHash(id);
            return GetStringFromHash(hashedGuid);
        }

        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("x2"));
            }
            return result.ToString();
        }
    }
}
