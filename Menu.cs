namespace Translate;

public enum MenuResult
{
    Continue,
    Exit,
}

public enum Bool
{
    Yes,
    No,
    On,
    Off,
}

public enum Settings
{
    Continue,
    ShowTranslated,
    ChangeFilePath,
    DebugMode,
    SaveAndExit,
    ExitWithoutSaving,
    DeleteConfigFile,
    ShowOriginalNodes,
}

public enum StartMenuAction
{
    Start,
    Continue,
    Settings,
    ChangeFilePath,
    Help,
    Exit,
}

public static class Menu
{
    private static readonly Tui.Option<Bool>[] BoolAction = [
        new("Yes", Bool.Yes), 
        new("No", Bool.No),
    ];
    
    private static readonly Tui.Option<Bool>[] OnOffAction = [
        new("On", Bool.On), 
        new("Off", Bool.Off),
    ];

    private static readonly Tui.Option<Settings>[] BaseSettingsActions = [
        new("Continue", Settings.Continue),
        new("Show Translated Strings", Settings.ShowTranslated),
        new("Change File Path", Settings.ChangeFilePath),
        new("Debug Mode", Settings.DebugMode),
        new("Save And Exit", Settings.SaveAndExit),
        new("Exit Without Saving", Settings.ExitWithoutSaving),
    ];

    private static bool VisualYesNoMenu(string title) =>
        Tui.Choice(BoolAction, title: title) == Bool.Yes;

    private static bool VisualOnOffMenu(string title) =>
        Tui.Choice(OnOffAction, title: title) == Bool.On;

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
            
            var choices = new List<Tui.Option<Settings>>(BaseSettingsActions);

            if (config.DebugMode)
            {
                choices.Add(new Tui.Option<Settings>("Delete Config File", Settings.DeleteConfigFile));
                choices.Add(new Tui.Option<Settings>("Show Original Nodes", Settings.ShowOriginalNodes));
            }

            var choice = Tui.Choice(choices.ToArray(), title: $"Settings v{config.Version}");

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (choice)
            {
                case Settings.Continue:
                    Config.Write(configPath, config);
                    loop = false;
                    break;
                case Settings.ShowTranslated:
                    config.ShowLoadedStrings = VisualOnOffMenu($"Show translated strings: {(config.ShowLoadedStrings ? "[ON]" : "[OFF]")}");
                    break;
                case Settings.ChangeFilePath:
                    config.LastFile = ChangeFilePath(configPath, config, false);
                    break;
                case Settings.DebugMode:
                    config.DebugMode = VisualOnOffMenu($"Debug Mode: {(config.DebugMode ? "[ON]" : "[OFF]")}");
                    break;
                case Settings.SaveAndExit:
                    Config.Write(configPath, config);
                    loop = false;
                    break;
                case Settings.ExitWithoutSaving:
                    if (VisualYesNoMenu("Are you sure you want to exit?"))
                        loop = false;
                    break;
                case Settings.DeleteConfigFile:
                    if (VisualYesNoMenu("Are you sure you want to delete the config file?\nThis will restart the program."))
                    {
                        File.Delete(configPath);
                        return;
                    }
                    break;
                case Settings.ShowOriginalNodes:
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
        const string configPath = Program.ConfigPath;
        
        var startOptions = new Tui.Option<StartMenuAction>[]
        {
            new(config.FirstTimeUsing ? "Start" : "Continue", config.FirstTimeUsing ? StartMenuAction.Start : StartMenuAction.Continue),
            new("Settings", StartMenuAction.Settings),
            new("Change File Path", StartMenuAction.ChangeFilePath),
            new("Help", StartMenuAction.Help),
            new("Exit", StartMenuAction.Exit),
        };

        var choice = Tui.Choice(startOptions, title: "Start Menu");

        switch (choice)
        {
            case StartMenuAction.Start:
            case StartMenuAction.Continue:
                var filePath = Helpers.GetFilePath(config, configPath);
                var result = Translation.Process(filePath, config, configPath);
                return result == TranslationResult.ExitRequested ? MenuResult.Exit : MenuResult.Continue;
                
            case StartMenuAction.Settings:
                SettingsMenu(configPath, config);
                return MenuResult.Continue;
                
            case StartMenuAction.ChangeFilePath:
                config.LastFile = ChangeFilePath(configPath, config, true);
                return MenuResult.Continue;
                
            case StartMenuAction.Help:
                Console.WriteLine("Press 'Escape' to exit while translating.\n" +
                                  "Write 'Settings' to access Settings.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return MenuResult.Continue;
                
            case StartMenuAction.Exit:
            case null:
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