namespace Fhi.Slash.Public.SlashMessenger.Slash.Models;

/// <summary>
/// A model that holds the required information for a message to be sent to the Slash API.
/// </summary>
public class SlashMessage
{
    /// <summary>
    /// The access token issued by HelseId.
    /// This token should be an access token for DPoP.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// The DPoP Proof to be sent with the message.
    /// </summary>
    public string? DPoPProof { get; set; }

    /// <summary>
    /// The encrypted message.
    /// </summary>
    public string? Payload { get; set; }

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(AccessToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(DPoPProof);
        ArgumentException.ThrowIfNullOrWhiteSpace(Payload);
    }
}
