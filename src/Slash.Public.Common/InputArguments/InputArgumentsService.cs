using Slash.Public.Common.CustomConsole;

namespace Slash.Public.Common.InputArguements;

public static class InputArgumentsService
{
    public static T CreateInputArguments<T>(string[] args) where T : new()
    {
        var arguments = new T();

        // Process input arguments
        var argumentsKeyValue = GetKeyValueOfArguments(args);
        foreach (var argument in argumentsKeyValue)
        {
            SetPropertyValueByKey<T>(arguments, argument.Key, argument.Value);
        }

        // Request input for arugments
        RequestInputForEmptyArguments(arguments);

        // Validate required arguments
        if (!ValidateRequiredArguments(arguments))
        {
            ConsoleServiceBase.PrintError("*** Exiting program due to missing required arguments.");
            ConsoleServiceBase.PrintNewLine();
            ConsoleServiceBase.PressAnyKeyToExit(exitCode: 1);
        }

        return arguments;
    }

    private static void RequestInputForEmptyArguments<T>(T obj)
    {
        var requestInputArguments = typeof(T).GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(InputArgumentAttribute), false)
                .Any(a => ((InputArgumentAttribute)a).RequestValue) && string.IsNullOrEmpty(p.GetValue(obj)?.ToString()));

        foreach (var property in requestInputArguments)
        {
            var inputArgumentProperty = property.GetCustomAttributes(typeof(InputArgumentAttribute), false);
            var inputType = inputArgumentProperty.Select(a => ((InputArgumentAttribute)a).RequestInputType).First();
            var propertyDesc = inputArgumentProperty.Select(a => ((InputArgumentAttribute)a).Description).First();
            var defaultValue = inputArgumentProperty.Select(a => ((InputArgumentAttribute)a).RequestDefaultValue).First();

            var value = string.Empty;
            while (string.IsNullOrEmpty(value))
            {
                switch (inputType)
                {
                    case RequestInputType.Text:
                        value = ConsoleServiceBase.RequestInput($"{propertyDesc}:", defaultValue);
                        break;
                    case RequestInputType.FilePath:
                        var isInputFile = inputArgumentProperty.Select(a => ((InputArgumentAttribute)a).RequestIsInput).First();
                        var expectedExtension = inputArgumentProperty.Select(a => ((InputArgumentAttribute)a).RequestInputFileExtension).First();
                        value = ConsoleServiceBase.RequestFilePath($"{propertyDesc}:", isInputFile, expectedExtension, defaultValue);
                        break;
                    case RequestInputType.DirectoryPath:
                        var isInputDir = inputArgumentProperty.Select(a => ((InputArgumentAttribute)a).RequestIsInput).First();
                        value = ConsoleServiceBase.RequestDirectoryPath($"{propertyDesc}:", isInputDir, defaultValue);
                        break;
                    default:
                        throw new Exception("Invalid configuration of input arguments. Contact support");
                }
            }

            property.SetValue(obj, value);
        }
    }

    private static bool ValidateRequiredArguments<T>(T obj)
    {
        var requiredArguments = typeof(T).GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(InputArgumentAttribute), false)
                .Any(a => ((InputArgumentAttribute)a).Required));

        var isValid = true;
        foreach (var property in requiredArguments)
        {
            var value = property.GetValue(obj);
            if (value == null)
            {
                ConsoleServiceBase.PrintError("Missing required argument: " + property.Name);
                ConsoleServiceBase.PrintError(" Input keys: " + string.Join(" | ", ((InputArgumentAttribute)property.GetCustomAttributes(typeof(InputArgumentAttribute), false).First()).InputKeys));
                ConsoleServiceBase.PrintError(" Description: " + ((InputArgumentAttribute)property.GetCustomAttributes(typeof(InputArgumentAttribute), false).First()).Description);
                ConsoleServiceBase.PrintNewLine();
                isValid = false;
            }
        }

        return isValid;
    }

    private static void SetPropertyValueByKey<T>(T obj, string key, dynamic value)
    {
        var property = typeof(T).GetProperties()
            .FirstOrDefault(p => 
                p.GetCustomAttributes(typeof(InputArgumentAttribute), false)
                 .Any(a => ((InputArgumentAttribute)a).InputKeys.Contains(key)));
        if (property == null)
        {
            ConsoleServiceBase.PrintWarning("Invalid argument: " + key);
            ConsoleServiceBase.PrintNewLine();
            return;
        }

        try
        {
            property.SetValue(obj, value);
        }
        catch
        {
            ConsoleServiceBase.PrintError($"Invalid value for property {property.Name} ({key})");
            ConsoleServiceBase.PrintNewLine();
        }
    }

    private static Dictionary<string, dynamic> GetKeyValueOfArguments(string[] args)
    {
        var keyValueDict = new Dictionary<string, dynamic>();
        for (var i = 0; i < args.Length; i++)
        {
            var key = args[i].ToLowerInvariant();
            if (!key.StartsWith('-'))
            {
                continue;
            }

            dynamic? value = args.Length > (i + 1) ? args[i + 1] : null;
            if (string.IsNullOrEmpty(value) || value!.StartsWith('-'))
            {
                value = true;
            }

            keyValueDict.Add(key, value);
        }

        return keyValueDict;
    }
}
