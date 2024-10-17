using Microsoft.Extensions.Configuration;
using Slash.Public.APIMessengerCLI.CustomConsole;
using Slash.Public.APIMessenger.Services;
using Slash.Public.Common.CustomConsole;
using Slash.Public.Common.InputArguements;
using Slash.Public.APIMessenger.Config;
using Slash.Public.APIMessenger.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace Slash.Public.APIMessengerCLI;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup Config
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var senderConfig = configuration.GetConfig<SenderConfig>(sectionName: "Sender") ??
            throw new Exception("Sender configuration is missing");
        var slashAPIConfig = configuration.GetConfig<SlashAPIConfig>(sectionName: "SlashAPI") ??
            throw new Exception("SlashAPI configuration is missing");
        var helseIdConfig = configuration.GetConfig<HelseIdConfig>(sectionName: "HelseId") ??
            throw new Exception("HelseId configuration is missing");

        // Setup SlashMessengerService
        var service = new SlashMessengerService(senderConfig, slashAPIConfig, helseIdConfig);

        // Process Input Arguments (Message File Path, Message Type, Message Version)
        var inputArguments = InputArgumentsService.CreateInputArguments<InputArguments.InputArguments>(args);
        if (inputArguments.Help)
        {
            ConsoleService.PrintHelp();
            ConsoleServiceBase.PrintNewLine();
            ConsoleServiceBase.PressAnyKeyToExit(0);
        }

        // Load Private Jwk for DPoP Proof
        JsonWebKey? dPopJwk = null;
        if (!string.IsNullOrEmpty(inputArguments.PrivateJwkFilePath))
        {
            var rawPrivateJwk = await File.ReadAllTextAsync(inputArguments.PrivateJwkFilePath);
            dPopJwk = new JsonWebKey(rawPrivateJwk);
        }

        // Send Message
        HttpResponseMessage? httpResponse = null;
        string? responseMessage = null;
        try
        {
            var rawJsonMessage = await File.ReadAllTextAsync(inputArguments.MessageFilePath);
            httpResponse = await service.SendMessage(rawJsonMessage, inputArguments.MessageType, inputArguments.MessageVersion, dPopJwk);
            responseMessage = await httpResponse.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            ConsoleServiceBase.PrintError("Could not send message");
            ConsoleServiceBase.PrintException(ex);
            ConsoleServiceBase.PrintNewLine();
            ConsoleServiceBase.PressAnyKeyToExit();
        }

        // Print Response
        ConsoleServiceBase.PrintTitle("HTTP Reponse");
        ConsoleServiceBase.PrintLines([
            $"StatusCode: {httpResponse!.StatusCode}",
            "Response: "
        ]);
        Console.WriteLine(responseMessage);
        ConsoleServiceBase.PrintNewLine();
        ConsoleServiceBase.PressAnyKeyToExit();
    }
}
