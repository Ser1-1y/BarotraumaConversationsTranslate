namespace Translate;

public enum MenuResult
{
    Continue,
    Exit
}

public static class Menu
{
    private static void PrintAsciiLogo()
    {
        Console.WriteLine("""
                          ┌───────────────────────────────────────────────────────────────────────────────────────┐
                          │                                                                                       │
                          │   ─────┬─────           ────────┐  ┌───────┐  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐  │
                          │        │       ───────      ────┤  │       │  │      │  │      │  │      │  │      │  │
                          │        │                ────────┘  └───────┘  └──────┘  └──────┘  └──────┘  └──────┘  │
                          │                                                                                       │
                          └───────────────────────────────────────────────────────────────────────────────────────┘
                          """);
    }

    private static string VisualMenu(string[] options, string title = "")
    {
        var selected = 0;
        var (left, top) = Console.GetCursorPosition();
        Console.CursorVisible = false;

        while (true)
        {
            Console.SetCursorPosition(left, top);
            if (!string.IsNullOrEmpty(title))
                Console.WriteLine(title);

            for (var i = 0; i < options.Length; i++)
            {
                if (i == selected)
                {
                    Console.BackgroundColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine($"> {options[i]} <");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {options[i]}   ");
                }
            }

            var keyInfo = Console.ReadKey(true);
            var key = keyInfo.Key;

            selected = key switch
            {
                ConsoleKey.UpArrow or ConsoleKey.LeftArrow => (selected - 1 + options.Length) % options.Length,
                ConsoleKey.DownArrow or ConsoleKey.RightArrow => (selected + 1) % options.Length,
                _ => selected
            };

            if (key == ConsoleKey.Enter)
                break;
        }

        Console.CursorVisible = true;
        Console.Clear();
        return options[selected];
    }

    private static bool VisualYesNoMenu(string title) =>
        VisualMenu(["Yes", "No"], title) == "Yes";

    private static bool VisualOnOffMenu(string title) =>
        VisualMenu(["On", "Off"], title) == "On";

    public static void ConfigMenu(string configPath)
    {
        Console.Clear();
        Color.Write("<=Red>No config file found</>\n");
        var choice = VisualYesNoMenu("Do you want to change the config now?");
        var config = new Config();
        if (choice)
            SettingsMenu(configPath, config);
        else
            Config.Write(configPath, config);
    }

    public static void SettingsMenu(string configPath, Config config)
    {
        var loop = true;
        while (loop)
        {
            Console.Clear();
            var choices = new List<string>
            {
                "Continue",
                "Show translated strings",
                "Change File path",
                "Debug Mode",
                "Save and Exit",
                "Exit without saving"
            };

            if (config.DebugMode)
            {
                choices.Add("Delete config file");
                choices.Add("Show original nodes");
            }

            var choice = VisualMenu(choices.ToArray(), $"Settings v{config.Version}");

            switch (choice)
            {
                case "Continue":
                    Config.Write(configPath, config);
                    loop = false;
                    break;
                case "Show translated strings":
                    config.ShowLoadedStrings = VisualOnOffMenu($"Show translated strings: {(config.ShowLoadedStrings ? "[ON]" : "[OFF]")}");
                    break;
                case "Change File path":
                    config.LastFile = ChangeFilePath(configPath, config, false);
                    break;
                case "Debug Mode":
                    config.DebugMode = VisualOnOffMenu($"Debug Mode: {(config.DebugMode ? "[ON]" : "[OFF]")}");
                    break;
                case "Save and Exit":
                    config.FirstTimeUsing = true;
                    Config.Write(configPath, config);
                    loop = false;
                    break;
                case "Exit without saving":
                    if (VisualYesNoMenu("Are you sure you want to exit?"))
                        loop = false;
                    break;
                case "Delete config file":
                    if (VisualYesNoMenu("Are you sure you want to delete the config file?\nThis will restart the program."))
                    {
                        File.Delete(configPath);
                        return;
                    }
                    break;
                case "Show original nodes":
                    config.ShowOriginalNodes = VisualOnOffMenu($"Show original nodes: {(config.ShowOriginalNodes ? "[ON]" : "[OFF]")}");
                    break;
            }
        }
    }

    private static string ChangeFilePath(string configPath, Config config, bool startMenuAfter)
    {
        Console.Clear();
        Console.WriteLine("Enter path to the XML file:");
        var newFilePath = Console.ReadLine() ?? "";
        config.LastFile = newFilePath;
        
        Config.Write(configPath, config);
        if (startMenuAfter)
            StartMenu(config);
        return newFilePath;
    }

    public static string ChangeFilePath()
    {
        Console.Clear();
        Console.WriteLine("Enter path to the XML file:");
        return Console.ReadLine() ?? "";
    }

    public static MenuResult StartMenu(Config config)
    {
        Console.Clear();
        PrintAsciiLogo();

        const string configPath = Program.ConfigPath;
        
        string[] choices =
        [
            config.FirstTimeUsing ? "Start" : "Continue",
            "Settings",
            "Change File path",
            "Help",
            "Exit"
        ];

        var choice = VisualMenu(choices, "Start Menu");

        switch (choice)
        {
            case "Start":
            case "Continue":
                var filePath = Helpers.GetFilePath(config, configPath);
                var result = Translation.Process(filePath, config, configPath);
                
                return result == TranslationResult.ExitRequested ? MenuResult.Exit : MenuResult.Continue;
            case "Settings":
                SettingsMenu(configPath, config);
                return MenuResult.Continue;
            case "Change File path":
                config.LastFile = ChangeFilePath(configPath, config, true);
                return MenuResult.Continue;
            case "Help":
                Console.WriteLine("Press 'Escape' to exit while translating.\n" +
                                  "Write 'Settings' to access Settings.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return MenuResult.Continue;
            case "Exit":
                return VisualYesNoMenu("Are you sure you want to exit?") ? MenuResult.Exit : MenuResult.Continue;
            default:
                return MenuResult.Continue;
        }
    }

    public static bool ExitMenu()
    {
        Console.Clear();
        return VisualYesNoMenu("Are you sure you want to exit?");
    }
}