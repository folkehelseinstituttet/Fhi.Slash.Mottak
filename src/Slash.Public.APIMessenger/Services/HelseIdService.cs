using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using Slash.Public.APIMessenger.Config;
using Slash.Public.APIMessenger.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using static IdentityModel.OidcConstants;

namespace Slash.Public.APIMessenger.Services;

internal class HelseIdService
{
    private readonly string _clientFilePath;
    private readonly JsonWebKey _privateJwk;
    private readonly HelseIdClientDefinition _helseIdClientDefinition;
    private readonly List<HelseIdClaim> _additionalConfigClaims;

    public HelseIdService(HelseIdConfig config)
    {
        _clientFilePath = config.ClientJsonFilePath;
        var clientFile = File.ReadAllText(_clientFilePath);
        _helseIdClientDefinition = JsonSerializer.Deserialize<HelseIdClientDefinition>(clientFile)!;
        _privateJwk = new JsonWebKey(_helseIdClientDefinition.PrivateJwk);
        _additionalConfigClaims = config.AdditionalClaims;
    }

    public JsonWebKey GetPrivateJwk() => _privateJwk;

    public async Task<ClientCredentialsTokenRequest> CreateClientCredentialsTokenRequestAsync(JsonWebKey? dPoPJwk, string? dPoPNonce)
    {
        var client = new HttpClient();
        var disco = await client.GetDiscoveryDocumentAsync(_helseIdClientDefinition.Authority?.ToString());
        var clientAssertion = BuildClientAssertion();

        return new ClientCredentialsTokenRequest
        {
            Address = disco.TokenEndpoint ?? throw new InvalidOperationException("No token endpoint in Discovery Document for HelseId"),
            ClientAssertion = clientAssertion,
            ClientId = _helseIdClientDefinition.ClientId.ToString(),
            GrantType = GrantTypes.ClientCredentials,
            ClientCredentialStyle = ClientCredentialStyle.PostBody,
            DPoPProofToken = CreateDPoPProof(disco.TokenEndpoint, "POST", dPoPJwk, dPoPNonce)
        };
    }

    public string CreateDPoPProof(string url, string httpMethod, JsonWebKey? dPoPJwk, string? dPoPNonce = null, string? accessToken = null, Dictionary<string, string>? customClaims = null)
    {
        dPoPJwk ??= _privateJwk;

        return DPoPService.CreateDPoPProof(dPoPJwk, dPoPJwk.Alg ?? SecurityAlgorithms.RsaSha256, url, httpMethod, dPoPNonce, accessToken, customClaims);
    }

    private ClientAssertion BuildClientAssertion()
    {
        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Subject, _helseIdClientDefinition.ClientId.ToString()),
            new(JwtClaimTypes.IssuedAt, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
            new(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N"))
        };

        // Add additional claims
        foreach (var claim in _additionalConfigClaims)
        {
            claims.Add(new(claim.Name, claim.Value));
        }

        var token = new JwtSecurityToken(
            _helseIdClientDefinition.ClientId.ToString(),
            _helseIdClientDefinition.Authority?.ToString(),
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddSeconds(60),
            GetSigningCredentialsFromHelseIdClient(_helseIdClientDefinition));

        return new ClientAssertion
        {
            Type = ClientAssertionTypes.JwtBearer,
            Value = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }

    private static SigningCredentials? GetSigningCredentialsFromHelseIdClient(HelseIdClientDefinition helseIdClientDefinition)
    {
        if (string.IsNullOrEmpty(helseIdClientDefinition?.RsaPrivateKey))
        {
            throw new InvalidOperationException($"Could not find a RsaPrivateKey in Client Definition: {helseIdClientDefinition?.ClientName}");
        }

        RSA rsa = RSA.Create();
        rsa.FromXmlString(helseIdClientDefinition.RsaPrivateKey);
        var rsaKey = new RsaSecurityKey(rsa);
        return new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);
    }
}