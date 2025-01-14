namespace Fhi.Slash.Public.SlashMessenger.HelseId.Exceptions;

public class HelseIdClientException : Exception
{
    public HelseIdClientException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public HelseIdClientException(string message) : base(message)
    {
    }
}
