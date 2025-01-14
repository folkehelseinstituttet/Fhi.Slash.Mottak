namespace Fhi.Slash.Public.SlashMessengerCLI.Config;

public class AppsettingsConfig
{
    public required string SlashBaseUrl { get; set; }

    public required string SenderVendorName { get; set; }
    public required string SenderSoftwareName { get; set; }
    public required string SenderSoftwareVersion { get; set; }
    public required string SenderExportSoftwareVersion { get; set; }

    public required string HelseIdOpenIdConfigurationUrl { get; set; }
    public required string HelseIdClientId { get; set; }
    public string? HelseIdCertificateThumbprint { get; set; } = null;
    public string? HelseIdCertificatePath { get; set; } = null;
    public string? HelseIdCertificatePassword { get; set; } = null;
    public string? HelseIdClientJsonFilePath { get; set; } = null;
}
