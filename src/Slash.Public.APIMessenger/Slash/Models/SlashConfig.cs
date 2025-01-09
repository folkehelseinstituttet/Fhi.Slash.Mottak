namespace Slash.Public.SlashMessenger.Slash.Models;

/// <summary>
/// The configurations for Slash.
/// </summary>
public class SlashConfig
{
    /// <summary>
    /// The base URL for Slash API.
    /// </summary>
    public required string BaseUrl { get; set; }

    /// <summary>
    /// The name of the EPJ system.
    /// </summary>
    public required string VendorName { get; set; }

    /// <summary>
    /// The name of the software used to send messages.
    /// </summary>
    public required string SoftwareName { get; set; }

    /// <summary>
    /// The version of the software used to send messages.
    /// </summary>
    public required string SoftwareVersion { get; set; }

    /// <summary>
    /// The version of the software used to export data.
    /// </summary>
    public required string ExportSoftwareVersion { get; set; }

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(BaseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(VendorName);
        ArgumentException.ThrowIfNullOrWhiteSpace(SoftwareName);
        ArgumentException.ThrowIfNullOrWhiteSpace(SoftwareVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(ExportSoftwareVersion);
    }
}
