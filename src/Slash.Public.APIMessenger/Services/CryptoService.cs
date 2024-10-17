using System.Security.Cryptography;
using System.Text;

namespace Slash.Public.APIMessenger.Services;

internal static class CryptoService
{
    public static byte[] GenerateRandomKey(int bytesLength)
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] key = new byte[bytesLength];
        rng.GetBytes(key);
        return key;
    }

    public static byte[] EncryptWithPublicKey(byte[] data, string publicKey)
    {
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(publicKey);
        return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
    }

    public static byte[] EncryptWithSymmetricKey(byte[] data, byte[] key)
    {
        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var memoryStream = new MemoryStream();
        memoryStream.Write(aes.IV, 0, aes.IV.Length);

        using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();

        return memoryStream.ToArray();
    }

    public static byte[] GetSHA256Hash(string data) =>
        GetSHA256Hash(Encoding.UTF8.GetBytes(data));


    public static byte[] GetSHA256Hash(byte[] data) =>
        SHA256.HashData(data);
}
