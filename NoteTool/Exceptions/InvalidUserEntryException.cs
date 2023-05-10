namespace NoteTool.Exceptions;

public class InvalidUserEntryException : Exception
{
    public InvalidUserEntryException(string message)
        : base(message)
    {
    }
}