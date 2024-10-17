using IdentityModel.Client;
using Slash.Public.APIMessenger.Config;
using Slash.Public.APIMessenger.Models;
using System.Text;
using System.Text.Json;

using static IdentityModel.OidcConstants;

namespace Slash.Public.APIMessenger.Services;

internal class SlashAPIService(SlashAPIConfig slashAPIConfig)
{
    private readonly SlashAPIConfig _slashAPIConfig = slashAPIConfig;

    private readonly Uri _baseUri = new(slashAPIConfig.BaseUrl);
    public Uri KeysEndpointUri => new(_baseUri, "keys");
    public Uri MessageEndpointUri => new(_baseUri, "message");

    public async Task<List<PublicKeyInfo>> GetPublicKeys(HttpClient httpClient)
    {
        // Send request
        var response = await httpClient.GetAsync(KeysEndpointUri);

        // Deserialize response
        var content =  await response.Content.ReadAsStringAsync();

        // Return parsed response
        return
        [
            .. JsonSerializer.Deserialize<List<PublicKeyInfo>>(content)?
                        .OrderByDescending(k => k.ExpirationDate)
        ];
    }

    public async Task<HttpResponseMessage> SendMessage(HttpClient httpClient, string dPoPAccessToken, string dPoPProof, string payload, Dictionary<string, string> headerValues)
    {
        // Setup HttpClient
        httpClient.SetToken(AuthenticationSchemes.AuthorizationHeaderDPoP, dPoPAccessToken);
        httpClient.DefaultRequestHeaders.Add(HttpHeaders.DPoP, dPoPProof);
        foreach (var headerValue in headerValues)
        {
            httpClient.DefaultRequestHeaders.Add(headerValue.Key, headerValue.Value);
        }

        // Send request
        var response = await httpClient.PostAsync(MessageEndpointUri, new StringContent(payload, Encoding.UTF8, "text/plain"));

        // Return response
        return response;
    }
}
