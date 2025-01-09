using System.Text.Json.Serialization;

namespace Slash.Public.SlashMessenger.HelseId.Models;

/// <summary>
/// Represents a HelseId client definition.
/// The JSON file containing the client definition is issued by HelseId.
/// </summary>
public class HelseIdClientDefinition
{
    [JsonPropertyName("clientName")]
    public string? ClientName { get; set; }

    [JsonPropertyName("authority")]
    public Uri? Authority { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("grantTypes")]
    public string[]? GrantTypes { get; set; }

    [JsonPropertyName("scopes")]
    public string[]? Scopes { get; set; }

    [JsonPropertyName("secretType")]
    public string? SecretType { get; set; }

    [JsonPropertyName("rsaPrivateKey")]
    public string? RsaPrivateKey { get; set; }

    [JsonPropertyName("rsaKeySizeBits")]
    public long RsaKeySizeBits { get; set; }

    [JsonPropertyName("privateJwk")]
    public string? PrivateJwk { get; set; }
}
