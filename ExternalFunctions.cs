using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml;

public static class ExternalFunctions
{
    static public Config ReadConfig(string ConfigFilePath)
    {
        if (File.Exists(ConfigFilePath))
        {
            string json = File.ReadAllText(ConfigFilePath);

            return JsonConvert.DeserializeObject<Config>(json);
        }
        else
        {
            Console.WriteLine("Config file not found.");

            return new Config();
        }
    }

    static public void WriteConfig(string ConfigFilePath, Config config)
    {
        string json = JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(ConfigFilePath, json);
    }

    static public void StartMenu(System.StringComparison OIC, Config config, string AppVersion)
    {
        bool continueSetting = true;

        while (continueSetting is true)
        {
            Console.Clear();
            Console.WriteLine("Start Menu");
            Console.WriteLine("1. Start");
            Console.WriteLine("2. Settings");
            Console.WriteLine("3. Exit");

            Console.Write("Please select an option (1-3): ");

            string choice = Console.ReadKey().KeyChar.ToString();
            Console.WriteLine("");

            switch (choice)
            {
                case "1":
                    continueSetting = false;
                    break;

                case "2":
                    ExternalFunctions.Settings("config.json", config, OIC, AppVersion);
                    break;

                case "3":
                    continueSetting = false;
                    Environment.Exit(0);
                    break;

                default:
                    ExternalFunctions.WriteColor("<=Red>Invalid option</>. Please select a valid option <=Green>(1-3)</>\n.");
                    Console.ReadKey();
                    break;
            }

            Console.Write("");
        }
    }

