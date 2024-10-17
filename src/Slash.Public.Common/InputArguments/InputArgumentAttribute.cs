namespace Slash.Public.Common.InputArguements;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class InputArgumentAttribute(
    string[] inputKeys,
    string description,
    bool required = false,
    bool requestValue = false,
    bool requestIsInput = false,
    string? requestInputFileExtension = null,
    string? requestDefaultValue = null,
    RequestInputType requestInputType = RequestInputType.Unknown) : Attribute
{
    // Keys to be used in CLI for arugment.
    public string[] InputKeys { get; } = inputKeys;

    // If a value is required for this property.
    public bool Required { get; } = required;

    // If the program should request a value for this property.
    public bool RequestValue { get; } = requestValue;

    // If the expected value is an "input value".
    // If this is true, the program expect that the input file/directory exists.
    public bool RequestIsInput { get; } = requestIsInput;

    // Expected file extension the the input file.
    public string? RequestInputFileExtension { get; } = requestInputFileExtension;

    // Default value used when requesting input value.
    public string? RequestDefaultValue { get; } = requestDefaultValue;

    // Type of input expected.
    // Used to validate the input value to ensure that the value is a directory path for instance.
    public RequestInputType RequestInputType { get; } = requestInputType;

    // Description of the value
    public string Description { get; } = description;
}

public enum RequestInputType
{
    Unknown = 0,
    Text = 1,
    FilePath = 2,
    DirectoryPath = 3,
}