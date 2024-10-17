using Slash.Public.Common.InputArguements;

namespace Slash.Public.APIMessengerCLI.InputArguments;

internal class InputArguments
{
    [InputArgument(
        inputKeys: ["--help", "-h"],
        description: "Show help")]
    public bool Help { get; set; }

    [InputArgument(
        inputKeys: ["--messageFilePath", "-mfp"],
        required: true,
        requestValue: true,
        requestInputFileExtension: "json",
        requestIsInput: true,
        requestInputType: RequestInputType.FilePath,
        description: "Full path to the message file")]
    public string MessageFilePath { get; set; } = null!;

    [InputArgument(
        inputKeys: ["--messageType", "-mt"],
        required: true,
        requestValue: true,
        requestInputType: RequestInputType.Text,
        description: "Message type")]
    public string MessageType { get; set; } = null!;

    [InputArgument(
        inputKeys: ["--messageVersion", "-mv"],
        required: true,
        requestValue: true,
        requestInputType: RequestInputType.Text,
        description: "Message version")]
    public string MessageVersion { get; set; } = null!;

    [InputArgument(
    inputKeys: ["--privateJwkFilePath", "-pjfp"],
    required: false,
    requestValue: false,
    requestInputType: RequestInputType.Text,
    description: "Jwk to use for the DPoP Proof. (Jwk from HelseId Client will be used if not provided)")]
    public string? PrivateJwkFilePath { get; set; } = null!;
}