    static public void Settings(string ConfigFilePath, Config config, System.StringComparison OIC, string AppVersion)
    {
        bool continueSetting = true;

        string LogFilePath = $"logs-{DateTime.Now:yyyy-MM-dd}-v.{config.Version}.txt";

        while (continueSetting)
        {
            Console.Clear();

            Console.WriteLine("Settings Menu");
            ExternalFunctions.WriteColor("1. Show translated strings when loading a file: " + (config.ShowLoadedStrings ? "<=Green>Enabled</>\n" : "<=Red>Disabled</>\n"));
            ExternalFunctions.WriteColor("2. Activate debug mode: " + (config.DebugMode ? "<=Green>Enabled</>\n" : "<=Red>Disabled</>\n"));
            Console.WriteLine("3. Save and exit");
            Console.WriteLine("4. Exit without saving");
            if (config.DebugMode) 
            {
                ExternalFunctions.WriteColor("<=Gray>5. Delete config file</>\n");
                ExternalFunctions.WriteColor("<=Gray>6. Show original nodes when translating: </>" + (config.ShowOriginalNodes ? "<=Green>Enabled</>\n" : "<=Red>Disabled</>\n"));
                ExternalFunctions.WriteColor("<=Gray>7. Overwrite last file: </>" + config.LastFile + "\n");
                ExternalFunctions.WriteColor("<=Gray>Version: </>" + config.Version + "\n");
            }
            Console.Write("Please select an option (1-");
            if (config.DebugMode) { Console.Write("7): "); } else { Console.Write("4): "); }
            Console.Write("");

            string choice = Console.ReadKey().KeyChar.ToString();
            Console.WriteLine("");

            switch (choice)
            {
                case "1":
                    config.ShowLoadedStrings = !config.ShowLoadedStrings;
                    ExternalFunctions.Logger("Settings -> 1 -> Show Loaded Strings: " + config.ShowLoadedStrings, LogFilePath);
                    break;

                case "2":
                    config.DebugMode = !config.DebugMode;
                    if (!config.DebugMode) { config.ShowOriginalNodes = false; }
                    ExternalFunctions.Logger("Settings -> 2 -> Debug Mode: " + config.DebugMode, LogFilePath);
                    break;

                case "3":
                    config.HasRecentlyLaunched = true;
                    ExternalFunctions.WriteConfig(ConfigFilePath, config);
                    Console.WriteLine("Settings saved successfully.");
                    if (config.DebugMode) { Console.WriteLine(config); ExternalFunctions.Logger("Settings -> 3 -> Save and Exit: \n" + JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented) + "\n", LogFilePath); }
                    continueSetting = false;
                    break;

                case "4":
                    Console.WriteLine("Exiting without saving changes.");
                    continueSetting = false;
                    ExternalFunctions.Logger("Settings -> 4 -> Exit without saving changes", LogFilePath);
                    break;

                case "5":
                    if (config.DebugMode)
                    {
                        ExternalFunctions.WriteColor("Are you sure you want to <=Red>delete the config file</>? <=Green>(Y/n)</>\n");
                        choice = Console.ReadLine();
                        if (choice.Equals("y", OIC)) { File.Delete(ConfigFilePath); Environment.Exit(0); ExternalFunctions.Logger("Settings -> 5 -> Delete config file: True", LogFilePath); }
                        break;
                    }
                    break;

                case "6":
                    if (config.DebugMode)
                    {
                        config.ShowOriginalNodes = !config.ShowOriginalNodes;
                        ExternalFunctions.Logger("Settings -> 6 -> ShowOriginalNodes: " + config.ShowOriginalNodes, LogFilePath);
                        break;
                    }
                    break;

                case "7":
                    if (config.DebugMode)
                    {
                        Console.Write("Input XML file path: ");
                        string FilePath = Console.ReadLine();
                        config.LastFile = FilePath;
                        ExternalFunctions.Logger("Settings -> 7 -> LastFile: " + config.LastFile, LogFilePath);
                        break;
                    }
                    break;
                
                case "0":
                    ExternalFunctions.WriteColor("Version: ");
                    const string pattern = @"^(\d{1,3}\.){3}\d{1,3}$";
                    string Version = Console.ReadLine();
                    if (Version != "" && Regex.IsMatch(Version, pattern) 
                        || Version != null && Regex.IsMatch(Version, pattern) ) 
                    {
                        config.Version = Version; 
                        ExternalFunctions.Logger("Settings -> 0 -> Version: " + Version, LogFilePath); 
                    }
                    else 
                    {
                        ExternalFunctions.WriteColor("<=Red>Invalid version</>. Please use a valid version.\n");
                        ExternalFunctions.Logger("Wrong input at Settings -> 0 -> Version: " + Version, LogFilePath);
                        Console.ReadKey();
                    }
                    break;

                default:
                    ExternalFunctions.WriteColor("<=Red>Invalid option</>. Please select a valid option <=Green>(1-4)</>.\n");
                    ExternalFunctions.Logger("Settings: Invalid option.", LogFilePath);
                    Console.ReadKey();
                    break;
            }
            Console.Write("");
        }
    }

    public static void WriteColor(string text)
    {
        string[] ss = text.Split('<', '>');
        ConsoleColor Color;
        foreach (var String in ss)
            if (String.StartsWith("/"))
                Console.ResetColor();
            else if (String.StartsWith("=") && Enum.TryParse(String.Substring(1), out Color))
                Console.ForegroundColor = Color;
            else
                Console.Write(String);
    }

    public static XmlNodeList XmlDocAnalysis(string FilePath)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(FilePath);
        XmlNodeList conversationNodes = xmlDoc.SelectNodes("//Conversation");

        return conversationNodes;
    }

    public static string WriteLine(string originalLine, XmlAttribute traitAttribute, bool ShowOriginalNodes, XmlNode conversationNode)
    {
        if (ShowOriginalNodes)
        {
            Console.Write($"<Conversation line=\"{originalLine}\" speaker=\"{conversationNode.Attributes["speaker"].Value}\"");
            if (traitAttribute != null) { Console.WriteLine($" speakertags=\"{traitAttribute.Value}\">"); }
            else { Console.WriteLine(">"); }
        }
        ExternalFunctions.WriteColor($"<=Green>Original Line:</> {originalLine}");
        if (traitAttribute != null)
        {
            ExternalFunctions.WriteColor($"<=Green> SpeakerTags:</> {traitAttribute.Value}\n");
        }
        else
        {
            Console.WriteLine();
        }

        string translatedLine = Console.ReadLine();
        return translatedLine;
    }

    public static int EnglishCounter(string FilePath)
    {
        int EnglishLinesCount = 0;

        XmlNodeList conversationNodes = XmlDocAnalysis(FilePath);

        Regex cyrillicRegex = new Regex(@"\p{IsCyrillic}");
        foreach (XmlNode conversationNode in conversationNodes)
        {
            XmlAttribute lineAttribute = conversationNode.Attributes["line"];
            if (!cyrillicRegex.IsMatch(lineAttribute.Value))
            {
                EnglishLinesCount++;
            }
        }

        return EnglishLinesCount;
    }

    public static int TransCounter(string FilePath)
    {
        int RussianLinesCount = 0;

        XmlNodeList conversationNodes = XmlDocAnalysis(FilePath);

        Regex cyrillicRegex = new Regex(@"\p{IsCyrillic}");
        foreach (XmlNode conversationNode in conversationNodes)
        {
            XmlAttribute lineAttribute = conversationNode.Attributes["line"];
            if (cyrillicRegex.IsMatch(lineAttribute.Value))
            {
                RussianLinesCount++;
            }
        }

        return RussianLinesCount;
    }

    public static void Logger(string message, string logFilePath)
    {
        try
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(logEntry);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logging failed: {ex.Message}");
        }
    }
}
