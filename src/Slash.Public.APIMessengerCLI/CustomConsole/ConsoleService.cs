using Slash.Public.Common.InputArguements;
using Slash.Public.Common.CustomConsole;

namespace Slash.Public.APIMessengerCLI.CustomConsole;

public class ConsoleService : ConsoleServiceBase
{
    public static void PrintHelp() {
        var possibleArguments = typeof(InputArguments.InputArguments).GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(InputArgumentAttribute), false).Length != 0)
            .Select(p => (InputArgumentAttribute)p.GetCustomAttributes(typeof(InputArgumentAttribute), false).First())
            .ToList();

        var argumentLines = possibleArguments.Select(a =>
        {
            var keys = a.InputKeys[0];
            for (var i = 1; i < a.InputKeys.Length; i++)
            {
                if (i == 1) { keys += " ("; }
                if (i > 1) { keys += " | "; }
                keys += a.InputKeys[i];
            }
            if (a.InputKeys.Length > 1)
            {
                keys += ")";
            }

            return $"{(a.Required ? $"{SetTextColorRed("*")} " : "  ")}{keys}: {a.Description}";
        });

        var helpLines = new List<string> {
            $"{SetTextColorCyan("HELP")}",
            string.Empty,
            $"You may use the following arguments ({SetTextColorRed("*")} = required):",
        };

        helpLines.AddRange(argumentLines);

        helpLines.AddRange(new List<string>
        {
            string.Empty,
            "You may update the settings in the appsettings.json file."
        });

        PrintLines([.. helpLines]);
    } 
}
