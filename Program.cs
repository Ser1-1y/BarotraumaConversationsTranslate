using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Translate
{
    class Program
    {
        static void Main(string[] args)
        {
            const StringComparison oic = StringComparison.OrdinalIgnoreCase;

            //
            const string appVersion = "1.0.1.0";
            //

            const string configFilePath = "config.json";
            Config config;

            if (File.Exists(configFilePath))
            {
                config = ExternalFunctions.ReadConfig(configFilePath);
            }
            else
            {
                ExternalFunctions.WriteColor("<=Red>No config file found</>\n");
                Console.Write("Creating config file");
                for (var i = 0; i < 5; i++) { Console.Write("."); Thread.Sleep(100); }
                Console.WriteLine();
                var json = new Config();
                ExternalFunctions.WriteConfig(configFilePath, json);
                ExternalFunctions.WriteColor("Change config now? You can do that in the settings menu later. <=Green>(Y/n)</>\n");
                var userChoice = Console.ReadLine() ?? throw new InvalidOperationException();
                switch (userChoice.ToLower())
                {
                    case "y" or "yes":
                        ExternalFunctions.Settings(configFilePath, json, oic, appVersion);
                        break;
                    default:
                        config = new Config
                        {
                            ShowLoadedStrings = true,
                            DebugMode = false
                        };
                        ExternalFunctions.WriteConfig(configFilePath, config);
                        break;
                }

                config = ExternalFunctions.ReadConfig(configFilePath);
            }

            ExternalFunctions.StartMenu(oic, config, appVersion);

            Console.Clear();

            Console.WriteLine("┌───────────────────────────────────────────────────────────────────────────────────────┐\r\n│                                                                                       │\r\n│   ─────┬─────           ────────┐  ┌───────┐  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │       ───────      ────┤  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                ────────┘  └───────┘  └──────┘  └──────┘  └──────┘  └──────┘  │\r\n│                                                                                       │\r\n└───────────────────────────────────────────────────────────────────────────────────────┘");

            ExternalFunctions.WriteColor("Type <=Green>'Exit'</> to stop.\n");

            string filePath;

            if (!config.HasRecentlyLaunched || config.LastFile == null)
            {
                Console.Write("Input XML file path: ");
                filePath = Console.ReadLine() ?? throw new InvalidOperationException();
                if (!filePath.EndsWith(".xml"))
                {
                    if (File.Exists(filePath + ".xml"))
                    {
                        filePath += ".xml";
                    }
                    else
                    {
                        try
                        {
                            filePath = config.LastFile!;
                        }
                        catch
                        {
                            throw new ArgumentNullException(new StringBuilder().Append("Could not find file").ToString(), nameof(filePath));
                        }
                    }
                }
            }
            else
            {
                filePath = config.LastFile;
            }

            Console.WriteLine($"Loading {filePath}...");

            var translatedLinesCount = 0;

            var startDateTime = DateTime.Now;

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            var conversationNodes = xmlDoc.SelectNodes("//Conversation");

            var cyrillicRegex = new Regex(@"\p{IsCyrillic}");

            if (conversationNodes != null)
            {
                config.HasRecentlyLaunched = true;

                foreach (XmlNode conversationNode in conversationNodes)
                {
                    var lineAttribute = conversationNode.Attributes!["line"];
                    var traitAttribute = conversationNode.Attributes["speakertags"];
                    if (lineAttribute != null && lineAttribute.Value != "" && lineAttribute.Value != "...")
                    {
                        var originalLine = lineAttribute.Value;

                        if (!cyrillicRegex.IsMatch(originalLine))
                        {
                            var translatedLine = ExternalFunctions.WriteLine(originalLine, traitAttribute, config.ShowOriginalNodes, conversationNode);

                            if (translatedLine.Equals("ExitTranslation", oic) ||
                                translatedLine.Equals("ExitT", oic) ||
                                translatedLine.Equals("Exit", oic) ||
                                translatedLine.Equals("", oic))
                            {
                                break;
                            }
                            if (translatedLine.Equals("Settings", oic))
                            {
                                ExternalFunctions.Settings(configFilePath, config, oic, appVersion);
                                config = ExternalFunctions.ReadConfig(configFilePath);
                                break;
                            }
                            else
                            {
                                lineAttribute.Value = translatedLine;
                                translatedLinesCount++;
                            }
                        }
                        else
                        {
                            if (config.ShowLoadedStrings)
                            {
                                Console.WriteLine($"Skipping: {originalLine}");
                            }
                        }
                    }
                }
            }

            config.LastFile = filePath;
            ExternalFunctions.WriteConfig(configFilePath, config);

            xmlDoc.Save(filePath);

            var engLines = ExternalFunctions.EnglishCounter(filePath);
            var transLines = ExternalFunctions.TransCounter(filePath);

            double time = DateTime.Now.Subtract(startDateTime).Minutes;
            var timeInSeconds = time * 60;
            var secPerLineRounded = Math.Round(timeInSeconds / translatedLinesCount, 1);

            ExternalFunctions.WriteColor($"The translated XML has been saved as <=Green>'{filePath}'</>.\n");
            ExternalFunctions.WriteColor($"Translated <=Green>{translatedLinesCount}</> lines, <=Green>{transLines}</> in total.\n");
            ExternalFunctions.WriteColor($"<=Green>{engLines}</> English lines left.\n");
            ExternalFunctions.WriteColor($"You've been working for <=Green>{time}</> minutes.\n");
            ExternalFunctions.WriteColor($"That's roughly <=Green>{secPerLineRounded}</> seconds per line.\n");
            Console.ReadKey();
        }
    }
}

