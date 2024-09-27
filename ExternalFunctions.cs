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
            // Handle the case where the file does not exist
            Console.WriteLine("Config file not found.");
            return new Config(); // Return a new instance or handle as needed
        }
    }

    static public void WriteConfig(string ConfigFilePath, Config config)
    {
        string json = JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(ConfigFilePath, json);
    }

    static public void Settings(string ConfigFilePath, Config config, System.StringComparison OIC)
    {
        Console.WriteLine("Would you like to see translated strings when loading a file? (Y/n)");
        string choice = Console.ReadLine();
        if (choice.Equals(("y", OIC)))
        {
            config.ShowLoadedStrings = true;
            config.HasRecentlyLaunched = true;
        }
        else
        {
            config.ShowLoadedStrings = false;
            config.HasRecentlyLaunched = true;
        }
        ExternalFunctions.WriteConfig(ConfigFilePath, config);
    }

    public static XmlNodeList XmlDocAnalysis(string FilePath)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(FilePath);
        XmlNodeList conversationNodes = xmlDoc.SelectNodes("//Conversation");

        return conversationNodes;
    }

    public static string WriteLine(string originalLine, XmlAttribute traitAttribute)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Original Line: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(originalLine);
        if (traitAttribute != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" SpeakerTags: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(traitAttribute.Value);
        }
        else
        {
            Console.WriteLine();
        }

        string translatedLine = Console.ReadLine();
        return translatedLine;
    }

    //public static string Settings(string originalLine, XmlAttribute traitAttribute)
    //{
    //
    //    while (true)
    //    {
    //        Console.WriteLine("1. Translated lines when loading a file");
    //        Console.WriteLine("2. Exit");
    //        string select = Console.ReadLine();
    //        if (select is "1" || select is "1.")
    //        {
    //            while (true)
    //            {
    //                Console.WriteLine("Y/n");
    //
    //                System.StringComparison OIC = StringComparison.OrdinalIgnoreCase;
    //                string selectYorN = Console.ReadLine();
    //                if (selectYorN.Equals("Y", OIC))
    //                {
    //
    //                };
    //            }
    //        }
    //    }
    //
    //    return WriteLine(originalLine, traitAttribute);
    //}

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

    public static int RussianCounter(string FilePath)
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
