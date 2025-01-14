using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Newtonsoft.Json.Linq;
using Fhi.Slash.Public.SlashMessenger.HelseId;
using Fhi.Slash.Public.SlashMessenger.HelseId.Interfaces;
using Fhi.Slash.Public.SlashMessenger.HelseId.Models;
using Fhi.Slash.Public.SlashMessenger.Slash;
using Fhi.Slash.Public.SlashMessenger.Slash.Exceptions;
using Fhi.Slash.Public.SlashMessenger.Slash.Interfaces;
using Fhi.Slash.Public.SlashMessenger.Slash.Models;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace Fhi.Slash.Public.SlashMessenger.UnitTests;

[TestClass]
public class SlashMessengerTests
{
    private readonly string _accessToken = File.ReadAllText("TestFiles/access-token.txt");
    private readonly string _testMessage = File.ReadAllText("TestFiles/hst_avtale_test_message.json");
    private readonly JObject _testKeys = JObject.Parse(File.ReadAllText("TestFiles/test_keys.json"))!;
    private readonly X509Certificate2 _certificate = new("TestFiles/Certs/test_cert_without_password.pfx", string.Empty, X509KeyStorageFlags.Exportable);
    private readonly JsonWebKey _dPoPProofJwk = new(File.ReadAllText("TestFiles/private_jwk.json"));

    private readonly Uri _slashBaseUrl = new("https://slash-api.just-a-test.com");
    private readonly Uri _helseIdBaseUrl = new("https://helseid-api.just-a-test.com");

    private readonly Dictionary<object, object> _cacheStore = [];

    private readonly SlashConfig _defaultSlashConfig = new()
    {
        BaseUrl = "https://slash-api.just-a-test.com",
        ExportSoftwareVersion = "1.0.0",
        SoftwareName = "TestSoftware",
        SoftwareVersion = "1.0.0",
        VendorName = "TestVendor",
    };

    [TestMethod]
    public async Task ShouldThrowErrorIfMessageIsNotAValidJson()
    {
        // Arrange
        var slashService = new DefaultSlashService(_defaultSlashConfig, new NullLogger<DefaultSlashService>(), new Mock<ISlashClient>().Object, new Mock<IHelseIdService>().Object, _dPoPProofJwk);

        // Act
        var act = async () => await slashService.PrepareAndSendMessage("NOT_A_VALID_JSON", "test", "test");

        // Assert
        await act.Should().ThrowAsync<SlashServiceException>()
            .WithMessage("Input validation failed");
    }

    [TestMethod]
    public async Task ShouldThrowErrorIfMessageIsNotAJsonArray()
    {
        // Arrange
        var slashService = new DefaultSlashService(_defaultSlashConfig, new NullLogger<DefaultSlashService>(), new Mock<ISlashClient>().Object, new Mock<IHelseIdService>().Object, _dPoPProofJwk);

        // Act
        var act = async () => await slashService.PrepareAndSendMessage("{\"error\": \"SHOULD_NOT_BE_JSON_OBJECT\"}", "test", "test");

        // Assert
        await act.Should().ThrowAsync<SlashServiceException>()
            .WithMessage("Input validation failed");
    }

