using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json;

namespace Translate;

public static class ExternalFunctions
{
    public static Config ReadConfig(string configFilePath)
    {
        if (File.Exists(configFilePath))
        {
            var json = File.ReadAllText(configFilePath);

            return JsonConvert.DeserializeObject<Config>(json) ?? throw new InvalidOperationException();
        }
        else
        {
            Console.WriteLine("Config file not found.");

            return new Config();
        }
    }

    public static void WriteConfig(string configFilePath, Config config)
    {
        var json = JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(configFilePath, json);
    }

    public static void StartMenu(StringComparison oic, Config config, string appVersion)
    {
        var continueSetting = true;

        while (continueSetting)
        {
            Console.Clear();
            Console.WriteLine("Start Menu");
            Console.WriteLine("1. Start");
            Console.WriteLine("2. Settings");
            Console.WriteLine("3. Exit");

            Console.Write("Please select an option (1-3): ");

            var choice = Console.ReadKey().KeyChar.ToString();
            Console.WriteLine("");

            switch (choice)
            {
                case "1":
                    continueSetting = false;
                    break;

                case "2":
                    Settings("config.json", config, oic, appVersion);
                    break;

                case "3":
                    continueSetting = false;
                    Environment.Exit(0);
                    break;

                default:
                    WriteColor("<=Red>Invalid option</>. Please select a valid option <=Green>(1-3)</>\n.");
                    Console.ReadKey();
                    break;
            }

            Console.Write("");
        }
    }

