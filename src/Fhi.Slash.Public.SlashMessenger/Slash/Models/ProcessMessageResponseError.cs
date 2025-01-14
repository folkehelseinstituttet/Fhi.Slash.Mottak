using System.Text.Json.Serialization;

namespace Fhi.Slash.Public.SlashMessenger.Slash.Models;

/// <summary>
/// Represents an error that occurred during the processing of a message.
/// </summary>
public class ProcessMessageResponseError
{
    /// <summary>
    /// The error code indicating the type of error that occurred during message processing.
    /// </summary>
    [JsonPropertyName("errorCode")]
    public int? ErrorCode { get; set; }

    /// <summary>
    /// The name of the property that caused the error during message processing.
    /// </summary>
    [JsonPropertyName("propertyName")]
    public string? PropertyName { get; set; }

    /// <summary>
    /// A descriptive error message providing details about the encountered issue.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional details regarding the error, offering further context or explanations.
    /// </summary>
    [JsonPropertyName("errorDetails")]
    public string? ErrorDetails { get; set; }
}
