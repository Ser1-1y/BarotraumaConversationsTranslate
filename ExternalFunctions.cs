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

    static public void StartMenu(System.StringComparison OIC, Config config)
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

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    continueSetting = false;
                    break;

                case "2":
                    Settings("config.json", config, OIC);
                    break;

                case "3":
                    continueSetting = false;
                    Environment.Exit(0);
                    break;

                default:
                    ExternalFunctions.WriteColor("<=Red>Invalid option</>. Please select a valid option <=Green>(1-3)</>\n.");
                    break;
            }
            Console.Write("");
        }
    }

    static public void Settings(string ConfigFilePath, Config config, System.StringComparison OIC)
    {
        bool continueSetting = true;

        while (continueSetting is true)
        {
            Console.Clear();
            Console.WriteLine("Settings Menu");
            ExternalFunctions.WriteColor("1. Show translated strings when loading a file: " + (config.ShowLoadedStrings ? "<=Green>Enabled</>\n" : "<=Red>Disabled</>\n"));
            ExternalFunctions.WriteColor("2. Activate debug mode: " + (config.DebugMode ? "<=Green>Enabled</>\n" : "<=Red>Disabled</>\n"));
            Console.WriteLine("3. Save and exit");
            Console.WriteLine("4. Exit without saving");
            if (config.DebugMode) { ExternalFunctions.WriteColor("<=Gray>5. Delete config file</>\n"); }
            if (config.DebugMode) { ExternalFunctions.WriteColor("<=Gray>6. Show original nodes when translating: </>" + (config.ShowOriginalNodes ? "<=Green>Enabled</>\n" : "<=Red>Disabled</>\n")); }
            Console.Write("Please select an option (1-");
            if (config.DebugMode) { Console.Write("6): "); } else { Console.Write("4): "); }
            Console.Write("");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    config.ShowLoadedStrings = !config.ShowLoadedStrings;
                    break;

                case "2":
                    config.DebugMode = !config.DebugMode;
                    if (!config.DebugMode) { config.ShowOriginalNodes = false; }
                    break;

                case "3":
                    config.HasRecentlyLaunched = true;
                    ExternalFunctions.WriteConfig(ConfigFilePath, config);
                    Console.WriteLine("Settings saved successfully.");
                    continueSetting = false;
                    break;

                case "4":
                    Console.WriteLine("Exiting without saving changes.");
                    continueSetting = false;
                    break;

                case "5":
                    if (config.DebugMode)
                    {
                        ExternalFunctions.WriteColor("Are you sure you want to <=Red>delete the config file</>? <=Green>(Y/n)</>\n");
                        choice = Console.ReadLine();
                        if (choice.Equals("y", OIC)) { File.Delete(ConfigFilePath); Environment.Exit(0); }
                        break;
                    }
                    break;

                case "6":
                    if (config.DebugMode)
                    {
                        config.ShowOriginalNodes = !config.ShowOriginalNodes;
                        break;
                    }
                    break;

                default:
                    ExternalFunctions.WriteColor("<=Red>Invalid option</>. Please select a valid option <=Green>(1-4)</>\n.");
                    Console.ReadKey();
                    break;
            }
            Console.Write("");
        }
    }


    public static void WriteColor(string text)
    {
        string[] ss = text.Split('<', '>');
        ConsoleColor c;
        foreach (var s in ss)
            if (s.StartsWith("/"))
                Console.ResetColor();
            else if (s.StartsWith("=") && Enum.TryParse(s.Substring(1), out c))
                Console.ForegroundColor = c;
            else
                Console.Write(s);
    }

    public static XmlNodeList XmlDocAnalysis(string FilePath)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(FilePath);
        XmlNodeList conversationNodes = xmlDoc.SelectNodes("//Conversation");

        return conversationNodes;
    }

    public static string WriteLine(string originalLine, XmlAttribute traitAttribute, bool debugMode, XmlNode conversationNode)
    {
        if (debugMode)
        {
            Console.Write($"<Conversation line=\"{originalLine}\" speaker=\"{conversationNode.Attributes["speaker"].Value}\"");
            if (traitAttribute != null) { Console.WriteLine($" speakertags=\"{traitAttribute.Value}\">"); }
            else { Console.WriteLine(">"); }
        }
        ExternalFunctions.WriteColor($"<=Green>Original Line:</> {originalLine}");
        //Console.ForegroundColor = ConsoleColor.Green;
        //Console.Write("Original Line: ");
        //Console.ForegroundColor = ConsoleColor.White;
        //Console.Write(originalLine);
        if (traitAttribute != null)
        {
            ExternalFunctions.WriteColor($"<=Green> SpeakerTags:</> {traitAttribute.Value}\n");
            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.Write(" SpeakerTags: ");
            //Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine(traitAttribute.Value);
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
}
