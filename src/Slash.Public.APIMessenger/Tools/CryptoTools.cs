using System.Security.Cryptography;

namespace Slash.Public.SlashMessenger.Tools;

/// <summary>
/// Provides utility methods for encryption and hashing.
/// </summary>
internal static class CryptoTools
{
    /// <summary>
    /// Generates a random key of the specified length.
    /// </summary>
    /// <param name="bytesLength">The length, in bytes, of the random key to generate.</param>
    /// <returns>A byte array representing the randomly generated key.</returns>
    public static byte[] GenerateRandomKey(int bytesLength)
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] key = new byte[bytesLength];
        rng.GetBytes(key);
        return key;
    }

    /// <summary>
    /// Encrypts data using a public key.
    /// </summary>
    /// <param name="data">The data to be encrypted.</param>
    /// <param name="publicKey">The public key used for encryption, in PEM format.</param>
    /// <returns>A byte array containing the encrypted data.</returns>
    public static byte[] EncryptWithPublicKey(byte[] data, string publicKey)
    {
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(publicKey);
        return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
    }

    /// <summary>
    /// Encrypts data using a symmetric key (AES).
    /// </summary>
    /// <param name="data">The data to be encrypted.</param>
    /// <param name="key">The symmetric key used for encryption.</param>
    /// <returns>A byte array containing the encrypted data.</returns>
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
}
