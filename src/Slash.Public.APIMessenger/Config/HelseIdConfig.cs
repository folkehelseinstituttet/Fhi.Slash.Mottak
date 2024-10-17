namespace Slash.Public.APIMessenger.Config;

public class HelseIdConfig
{
    public string ClientJsonFilePath { get; set; } = null!;
    public List<HelseIdClaim> AdditionalClaims { get; set; } = [];
}

public class HelseIdClaim
{
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
}