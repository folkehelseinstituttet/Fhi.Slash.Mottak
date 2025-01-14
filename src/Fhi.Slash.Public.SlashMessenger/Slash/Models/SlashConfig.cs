namespace Fhi.Slash.Public.SlashMessenger.Slash.Models;

/// <summary>
/// The configurations for Slash.
/// </summary>
public class SlashConfig
{
    /// <summary>
    /// The base URL for Slash API.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// The endpoint for retrieving public keys from the Slash API.
    /// </summary>
    public string KeysEndpoint { get; set; } = "keys";

    /// <summary>
    /// The endpoint for sending messages to the Slash API.
    /// </summary>
    public string MessageEndpoint { get; set; } = "message";

    /// <summary>
    /// The name of the EPJ system.
    /// </summary>
    public string VendorName { get; set; } = string.Empty;

    /// <summary>
    /// The name of the software used to send messages.
    /// </summary>
    public string SoftwareName { get; set; } = string.Empty;

    /// <summary>
    /// The version of the software used to send messages.
    /// </summary>
    public string SoftwareVersion { get; set; } = string.Empty;

    /// <summary>
    /// The version of the software used to export data.
    /// </summary>
    public string ExportSoftwareVersion { get; set; } = string.Empty;

    /// <summary>
    /// Name of the HttpClient used for basic requests requests (Unauthenticated requests)
    /// </summary>
    public string BasicClientName { get; set; } = "SlashBasicClient";

    /// <summary>
    /// Name of the HttpClient used for DPoP requests.
    /// </summary>
    public string DPoPClientName { get; set; } = "SlashDPoPClient";

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(BaseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(KeysEndpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(MessageEndpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(VendorName);
        ArgumentException.ThrowIfNullOrWhiteSpace(SoftwareName);
        ArgumentException.ThrowIfNullOrWhiteSpace(SoftwareVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(ExportSoftwareVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(BasicClientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(DPoPClientName);
    }
}