    [TestMethod]
    public async Task HelseIdClientShouldCacheAccessToken()
    {
        // Arrange
        var dPoPHttpClientMock = new Mock<HttpClient>();
        var slashHttpFactoryMock = new Mock<IHttpClientFactory>();
        slashHttpFactoryMock.Setup(x => x.CreateClient(_defaultSlashConfig.BasicClientName)).Returns(CreateSlashDefaultClient);
        slashHttpFactoryMock.Setup(x => x.CreateClient(_defaultSlashConfig.DPoPClientName)).Returns(CreateSlashDPoPClient);

        var helseIdConfig = new HelseIdConfig
        {
            TokenEndpoint = new Uri(_helseIdBaseUrl, "token").AbsoluteUri,
            ClientId = "just-a-test",
            Certificate = _certificate
        };
        var helseIdHttpFactoryMock = new Mock<IHttpClientFactory>();
        helseIdHttpFactoryMock.Setup(x => x.CreateClient(helseIdConfig.BasicClientName)).Returns(CreateHelseIdClient);

        var memoryCacheMock = SetupMemoryCacheMock();
        
        var helseIdClient = new DefaultHelseIdClient(helseIdConfig, new NullLogger<DefaultHelseIdClient>(), helseIdHttpFactoryMock.Object, _dPoPProofJwk);
        var helseIdService = new DefaultHelseIdService(new NullLogger<DefaultHelseIdService>(), memoryCacheMock.Object, helseIdClient);

        var slashClient = new DefaultSlashClient(_defaultSlashConfig, new NullLogger<DefaultSlashClient>(), slashHttpFactoryMock.Object);
        var slashService = new DefaultSlashService(_defaultSlashConfig, new NullLogger<DefaultSlashService>(), slashClient, helseIdService, _dPoPProofJwk);

        // Act
        await slashService.PrepareAndSendMessage(_testMessage, "HST_Avtale", "1");
        await slashService.PrepareAndSendMessage(_testMessage, "HST_Avtale", "1");

        // Assert
        memoryCacheMock.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny!), Times.Exactly(3));
        memoryCacheMock.Verify(x => x.CreateEntry(It.IsAny<object>()), Times.Once);
    }

    [TestMethod]
    public async Task ShouldSendMessageToSlash()
    {
        // Arrange
        var dPoPHttpClientMock = new Mock<HttpClient>();
        var slashHttpFactoryMock = new Mock<IHttpClientFactory>();
        slashHttpFactoryMock.Setup(x => x.CreateClient(_defaultSlashConfig.BasicClientName)).Returns(CreateSlashDefaultClient);
        slashHttpFactoryMock.Setup(x => x.CreateClient(_defaultSlashConfig.DPoPClientName)).Returns(CreateSlashDPoPClient);

        var helseIdConfig = new HelseIdConfig
        {
            TokenEndpoint = new Uri(_helseIdBaseUrl, "token").AbsoluteUri,
            ClientId = "just-a-test",
            Certificate = _certificate
        };
        var helseIdHttpFactoryMock = new Mock<IHttpClientFactory>();
        helseIdHttpFactoryMock.Setup(x => x.CreateClient(helseIdConfig.BasicClientName)).Returns(CreateHelseIdClient);

        var memoryCacheMock = SetupMemoryCacheMock();

       
        var helseIdClient = new DefaultHelseIdClient(helseIdConfig, new NullLogger<DefaultHelseIdClient>(), helseIdHttpFactoryMock.Object, _dPoPProofJwk);
        var helseIdService = new DefaultHelseIdService(new NullLogger<DefaultHelseIdService>(), memoryCacheMock.Object, helseIdClient);

        var slashClient = new DefaultSlashClient(_defaultSlashConfig, new NullLogger<DefaultSlashClient>(), slashHttpFactoryMock.Object);
        var slashService = new DefaultSlashService(_defaultSlashConfig, new NullLogger<DefaultSlashService>(), slashClient, helseIdService, _dPoPProofJwk);

        // Act
        var response = await slashService.PrepareAndSendMessage(_testMessage, "HST_Avtale", "1");

        // Assert
        response.ProcessMessageResponse.Delivered.Should().BeTrue();
    }

    private Mock<IMemoryCache> SetupMemoryCacheMock()
    {
        var memoryCacheMock = new Mock<IMemoryCache>();
        memoryCacheMock
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny!))
            .Returns((object key, out object value) =>
            {
                if (_cacheStore.TryGetValue(key, out var foundValue))
                {
                    value = foundValue;
                    return true;
                }
                value = null!;
                return false;
            });

        memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns((object key) =>
            {
                var mockEntry = new Mock<ICacheEntry>();
                mockEntry.SetupAllProperties();
                mockEntry
                    .Setup(e => e.Dispose())
                    .Callback(() =>
                    {
                        if (mockEntry.Object.Value != null)
                        {
                            _cacheStore[key] = mockEntry.Object.Value;
                        }
                    });

                return mockEntry.Object;
            });

        return memoryCacheMock;
    }

    private HttpClient CreateSlashDefaultClient()
    {
        var defaultHttpMessageHandler = new MockHttpMessageHandler(request =>
        {
            if ("/keys".Equals(request.RequestUri?.AbsolutePath))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new List<PublicKeyInfo>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            PublicKey = _testKeys.Value<string>("publicKey")!,
                            ExpirationDate = DateTime.Now.AddDays(1)
                        }
                    }))
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        return new HttpClient(defaultHttpMessageHandler)
        {
            BaseAddress = _slashBaseUrl,
        };
    }

    private HttpClient CreateSlashDPoPClient()
    {
        var dpopHttpMessageHandler = new MockHttpMessageHandler(request =>
        {
            if ("/message".Equals(request.RequestUri?.AbsolutePath))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Headers =
                    {
                        { "X-Correlation-ID", Guid.NewGuid().ToString() }
                    },
                    Content = new StringContent(
                        JsonSerializer.Serialize(new { delivered = true }),
                        Encoding.UTF8,
                        "application/json"
                    )
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        return new HttpClient(dpopHttpMessageHandler)
        {
            BaseAddress = _slashBaseUrl,
        };
    }

    private HttpClient CreateHelseIdClient()
    {
        var helseIdHttpMessageHandler = new MockHttpMessageHandler(request =>
        {
            if ("/token".Equals(request.RequestUri?.AbsolutePath))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        access_token = _accessToken,
                        expires_in = 60,
                        token_type = "DPoP",
                        scope = "fhi:slash.mottak/all"
                    }), Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        return new HttpClient(helseIdHttpMessageHandler)
        {
            BaseAddress = _helseIdBaseUrl,
        };
    }
}

public class MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handlerFunc) : DelegatingHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handlerFunc = handlerFunc ??
        throw new ArgumentNullException(nameof(handlerFunc));

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_handlerFunc(request));
    }
}
