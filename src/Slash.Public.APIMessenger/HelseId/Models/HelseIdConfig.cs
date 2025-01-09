using System.Security.Cryptography.X509Certificates;

namespace Slash.Public.SlashMessenger.HelseId.Models;

public class HelseIdConfig
{
    /// <summary>
    /// The full URL of the endpoint used to obtain tokens from HelseID.
    /// This value is to be found in HelseID's OpenId Configurations:
    /// https://helseid-sts.nhn.no/.well-known/openid-configuration
    /// </summary>
    public required string TokenEndpoint { get; set; }

    /// <summary>
    /// The unique identifier for the client, used to obtain tokens from HelseID.
    /// This value is issued by HelseID when registering a new client.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// The certificate used to sign requests to HelseID when obtaining tokens.
    /// The private key in this certificate must correspond to the public key associated with the client in HelseID.
    /// 
    /// Either a certificate or a client definition must be provided.
    /// If both are specified, the certificate will take precedence.
    /// </summary>
    public X509Certificate2? Certificate { get; set; }

    /// <summary>
    /// The client definition issued by HelseID when creating a HelseID client.
    /// 
    /// Either a certificate or a client definition must be provided.
    /// If both are specified, the certificate will take precedence.
    /// </summary>
    public HelseIdClientDefinition? ClientDefinition { get; set; }

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(TokenEndpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(ClientId);

        if (Certificate == null && ClientDefinition == null)
        {
            throw new ArgumentException($"Either {nameof(Certificate)} or {nameof(ClientDefinition)} must be set");
        }
    }
}
