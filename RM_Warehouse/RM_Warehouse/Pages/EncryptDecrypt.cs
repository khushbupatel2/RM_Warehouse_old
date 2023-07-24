using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RM_Warehouse.Pages
{
    // THIS CLASS IS A COPY OF EncryptDecrypt CLASS FROM YARDMANAGER.WITH MINOR CHANGE FOR VIKey.
    // REST IS SAME AS YARDMANAGER. 
    public class EncryptDecrypt
    {
        static readonly string PasswordHash = "55ppM9jNRXm6GHww";
        static readonly string SaltKey = "5O1QZiYl220bsdsf";
        //    static readonly string VIKey = "Tt3Yd122XzSXdsASFDA";
        // changing VIKey to 16 bytes for .net 6
        static readonly string VIKey = "Tt3Yd122XzSXdsAS";
        public static string Encrypt(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));

            byte[] cipherTextBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherTextBytes = memoryStream.ToArray();
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Convert.ToBase64String(cipherTextBytes);
        }

        public static string Decrypt(string encryptedText)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };

            var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }

        public static string buildDB2ConnectionString(string server, string dbname, string un, string pwd)
        {
            string connStr = "server";
            connStr = "Provider=IBMDADB2;Connection Timeout=9000;Database=" + dbname + ";Hostname=" + server + ";Protocol=TCPIP;Port=50000;Uid=" + un + ";Pwd=" + Decrypt(pwd);
            return connStr;
        }
    }
}