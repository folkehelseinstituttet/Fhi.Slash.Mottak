using System.Text.Json.Serialization;

namespace Slash.Public.APIMessenger.Models;

public class PublicKeyInfo
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("expirationDate")]
    public DateTime ExpirationDate { get; set; }

    [JsonPropertyName("publicKey")]
    public string PublicKey { get; set; } = null!;
}
