using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class PasswordService
{
    private static readonly string EncryptionKey = "YourSecretKey123456"; // Exact 16, 24 of 32 tekens

    private static byte[] GetValidKey()
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);
        Array.Resize(ref keyBytes, 32); // AES-256 vereist 32 bytes
        return keyBytes;
    }

    public static string EncryptPassword(string password)
    {
        byte[] keyBytes = GetValidKey();
        byte[] iv = new byte[16]; // AES IV is altijd 16 bytes

        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7; // Voorkomt "input data is not a complete block"

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    using (StreamWriter writer = new StreamWriter(cs))
                    {
                        writer.Write(password);
                    }
                }
                return Convert.ToBase64String(ms.ToArray()); // Base64 encode
            }
        }
    }

    public static string DecryptPassword(string encryptedPassword)
    {
        byte[] keyBytes = GetValidKey();
        byte[] iv = new byte[16]; // Zelfde IV als encryptie
        byte[] buffer = Convert.FromBase64String(encryptedPassword); // Base64 decode

        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7; // Voorkomt "input data is not a complete block"

            using (MemoryStream ms = new MemoryStream(buffer))
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(cs))
                    {
                        return reader.ReadToEnd(); // Correct decoderen
                    }
                }
            }
        }
    }
}
