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
                    Console.WriteLine($"> {i}.{options[i]} <");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {i}.{options[i]}  ");
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

    private static bool VisualBoolMenu(string title)
    {
        var choice = VisualMenu(["Yes", "No"], title);
        return choice == "Yes";
    }
    private static bool VisualBoolOnMenu(string title)
    {
        var choice = VisualMenu(["On", "Off"], title);
        return choice == "On";
    }

    public static void ConfigMenu(string configPath)
    {
        Console.Clear();
        
        var choice = VisualBoolMenu("Do you want to change the config now?");

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
        var logFile = $"logs-{DateTime.Now:yyyy-MM-dd}-v.{config.Version}.txt";
        
        Console.Clear();
        
        List<string> possibleChoices = [
            "Continue",
            "Show translated strings",
            "Debug Mode",
            "Save and Exit",
            "Exit without saving"];

        if (config.DebugMode)
        {
            possibleChoices.AddRange([
                "Delete config file",
                "Show original nodes",
                "Override last file"]);
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
                config.ShowLoadedStrings = VisualBoolOnMenu($"Show translated strings: {(config.ShowLoadedStrings ? "[ON]" : "[OFF]")}");
                break;
            case "Debug Mode":
                config.DebugMode = VisualBoolOnMenu($"Debug Mode: {(config.DebugMode ? "[ON]" : "[OFF]")}");
                break;
            case "Save and Exit":
                config.FirstTimeUsing = true;
                Config.Write(configPath, config);
                Environment.Exit(0);
                return;
            case "Exit without saving":
                settingChoice = VisualBoolMenu("Are you sure you want to exit?");
                if (!settingChoice)
                    break;
                Environment.Exit(0);
                break;
            case "Delete config file":
                settingChoice = VisualBoolMenu("Are you sure you want to delete the config file?" +
                                               "\nThis will end the program.");
                if (!settingChoice)
                    break;
                File.Delete(configPath); Environment.Exit(0);
                break;
            case "Show original nodes":
                config.ShowOriginalNodes = VisualBoolOnMenu($"Show original nodes: {(config.ShowOriginalNodes ? "[ON]" : "[OFF]")}");
                break;
            case "Override last file":
                settingChoice = VisualBoolMenu("Override last file?");
                if (!settingChoice)
                    break;

                var filePath = ChangeFilePath();
                
                config.LastFile = filePath;
                break;
        }
        // ReSharper disable once TailRecursiveCall
        SettingsMenu(configPath, config);
    }
    
    private static string ChangeFilePath()
    {
        Console.WriteLine("Enter path to config file:");
        return Console.ReadLine()!;
    }

    public static void StartMenu(string configPath, Config config, string filePath)
    {
        Console.Clear();

        PrintAsciiLogo();

        string[] choices =
        [
            "Continue",
            "Settings",
            "Change file path",
            "Exit"
        ];
        
        if (config.FirstTimeUsing)
            choices[0] = "Start";
        
        var choice = VisualMenu(choices, "Start Menu");
        
        switch (choice)
        {
            case "Start" or "Continue":
                Program.StartTranslation(filePath, config);
                break;
            case "Settings":
                SettingsMenu(configPath, config);
                break;
            case "Change file path":
                filePath = ChangeFilePath();
                config.LastFile = filePath;
                Config.Write(configPath, config);
                break;
            case "Exit":
                Environment.Exit(0);
                break;
        }
    }
}