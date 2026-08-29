using System;
using System.Security.Cryptography;
using System.Text;

namespace Ray.Utils.Security
{
    public static class EncryptionProvider
    {
        public static string GetHash(string value)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            Byte[] textBytes = encoding.GetBytes(value);
            Byte[] keyBytes = encoding.GetBytes(value);
            Byte[] hashBytes;

            using (HMACSHA512 hash = new HMACSHA512(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }
}
