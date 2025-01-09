﻿using IdentityModel;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Slash.Public.SlashMessenger.Extensions;
using Slash.Public.SlashMessenger.HelseId.Exceptions;
using Slash.Public.SlashMessenger.HelseId.Interfaces;
using Slash.Public.SlashMessenger.HelseId.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static IdentityModel.OidcConstants;

using TokenResponse = IdentityModel.Client.TokenResponse;

namespace Slash.Public.SlashMessenger.HelseId;

/// <summary>
/// The default implementation of <see cref="IHelseIdClient"/>.
///
/// This class's main responsibility is to communicate with HelseId to get an access token.
/// You can inject your own implementation of <see cref="IHelseIdClient"/> if you want to override the default behavior.
/// </summary>
public class DefaultHelseIdClient : IHelseIdClient
{
    /// <summary>
    /// Name of the HttpClient used for basic requests requests (Unauthenticated requests)
    /// </summary>
    public const string BasicClientName = "HelseIdBasicClient";

    private readonly HelseIdConfig _helseIdConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DefaultHelseIdClient> _logger;
    private readonly JsonWebKey _helseIdJwk;

    /// <summary>
    /// Constructor for <see cref="DefaultHelseIdClient"/>.
    /// </summary>
    /// <param name="helseIdConfig">Configuration settings for HelseID.</param>
    /// <param name="logger">The logger used for logging operations.</param>
    /// <param name="httpClientFactory">Factory for creating <see cref="HttpClient"/> instances.</param>
    /// <param name="helseIdJwk">The <see cref="JsonWebKey"/> used for HelseID authentication.</param>
    public DefaultHelseIdClient(
        HelseIdConfig helseIdConfig, 
        ILogger<DefaultHelseIdClient> logger,
        IHttpClientFactory httpClientFactory,
        [FromKeyedServices(ServiceCollectionExtensions.helseIdJwkKey)] JsonWebKey helseIdJwk)
    {
        helseIdConfig.Validate();

        _helseIdConfig = helseIdConfig;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _helseIdJwk = helseIdJwk;
    }

    /// <summary>
    /// Gets an access token from HelseId.
    /// </summary>
    /// <param name="dPoPProofJwk">The <see cref="JsonWebKey"/> used when generating the DPoP proof.</param>
    /// <returns>A <see cref="TokenResponse"/> with the Access Token</returns>
    /// <exception cref="HelseIdClientException">Thrown if the access token retrieval fails.</exception>
    public virtual async Task<TokenResponse> GetAccessToken(JsonWebKey dPoPProofJwk)
    {
        _logger.LogDebug("Getting Access Token from HelseId");

        // Creating Client Credentials Token Request
        ClientCredentialsTokenRequest clientCredentialsTokenRequest;
        try
        {
            clientCredentialsTokenRequest = CreateClientCredentialsTokenRequestAsync(dPoPProofJwk, dPoPNonce: null);
        }
        catch (Exception ex)
        {
            throw new HelseIdClientException("Could not create Client Credentials Token Request", ex);
        }

        // Requesting Access Token from HelseId
        TokenResponse tokenResponse;
        try
        {
            using var httpClient = _httpClientFactory.CreateClient(BasicClientName);
            tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(clientCredentialsTokenRequest);

            // If the token response requires a DPoP nonce, create a new request with the nonce from the previous response
            if (tokenResponse.IsError && tokenResponse.Error == "use_dpop_nonce" && !string.IsNullOrEmpty(tokenResponse.DPoPNonce))
            {
                clientCredentialsTokenRequest = CreateClientCredentialsTokenRequestAsync(dPoPProofJwk, tokenResponse.DPoPNonce);
                tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(clientCredentialsTokenRequest);
            }

            if (tokenResponse.IsError || tokenResponse.AccessToken == null)
            {
                var errorMessage = tokenResponse.Error ?? "No access token in the token response returned from HelseId";
                throw new InvalidOperationException(errorMessage);
            }
        }
        catch (Exception ex)
        {
            throw new HelseIdClientException("Could not get Access Token from HelseId", ex);
        }

        return tokenResponse;
    }

    /// <summary>
    /// Creates a <see cref="ClientCredentialsTokenRequest"/> to be used to get an access token from HelseId.
    /// </summary>
    /// <param name="dPoPProofJwk">The <see cref="JsonWebKey"/> used when generating DPoP proofs.</param>
    /// <param name="dPoPNonce">A nonce issued by HelseId</param>
    /// <returns>A <see cref="ClientCredentialsTokenRequest"/> for requesting a new access token.</returns>
    protected virtual ClientCredentialsTokenRequest CreateClientCredentialsTokenRequestAsync(JsonWebKey dPoPProofJwk, string? dPoPNonce) => new()
    {
        Address = _helseIdConfig.TokenEndpoint,
        ClientAssertion = BuildClientAssertion(),
        ClientId = _helseIdConfig.ClientId,
        GrantType = GrantTypes.ClientCredentials,
        ClientCredentialStyle = ClientCredentialStyle.PostBody,
        DPoPProofToken = dPoPProofJwk.CreateDPoPProof(_helseIdConfig.TokenEndpoint, HttpMethod.Post.Method, dPoPNonce)
    };

    /// <summary>
    /// Creates a client assertion for use in client credentials token requests to HelseID. 
    /// The client assertion is a JWT signed with the private key corresponding to the public key registered with HelseID.
    /// </summary>
    /// <returns>A <see cref="ClientAssertion"/> to be used in a <see cref="ClientCredentialsTokenRequest"/>.</returns>
    protected virtual ClientAssertion BuildClientAssertion()
    {
        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Subject, _helseIdConfig.ClientId.ToString()),
            new(JwtClaimTypes.IssuedAt, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
            new(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N"))
        };

        var token = new JwtSecurityToken(
            _helseIdConfig.ClientId.ToString(),
            _helseIdConfig.TokenEndpoint,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddSeconds(60),
            new SigningCredentials(_helseIdJwk, SecurityAlgorithms.RsaSha256));

        return new ClientAssertion
        {
            Type = ClientAssertionTypes.JwtBearer,
            Value = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }
}
