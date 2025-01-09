namespace Slash.Public.SlashMessenger.Slash.Exceptions;

public class SlashClientException : Exception
{
    public SlashClientException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public SlashClientException(string message) : base(message)
    {
    }
}
