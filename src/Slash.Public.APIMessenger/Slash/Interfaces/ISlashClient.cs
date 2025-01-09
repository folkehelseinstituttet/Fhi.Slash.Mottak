using Slash.Public.SlashMessenger.Slash.Models;

namespace Slash.Public.SlashMessenger.Slash.Interfaces;

/// <summary>
/// Defines the interface for the Slash client.
/// Implement this interface to customize the behavior of the Slash client, or inject your own implementation.
/// </summary>
public interface ISlashClient
{
    /// <summary>
    /// The endpoint for retrieving public keys from the Slash API.
    /// </summary>
    string KeysEndpoint { get; }

    /// <summary>
    /// The endpoint for sending messages to the Slash API.
    /// </summary>
    string MessageEndpoint { get; }

    /// <summary>
    /// Retrieves a list of public keys from the Slash API.
    /// </summary>
    /// <returns>A list of <see cref="PublicKeyInfo"/> objects containing the public keys.</returns>
    public Task<List<PublicKeyInfo>> GetPublicKeys();

    /// <summary>
    /// Sends a message to the Slash API.
    /// </summary>
    /// <param name="message">A <see cref="SlashMessage"/> containing the data to be sent.</param>
    /// <returns>An <see cref="SendMessageResponse"/> representing the response from the Slash API.</returns>
    public Task<SendMessageResponse> SendMessage(SlashMessage message);
}