    public static void Settings(string configFilePath, Config config, StringComparison oic, string appVersion)
    {
        var continueSetting = true;

        var logFilePath = $"logs-{DateTime.Now:yyyy-MM-dd}-v.{config.Version}.txt";

        while (continueSetting)
        {
            Console.Clear();

            Console.WriteLine("Settings Menu");
            WriteColor("1. Show translated strings when loading a file: " + (config.ShowLoadedStrings ? "<=Green>Enabled</>\n" : "<=Red>Disabled</>\n"));
            WriteColor("2. Activate debug mode: " + (config.DebugMode ? "<=Green>Enabled</>\n" : "<=Red>Disabled</>\n"));
            Console.WriteLine("3. Save and exit");
            Console.WriteLine("4. Exit without saving");
            if (config.DebugMode)
            {
                WriteColor("<=Gray>5. Delete config file</>\n");
                WriteColor("<=Gray>6. Show original nodes when translating: </>" + (config.ShowOriginalNodes ? "<=Green>Enabled</>\n" : "<=Red>Disabled</>\n"));
                WriteColor("<=Gray>7. Overwrite last file: </>" + config.LastFile + "\n");
                WriteColor("<=Gray>Version: </>" + config.Version + "\n");
            }
            Console.Write("Please select an option (1-");
            if (config.DebugMode) { Console.Write("7): "); } else { Console.Write("4): "); }
            Console.Write("");

            var choice = Console.ReadKey().KeyChar.ToString();
            Console.WriteLine("");

            switch (choice)
            {
                case "1":
                    config.ShowLoadedStrings = !config.ShowLoadedStrings;
                    Logger("Settings -> 1 -> Show Loaded Strings: " + config.ShowLoadedStrings, logFilePath);
                    break;

                case "2":
                    config.DebugMode = !config.DebugMode;
                    if (!config.DebugMode) { config.ShowOriginalNodes = false; }
                    Logger("Settings -> 2 -> Debug Mode: " + config.DebugMode, logFilePath);
                    break;

                case "3":
                    config.HasRecentlyLaunched = true;
                    WriteConfig(configFilePath, config);
                    Console.WriteLine("Settings saved successfully.");
                    if (config.DebugMode) { Console.WriteLine(config); Logger("Settings -> 3 -> Save and Exit: \n" + JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented) + "\n", logFilePath); }
                    continueSetting = false;
                    break;

                case "4":
                    Console.WriteLine("Exiting without saving changes.");
                    continueSetting = false;
                    Logger("Settings -> 4 -> Exit without saving changes", logFilePath);
                    break;

                case "5":
                    if (config.DebugMode)
                    {
                        WriteColor("Are you sure you want to <=Red>delete the config file</>? <=Green>(Y/n)</>\n");
                        choice = Console.ReadLine();
                        if (choice!.Equals("y", oic)) { File.Delete(configFilePath); Environment.Exit(0); Logger("Settings -> 5 -> Delete config file: True", logFilePath); }
                    }
                    break;

                case "6":
                    if (config.DebugMode)
                    {
                        config.ShowOriginalNodes = !config.ShowOriginalNodes;
                        Logger("Settings -> 6 -> ShowOriginalNodes: " + config.ShowOriginalNodes, logFilePath);
                    }
                    break;

                case "7":
                    if (config.DebugMode)
                    {
                        Console.Write("Input XML file path: ");
                        var filePath = Console.ReadLine();
                        config.LastFile = filePath;
                        Logger("Settings -> 7 -> LastFile: " + config.LastFile, logFilePath);
                    }
                    break;

                case "0":
                    WriteColor("Version: ");
                    const string pattern = @"^(\d{1,3}\.){3}\d{1,3}$";
                    var version = Console.ReadLine();
                    if (version != "" && Regex.IsMatch(version ?? throw new InvalidOperationException(), pattern)
                        || Regex.IsMatch(version, pattern))
                    {
                        config.Version = version;
                        Logger("Settings -> 0 -> Version: " + version, logFilePath);
                    }
                    else
                    {
                        WriteColor("<=Red>Invalid version</>. Please use a valid version.\n");
                        Logger("Wrong input at Settings -> 0 -> Version: " + version, logFilePath);
                        Console.ReadKey();
                    }
                    break;

                default:
                    WriteColor("<=Red>Invalid option</>. Please select a valid option <=Green>(1-4)</>.\n");
                    Logger("Settings: Invalid option.", logFilePath);
                    Console.ReadKey();
                    break;
            }
            Console.Write("");
        }
    }

    public static void WriteColor(string text)
    {
        var ss = text.Split('<', '>');
        foreach (var @string in ss)
            if (@string.StartsWith($"/"))
                Console.ResetColor();
            else if (@string.StartsWith($"=") && Enum.TryParse(@string[1..], out ConsoleColor color))
                Console.ForegroundColor = color;
            else
                Console.Write(@string);
    }

    private static XmlNodeList XmlDocAnalysis(string filePath)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(filePath);
        var conversationNodes = xmlDoc.SelectNodes("//Conversation");

        return conversationNodes ?? throw new InvalidOperationException();
    }

    public static string WriteLine(string originalLine, XmlAttribute? traitAttribute, bool showOriginalNodes, XmlNode conversationNode)
    {
        if (showOriginalNodes)
        {
            Console.Write($"<Conversation line=\"{originalLine}\" speaker=\"{conversationNode.Attributes!["speaker"]!.Value}\"");
            Console.WriteLine(traitAttribute != null ? $" speakertags=\"{traitAttribute.Value}\">" : ">");
        }
        WriteColor($"<=Green>Original Line:</> {originalLine}");
        if (traitAttribute != null)
        {
            WriteColor($"<=Green> SpeakerTags:</> {traitAttribute.Value}\n");
        }
        else
        {
            Console.WriteLine();
        }

        var translatedLine = Console.ReadLine();
        return translatedLine ?? throw new InvalidOperationException();
    }

    public static int EnglishCounter(string filePath)
    {
        var englishLinesCount = 0;

        var conversationNodes = XmlDocAnalysis(filePath);

        var cyrillicRegex = new Regex(@"\p{IsCyrillic}");
        foreach (XmlNode conversationNode in conversationNodes)
        {
            var lineAttribute = conversationNode.Attributes!["line"];
            if (!cyrillicRegex.IsMatch(lineAttribute!.Value))
            {
                englishLinesCount++;
            }
        }

        return englishLinesCount;
    }

    public static int TransCounter(string filePath)
    {
        var russianLinesCount = 0;

        var conversationNodes = XmlDocAnalysis(filePath);

        var cyrillicRegex = new Regex(@"\p{IsCyrillic}");
        foreach (XmlNode conversationNode in conversationNodes)
        {
            var lineAttribute = conversationNode.Attributes!["line"];
            if (cyrillicRegex.IsMatch(lineAttribute!.Value))
            {
                russianLinesCount++;
            }
        }

        return russianLinesCount;
    }

    private static void Logger(string message, string logFilePath)
    {
        try
        {
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

            using var writer = new StreamWriter(logFilePath, true);
            writer.WriteLine(logEntry);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logging failed: {ex.Message}");
        }
    }
}