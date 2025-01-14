namespace Fhi.Slash.Public.SlashMessenger.HelseId.Exceptions;

public class HelseIdServiceException : Exception
{
    public HelseIdServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public HelseIdServiceException(string message) : base(message)
    {
    }
}
