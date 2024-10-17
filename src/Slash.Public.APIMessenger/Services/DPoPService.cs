using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using IdentityModel;
using Microsoft.IdentityModel.Tokens;

namespace Slash.Public.APIMessenger.Services;

internal static class DPoPService
{
    public static string CreateDPoPProof(JsonWebKey jwk, string rsaAlgo, string url, string httpMethod, string? dPoPNonce = null, string? accessToken = null, Dictionary<string, string>? customClaims = null)
    {
        var signingCredentials = new SigningCredentials(jwk, algorithm: rsaAlgo);

        var newJwk = jwk.Kty switch
        {
            JsonWebAlgorithmsKeyTypes.EllipticCurve => new Dictionary<string, string>
            {
                [JsonWebKeyParameterNames.Kty] = jwk.Kty,
                [JsonWebKeyParameterNames.X] = jwk.X,
                [JsonWebKeyParameterNames.Y] = jwk.Y,
                [JsonWebKeyParameterNames.Crv] = jwk.Crv,
            },
            JsonWebAlgorithmsKeyTypes.RSA => new Dictionary<string, string>
            {
                [JsonWebKeyParameterNames.Kty] = jwk.Kty,
                [JsonWebKeyParameterNames.N] = jwk.N,
                [JsonWebKeyParameterNames.E] = jwk.E,
                [JsonWebKeyParameterNames.Alg] = signingCredentials.Algorithm,
            },
            _ => throw new InvalidOperationException("Invalid key type for DPoP proof.")
        };

        var jwtHeader = new JwtHeader(signingCredentials)
        {
            [JwtClaimTypes.TokenType] = "dpop+jwt",
            [JwtClaimTypes.JsonWebKey] = newJwk,
        };

        var payload = new JwtPayload
        {
            [JwtClaimTypes.JwtId] = Guid.NewGuid().ToString(),
            [JwtClaimTypes.DPoPHttpMethod] = httpMethod,
            [JwtClaimTypes.DPoPHttpUrl] = url,
            [JwtClaimTypes.IssuedAt] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };

        // Custom claims not in the DPoP standard
        if (customClaims != null)
        {
            foreach (var claim in customClaims)
            {
                payload[claim.Key] = claim.Value;
            }
        }

        // Used when accessing the authentication server (HelseID):
        if (!string.IsNullOrEmpty(dPoPNonce))
        {
            // nonce: A recent nonce provided via the DPoP-Nonce HTTP header.
            payload[JwtClaimTypes.Nonce] = dPoPNonce;
        }

        // Used when accessing an API that requires a DPoP token:
        if (!string.IsNullOrEmpty(accessToken))
        {
            // ath: hash of the access token. The value MUST be the result of a base64url encoding
            // the SHA-256 [SHS] hash of the ASCII encoding of the associated access token's value.
            var hash = SHA256.HashData(Encoding.ASCII.GetBytes(accessToken));
            var ath = Base64Url.Encode(hash);

            payload[JwtClaimTypes.DPoPAccessTokenHash] = ath;
        }

        var jwtSecurityToken = new JwtSecurityToken(jwtHeader, payload);
        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }
}
