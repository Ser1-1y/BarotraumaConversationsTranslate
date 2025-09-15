namespace Translate;

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
    
    /// <summary>
    /// Presents a user with a graphic selection of some options.
    /// </summary>
    /// <param name="options">String array of options displayed to the user.</param>
    /// <param name="title">Optional title for the options.</param>
    /// <returns>String of a selected option</returns>
    private static string VisualMenu(string[] options, string title = "")
    {
        var selected = 0;
        ConsoleKey key;

        var (left, top) = Console.GetCursorPosition();
        Console.CursorVisible = false;
        
        do
        {
            Console.SetCursorPosition(left, top);
            
            if (title != "")
                Console.Write(title + "\n");
            
            for (var i = 0; i < options.Length; i++)
            {
                if (i == selected)
                {
                    Console.BackgroundColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine($"> {i}. {options[i]} <");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {i}. {options[i]}  ");
                }
            }

            var keyInfo = Console.ReadKey(true);
            key = keyInfo.Key;

            selected = key switch
            {
                ConsoleKey.UpArrow or ConsoleKey.LeftArrow => (selected - 1 + options.Length) % options.Length,
                ConsoleKey.DownArrow or ConsoleKey.RightArrow => (selected + 1) % options.Length,
                _ => selected
            };
        } while (key != ConsoleKey.Enter);

        Console.CursorVisible = true;
        Console.Clear();
        return options[selected];
    }

    private static bool VisualYesNoMenu(string title)
    {
        var choice = VisualMenu(["Yes", "No"], title);
        return choice == "Yes";
    }
    private static bool VisualOnOffMenu(string title)
    {
        var choice = VisualMenu(["On", "Off"], title);
        return choice == "On";
    }

    public static void ConfigMenu(string configPath)
    {
        Console.Clear();
        
        Color.Write("<=Red>No config file found</>\n");
        var choice = VisualYesNoMenu("Do you want to change the config now?");

        var config = new Config();
        if (choice)
            SettingsMenu(configPath, config);
        else
        {
            Config.Write(configPath, config);
        }
    }

    public static void SettingsMenu(string configPath, Config config)
    {
        
        Console.Clear();
        
        List<string> possibleChoices = [
            "Continue",
            "Show translated strings",
            "Change File path",
            "Debug Mode",
            "Save and Exit",
            "Exit without saving"];

        if (config.DebugMode)
        {
            possibleChoices.AddRange([
                "Delete config file",
                "Show original nodes"]);
        }

        var choice = VisualMenu(possibleChoices.ToArray(), $"Settings v{config.Version}");

        bool settingChoice;
        
        switch (choice)
        {
            case "Continue":
                Config.Write(configPath, config);
                Program.Main();
                return;
            case "Show translated strings":
                config.ShowLoadedStrings = VisualOnOffMenu($"Show translated strings: {(config.ShowLoadedStrings ? "[ON]" : "[OFF]")}");
                break;
            case "Change File path":
                ChangeFilePath(configPath, config, false);
                break;
            case "Debug Mode":
                config.DebugMode = VisualOnOffMenu($"Debug Mode: {(config.DebugMode ? "[ON]" : "[OFF]")}");
                break;
            case "Save and Exit":
                config.FirstTimeUsing = true;
                Config.Write(configPath, config);
                Environment.Exit(0);
                return;
            case "Exit without saving":
                settingChoice = VisualYesNoMenu("Are you sure you want to exit?");
                if (!settingChoice)
                    break;
                Environment.Exit(0);
                break;
            case "Delete config file":
                settingChoice = VisualYesNoMenu("Are you sure you want to delete the config file?" +
                                               "\nThis will restart the program.");
                if (!settingChoice)
                    break;
                File.Delete(configPath); 
                Environment.Exit(0);
                Program.Main();
                break;
            case "Show original nodes":
                config.ShowOriginalNodes = VisualOnOffMenu($"Show original nodes: {(config.ShowOriginalNodes ? "[ON]" : "[OFF]")}");
                break;
        }
        // ReSharper disable once TailRecursiveCall
        SettingsMenu(configPath, config);
    }

    private static void ChangeFilePath(string configPath, Config config, bool start)
    {
        Console.Clear();
        
        Console.WriteLine("Enter path to the XML file:");
        var newFilePath = Console.ReadLine();
        config.LastFile = newFilePath;
        Config.Write(configPath, config);
        if (start)
            StartMenu(configPath, config);
    }
    
    public static string? ChangeFilePath()
    {
        Console.Clear();
        
        Console.WriteLine("Enter path to the XML file:");
        var newFilePath = Console.ReadLine();
        return newFilePath;
    }

    public static void StartMenu(string configPath, Config config)
    {
        Console.Clear();

        PrintAsciiLogo();

        string[] choices =
        [
            "Continue",
            "Settings",
            "Change File path",
            "Help",
            "Exit"
        ];
        
        if (config.FirstTimeUsing)
            choices[0] = "Start";
        
        var choice = VisualMenu(choices, "Start Menu");

        switch (choice)
        {
            case "Start" or "Continue":
                var filePath = Helpers.GetFilePath(config, configPath);
                Translation.Process(filePath, config, configPath);
                break;
            case "Settings":
                SettingsMenu(configPath, config);
                break;
            case "Change File path":
                ChangeFilePath(configPath, config, true);
                break;
            case "Help":
                Console.WriteLine("Press 'Escape' to exit while translating.\n" +
                                  "Write 'Settings' to access Settings.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                // ReSharper disable once TailRecursiveCall
                StartMenu(configPath, config);
                break;
            case "Exit":
                var exitChoice = VisualYesNoMenu("Are you sure you want to exit?");
                if (exitChoice)
                    Environment.Exit(0);
                // ReSharper disable once TailRecursiveCall
                StartMenu(configPath, config);
                break;
        }
    }

    public static bool ExitMenu()
    {
        Console.Clear();
        
        return VisualYesNoMenu("Are you sure you want to exit?");
    }
}