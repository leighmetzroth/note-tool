namespace NoteTool.Exceptions;

public class InvalidSettingException : Exception
{
    public InvalidSettingException(string settingName)
        : base($"The setting {settingName} is not valid")
    {
    }
}