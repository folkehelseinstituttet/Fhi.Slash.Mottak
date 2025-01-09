using System.Text.Json.Serialization;

namespace Slash.Public.SlashMessenger.Slash.Models;

/// <summary>
/// Represents the payload of the response from the Slash API after processing a message.
/// </summary>
public class ProcessMessageResponse
{
    /// <summary>
    /// Indicates whether the message was successfully delivered to the target system.
    /// </summary>
    [JsonPropertyName("delivered")]
    public bool Delivered { get; set; }

    /// <summary>
    /// A list of errors that occurred during message processing, if any.
    /// </summary>
    [JsonPropertyName("errors")]
    public List<ProcessMessageResponseError> Errors { get; set; } = [];
}
