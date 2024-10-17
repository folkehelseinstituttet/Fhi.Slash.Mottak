using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using Slash.Public.APIMessenger.Services;

namespace Slash.Public.APIMessenger.Clients;

internal class Machine2MachineClient(HelseIdService helseIdService)
{
    private readonly HelseIdService _helseIdService = helseIdService;
    
    private DateTime _persistedAccessTokenExpiresAt = DateTime.MinValue;
    private string _persistedAccessToken = string.Empty;
    private string? _persistedPrivateJwkKid = null;

    public async Task<string> GetAccessToken(HttpClient httpClient, JsonWebKey? dPoPJwk)
    {
        var persistedAccessTokenIdValid = !string.IsNullOrEmpty(_persistedAccessToken) && 
            DateTime.UtcNow < _persistedAccessTokenExpiresAt &&
            (_persistedPrivateJwkKid?.Equals(dPoPJwk?.Kid) ?? true);
      
        // Get new Access Token if persisted is not valid
        if (!persistedAccessTokenIdValid)
        {
            var tokenResponse = await GetAccessTokenFromHelseId(httpClient, dPoPJwk);
            _persistedAccessTokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 30); // Skew expiry time -30 sec. To ensure that token is valid upon request
            _persistedAccessToken = tokenResponse.AccessToken!;
            _persistedPrivateJwkKid = dPoPJwk?.Kid;
        }

        return _persistedAccessToken;
    }

    private async Task<TokenResponse> GetAccessTokenFromHelseId(HttpClient httpClient, JsonWebKey? dPoPJwk)
    {
        // We use the HTTP client to retrieve the response from HelseID:
        var tokenResponse = await RequestClientCredentialsTokenAsync(httpClient, dPoPJwk);

        if (tokenResponse.IsError || tokenResponse.AccessToken == null)
        {
            throw new Exception(tokenResponse.Error);
        }

        return tokenResponse;
    }

    private async Task<TokenResponse> RequestClientCredentialsTokenAsync(HttpClient httpClient, JsonWebKey? dPoPJwk)
    {
        // First request to HelseId. Expect error on this request, asking for DPoP nonce.
        var request = await _helseIdService.CreateClientCredentialsTokenRequestAsync(dPoPJwk, dPoPNonce: null);
        var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(request);
        if (tokenResponse.IsError && tokenResponse.Error == "use_dpop_nonce" && !string.IsNullOrEmpty(tokenResponse.DPoPNonce))
        {
            // Create new request to HelseId with the nonce from in previous response
            request = await _helseIdService.CreateClientCredentialsTokenRequestAsync(dPoPJwk, dPoPNonce: tokenResponse.DPoPNonce);
            tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(request);
        }

        return tokenResponse;
    }
}
