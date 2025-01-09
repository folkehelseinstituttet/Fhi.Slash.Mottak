namespace Slash.Public.SlashMessenger.Slash.Models;

/// <summary>
/// This class represents an encrypted message with neccessary information about the encryption.
/// </summary>
public class EncryptedMessage
{
    /// <summary>
    /// The hash of the message before encryption.
    /// </summary>
    public required string MessageHash { get; set; }

    /// <summary>
    /// Encrypted symmetric key used to encrypt the message.
    /// </summary>
    public required string EncryptedSymmetricKey { get; set; }

    /// <summary>
    /// Encrypted message.
    /// </summary>
    public required string Message { get; set; }
}
