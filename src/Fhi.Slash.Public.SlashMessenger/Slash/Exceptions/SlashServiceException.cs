namespace Fhi.Slash.Public.SlashMessenger.Slash.Exceptions;

public class SlashServiceException : Exception
{
    public SlashServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public SlashServiceException(string message) : base(message)
    {
    }
}
