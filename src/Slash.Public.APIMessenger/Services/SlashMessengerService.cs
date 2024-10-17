using IdentityModel;
using Microsoft.IdentityModel.Tokens;
using Slash.Public.APIMessenger.Clients;
using Slash.Public.APIMessenger.Config;
using Slash.Public.APIMessenger.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Slash.Public.APIMessenger.Services;

public class SlashMessengerService
{
    private readonly HelseIdService _helseIdService;
    private readonly Machine2MachineClient _m2mClient;
    private readonly SlashAPIService _slashAPIService;
    private readonly SenderConfig _senderConfig;

    public SlashMessengerService(SenderConfig senderConfig, SlashAPIConfig slashApiConfig, HelseIdConfig helseIdConfig)
    {
        _helseIdService = new HelseIdService(helseIdConfig);
        _m2mClient = new Machine2MachineClient(_helseIdService);
        _senderConfig = senderConfig;
        _slashAPIService = new SlashAPIService(slashApiConfig);
    }

    public async Task<HttpResponseMessage> SendMessage(string rawJsonMessage, string messageType, string messageVersion, JsonWebKey? dPoPJwk = null)
    {
        // Validation of input
        if (string.IsNullOrWhiteSpace(rawJsonMessage))
        {
            throw new ArgumentException("Message cannot be empty", nameof(rawJsonMessage));
        }
        if (string.IsNullOrWhiteSpace(messageType))
        {
            throw new ArgumentException("MessageType cannot be empty", nameof(messageType));
        }
        if (string.IsNullOrWhiteSpace(messageVersion))
        {
            throw new ArgumentException("MessageVersion cannot be empty", nameof(messageVersion));
        }

        try
        {
            var msgObj = JsonSerializer.Deserialize<object>(rawJsonMessage);
            if (msgObj is not JsonElement jsonElement || jsonElement.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("Payload must be an array of objects");
            }
        }
        catch(JsonException jre)
        {
            throw new InvalidOperationException("Could not parse json to an array of messages", jre);
        }

        // Setup HttpClient
        var httpClient = new HttpClient();

        // Get Public Key from Slash API
        PublicKeyInfo? slashPublicKeyInfo;
        try
        {
            var slashPublicKeyInfos = await _slashAPIService.GetPublicKeys(httpClient);
            slashPublicKeyInfo = slashPublicKeyInfos.First();
        }
        catch(Exception ex)
        {
            throw new Exception("Could not get public key from Slash API", ex);
        }

        // Encrypt payload, encrypt key and add encrypted key to header
        var msgInBytes = Encoding.UTF8.GetBytes(rawJsonMessage);
        var msgHash = Base64Url.Encode(SHA256.HashData(msgInBytes));
        var symmetricKey = CryptoService.GenerateRandomKey(32);
        byte[] encSymKeyBytes = CryptoService.EncryptWithPublicKey(symmetricKey, slashPublicKeyInfo.PublicKey);
        byte[] encryptedMessageBytes = CryptoService.EncryptWithSymmetricKey(msgInBytes, symmetricKey);
        var encSymKey = Base64Url.Encode(encSymKeyBytes);
        var encryptedMessage = Convert.ToBase64String(encryptedMessageBytes);

        // Get HelseId Token
        string dPoPAccessToken;
        try
        {
            dPoPAccessToken = await _m2mClient.GetAccessToken(httpClient, dPoPJwk);
        }
        catch (Exception ex)
        {
            throw new Exception("Could not get access token from HelseId", ex);
        }

        // Create DPoP Proof
        string dPoPProof;
        try
        {
            dPoPProof = _helseIdService.CreateDPoPProof(_slashAPIService.MessageEndpointUri.AbsoluteUri, "POST", dPoPJwk, accessToken: dPoPAccessToken, customClaims: new Dictionary<string, string>(){
                { "msg_type", messageType },
                { "msg_version", messageVersion },
                { "msg_hash", msgHash },
                { "enc_sym_key", encSymKey },
                { "enc_key_id", slashPublicKeyInfo.Id.ToString() }
            });
        }
        catch (Exception ex)
        {
            throw new Exception("Could not create DPoP Proof", ex);
        }

        // Send message
        var headerValues = new Dictionary<string, string>()
        {
            { "x-vendor-name", _senderConfig.VendorName },
            { "x-software-name", _senderConfig.SoftwareName },
            { "x-software-version", _senderConfig.SoftwareVersion },
            { "x-export-software-version", _senderConfig.ExportSoftwareVersion },
            { "x-data-extraction-date", DateTime.Now.ToString("dd.MM.yyyy") },
        };

        httpClient = new HttpClient();
        var response = await _slashAPIService.SendMessage(httpClient, dPoPAccessToken, dPoPProof, encryptedMessage, headerValues);

        return response;
    }
}
