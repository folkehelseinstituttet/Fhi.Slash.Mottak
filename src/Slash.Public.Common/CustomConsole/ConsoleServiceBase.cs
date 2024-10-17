using Spectre.Console;

namespace Slash.Public.Common.CustomConsole;

public abstract class ConsoleServiceBase
{
    public static string RequestInput(string text, string? defaultValue = null) =>
        defaultValue == null ?
            AnsiConsole.Ask<string>(SetTextColorCyan(text)) :
            AnsiConsole.Ask(SetTextColorCyan(text), defaultValue);

    public static string RequestFilePath(string text, bool isInput, string? extension, string? defaultValue = null)
    {
        var input = RequestInput(text, defaultValue);

        // Remove quotes if they exist
        if (input.StartsWith('"')) { input = input[1..]; }
        if (input.EndsWith('"')) { input = input[..^1]; }

        var failed = extension == null ?
            !ConsoleValidationService.FilePathIsValid(input, fileShouldExist: isInput, out string? errorMsg) :
            !ConsoleValidationService.FilePathIsValidWithExtension(input, extension!, fileShouldExist: isInput, out errorMsg);

        if (failed)
        {
            PrintError(errorMsg!);
            return RequestFilePath(text, isInput, extension);
        }

        PrintNewLine();
        return input;
    }

    public static string RequestDirectoryPath(string text, bool isInput, string? defaultValue = null)
    {
        var input = RequestInput(text, defaultValue);

        if (!ConsoleValidationService.DirectoryPathIsValid(input, directoryShouldExist: isInput, out string? error))
        {
            PrintError(error!);
            return RequestDirectoryPath(text, isInput);
        }

        PrintNewLine();
        return input;
    }

    public static void PrintTitle(string title)
    {
        var rule = new Rule(title)
        {
            Justification = Justify.Left,
        };

        AnsiConsole.Write(rule);
        PrintNewLine();
    }

    public static void PrintProcess(string process) =>
       PrintText($"{SetTextColorCyan("Started Process")}: {process}");

    public static void PrintProcessDone(string? comment = null, bool newLine = true)
    {
        var commentStr = string.IsNullOrEmpty(comment) ? string.Empty : $" {comment}";
        PrintText($"{SetTextColorGreen("DONE")}{commentStr}");
        if (newLine)
        {
            PrintNewLine();
        }
    }

    public static void PrintNewLine() =>
       Console.WriteLine();

    public static void PressAnyKeyToContinue()
    {
        Console.WriteLine($"Press any key to continue ...");
        Console.ReadKey(true);
    }

    public static void PressAnyKeyToExit(int exitCode = 0)
    {
        Console.WriteLine($"Press any key to exit program ...");
        Console.ReadKey(true);
        Environment.Exit(exitCode);
    }

    public static void PrintLines(string[] lines) =>
        PrintText(string.Join("\r\n", lines));

    public static void PrintException(Exception ex) =>
        AnsiConsole.WriteException(ex);

    public static void PrintError(string error) =>
       PrintText(error, ConsoleColor.Red);

    public static void PrintWarning(string warning) =>
        PrintText(warning, ConsoleColor.Yellow);

    public static void PrintText(string text, ConsoleColor color = ConsoleColor.White) =>
        AnsiConsole.MarkupLine(SetTextColor(text, color));

    public static string SetTextColorCyan(string text) =>
        SetTextColor(text, ConsoleColor.Cyan);

    public static string SetTextColorMagenta(string text) =>
        SetTextColor(text, ConsoleColor.Magenta);

    public static string SetTextColorGreen(string text) =>
        SetTextColor(text, ConsoleColor.Green);

    public static string SetTextColorRed(string text) =>
        SetTextColor(text, ConsoleColor.Red);

    public static string SetTextColor(string text, ConsoleColor color) =>
        $"[{Enum.GetName(color)}]{text}[/]";
}
