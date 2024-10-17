namespace Slash.Public.Common.CustomConsole;

internal static class ConsoleValidationService
{
    public static bool FilePathIsValidWithExtension(string path, string extension, bool fileShouldExist, out string? error)
    {
        if (FilePathIsValid(path, fileShouldExist, out error) &&
            !path.EndsWith($".{extension}", StringComparison.OrdinalIgnoreCase))
        {
            error = $"The file extension is not expected. The file type should be .{extension}";
        }

        return string.IsNullOrEmpty(error);
    }

    public static bool DirectoryPathIsValid(string path, bool directoryShouldExist, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(path))
        {
            error = "Invalid path";
        }
        else if (!Uri.TryCreate(path, UriKind.Absolute, out Uri? uri))
        {
            error = "Path is not well formatted";
        }
        else if (directoryShouldExist && !Directory.Exists(uri.LocalPath))
        {
            error = "Directory does not exist";
        }

        return string.IsNullOrEmpty(error);
    }

    public static bool FilePathIsValid(string path, bool fileShouldExist, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(path))
        {
            error = "Invalid path";
        }
        else if (!Uri.TryCreate(path, UriKind.Absolute, out Uri? uri))
        {
            error = "Path is not well formatted";
        }
        else if (!uri.IsFile)
        {
            error = "Path is not a path for file";
        }
        else if (fileShouldExist && !File.Exists(path))
        {
            error = "File does not exist";
        }

        return string.IsNullOrEmpty(error);
    }
}
