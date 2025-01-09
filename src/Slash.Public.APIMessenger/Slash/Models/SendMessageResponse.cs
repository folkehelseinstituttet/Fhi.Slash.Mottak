namespace Slash.Public.SlashMessenger.Slash.Models;

/// <summary>
/// Represents the response from the Slash API after sending a message.
/// </summary>
public class SendMessageResponse
{
    /// <summary>
    /// The correlation ID associated with the sent message.
    /// </summary>
    public required Guid CorrelationId { get; set; }

    /// <summary>
    /// The parsed response from the Slash API, providing details about the processed message.
    /// </summary>
    public required ProcessMessageResponse ProcessMessageResponse { get; set; }

    /// <summary>
    /// The raw HTTP response from the Slash API.
    /// </summary>
    public required HttpResponseMessage ResposeMessage { get; set; }
}
