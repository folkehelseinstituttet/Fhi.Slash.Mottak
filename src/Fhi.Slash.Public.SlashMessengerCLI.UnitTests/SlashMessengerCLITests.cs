using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Fhi.Slash.Public.SlashMessenger.Slash.Interfaces;
using Fhi.Slash.Public.SlashMessenger.Slash.Models;
using Fhi.Slash.Public.SlashMessengerCLI.Config;
using System.Security.Cryptography;
using NSubstitute;

namespace Fhi.Slash.Public.SlashMessengerCLI.UnitTests;

[TestClass]
public class SlashMessengerCLITests
{
    private const string certWithoutPasswordPath = "TestFiles/Certs/test_cert_without_password.pfx";
    private const string certWithPasswordPath = "TestFiles/Certs/test_cert_with_password.pfx";
    private readonly AppsettingsConfig defaultConfig = new()
    {
        HelseIdCertificateThumbprint = string.Empty,
        HelseIdCertificatePath = string.Empty,
        HelseIdCertificatePassword = string.Empty,
        HelseIdOpenIdConfigurationUrl = string.Empty,
        HelseIdClientJsonFilePath = string.Empty,
        SenderExportSoftwareVersion = string.Empty,
        SenderSoftwareVersion = string.Empty,
        SenderSoftwareName = string.Empty,
        SenderVendorName = string.Empty,
        HelseIdClientId = string.Empty,
        SlashBaseUrl = string.Empty,
    };

    [TestMethod]
    public async Task ShouldSendMessage()
    {
        // Arrange
        var slashService = Substitute.For<ISlashService>();
        slashService.PrepareAndSendMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(new SendMessageResponse()
            {
                CorrelationId = Guid.Empty,
                ProcessMessageResponse = new ProcessMessageResponse()
                {
                    Delivered = false,
                    Errors =
                    [
                        new()
                        {
                            ErrorCode = 0,
                            ErrorDetails = "TestDetails 1",
                            ErrorMessage = "TestMessage 1",
                            PropertyName = "TestProperty 1"
                        },
                        new()
                        {
                            ErrorCode = 1,
                            ErrorDetails = "TestDetails 2",
                            ErrorMessage = "TestMessage 2",
                            PropertyName = "TestProperty 2"
                        }
                    ]
                },
                ResposeMessage = new HttpResponseMessage()
            });

        var testHost = new HostBuilder()
            .ConfigureServices((ctx, services) =>
            {
                services.AddTransient(_ => slashService);
            })
            .Build();

        var msgType = "testType";
        var msgVersion = "testVersion";
        var filename = "testfile.txt";
        var fileContent = "Test message content";
        File.WriteAllText(filename, fileContent); // Create a temporary file message file

        // Act
        await Program.Execute(testHost, filename, msgType, msgVersion);

        // Assert
        await slashService.Received(1).PrepareAndSendMessage(fileContent, msgType, msgVersion);

        // Cleanup
        File.Delete(filename);
    }

    [TestMethod]
    public void ShouldTryToGetCertFromCertStoreIfThumbprintProvided()
    {
        // Arrange
        defaultConfig.HelseIdCertificateThumbprint = "non-existent-thumbprint";

        // Act
        var act = () => Program.GetCertificateByHelseIdConfig(defaultConfig);

        // Assert
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("No valid certfication was found for: onlyValid=False and FindByThumbprint=non-existent-thumbprint");
    }

    [TestMethod]
    public void ShouldTryToGetCertFromFileIfCertPathProvided_WithoutPassword()
    {
        // Arrange
        defaultConfig.HelseIdCertificatePath = certWithoutPasswordPath;
        defaultConfig.HelseIdCertificatePassword = string.Empty;

        // Act
        var cert = Program.GetCertificateByHelseIdConfig(defaultConfig);

        // Assert
        cert.Should().NotBeNull();
        cert!.Subject.Should().Contain("Just-For-Testing");
    }

    [TestMethod]
    public void ShouldTryToGetCertFromFileIfCertPathProvided_WithPassword()
    {
        // Arrange
        defaultConfig.HelseIdCertificatePath = certWithPasswordPath;
        defaultConfig.HelseIdCertificatePassword = "password";

        // Act
        var cert = Program.GetCertificateByHelseIdConfig(defaultConfig);

        // Assert
        cert.Should().NotBeNull();
        cert!.Subject.Should().Contain("Just-For-Testing");
    }

    [TestMethod]
    public void ShouldThrowExceptionIfCertNotFoundByPath()
    {
        // Arrange
        defaultConfig.HelseIdCertificatePath = "TestFiles/Certs/THIS_CERT_DOES_NOT_EXIST.pfx";

        // Act
        var act = () => Program.GetCertificateByHelseIdConfig(defaultConfig);

        // Assert
        act.Should().Throw<CryptographicException>()
            .WithMessage("The system cannot find the file specified.");
    }

    [TestMethod]
    public void ShouldThrowExceptionIfCertFoundByPathWithInvalidPassword()
    {
        // Arrange
        defaultConfig.HelseIdCertificatePath = certWithPasswordPath;
        defaultConfig.HelseIdCertificatePassword = "NOT_VALID_PASSWORD";

        // Act
        var act = () => Program.GetCertificateByHelseIdConfig(defaultConfig);

        // Assert
        act.Should().Throw<CryptographicException>()
            .WithMessage("The specified network password is not correct.");
    }
}