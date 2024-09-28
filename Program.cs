using System;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json;
using System.Threading;

namespace XMLConversationTranslator
{
    class Program
    {
        static void Main(string[] args)
        {
            System.StringComparison OIC = StringComparison.OrdinalIgnoreCase;

            //
            string AppVersion = "1.0.0.0";
            //

            string ConfigFilePath = "config.json";
            Config config;

            if (File.Exists(ConfigFilePath))
            {
                config = ExternalFunctions.ReadConfig(ConfigFilePath);
            }
            else
            {
                ExternalFunctions.WriteColor("<=Red>No config file found</>\n");
                Console.Write("Creating config file");
                for (int i = 0; i < 5; i++) { Console.Write("."); Thread.Sleep(100); } Console.WriteLine();
                Config json = new Config();
                ExternalFunctions.WriteConfig(ConfigFilePath, json);
                ExternalFunctions.WriteColor("Change config now? You can do that in the settings menu later. <=Green>(Y/n)</>\n");
                string answer = Console.ReadLine();
                if (answer.Equals("y", OIC))
                {
                    ExternalFunctions.Settings(ConfigFilePath, json, OIC, AppVersion);
                    config = ExternalFunctions.ReadConfig(ConfigFilePath);
                }
                else
                {
                    config = new Config();
                    config.ShowLoadedStrings = true;
                    config.DebugMode = false;
                    ExternalFunctions.WriteConfig(ConfigFilePath, config);
                    config = ExternalFunctions.ReadConfig(ConfigFilePath);
                }
            }

            Console.WriteLine("┌───────────────────────────────────────────────────────────────────────────────────────┐\r\n│                                                                                       │\r\n│   ─────┬─────           ────────┐  ┌───────┐  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │       ───────      ────┤  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                ────────┘  └───────┘  └──────┘  └──────┘  └──────┘  └──────┘  │\r\n│                                                                                       │\r\n└───────────────────────────────────────────────────────────────────────────────────────┘");

            ExternalFunctions.StartMenu(OIC, config, AppVersion);
            
            ExternalFunctions.WriteColor("Type <=Green>'Exit'</> to stop.\n");

            string FilePath;

            if (!config.HasRecentlyLaunched || config.LastFile == null)
            {
                Console.Write("Input XML file path: ");
                FilePath = Console.ReadLine();
                if (!FilePath.EndsWith(".xml"))
                {
                    if (File.Exists(FilePath + ".xml"))
                    {
                        FilePath += ".xml";
                    }
                    else
                    {
                        try
                        {
                            FilePath = config.LastFile;
                        }
                        catch
                        {
                            throw new ArgumentNullException("Could not find file", nameof(FilePath));
                        }
                    }
                }
            }
            else
            {
                FilePath = config.LastFile;
            }

            Console.WriteLine($"Loading {FilePath}...");

            int TranslatedLinesCount = 0;

            DateTime StartDateTime = DateTime.Now;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(FilePath);

            XmlNodeList conversationNodes = xmlDoc.SelectNodes("//Conversation");

            Regex cyrillicRegex = new Regex(@"\p{IsCyrillic}");

            if (conversationNodes != null)
            {
                config.HasRecentlyLaunched = true;

                foreach (XmlNode conversationNode in conversationNodes)
                {
                    XmlAttribute lineAttribute = conversationNode.Attributes["line"];
                    XmlAttribute traitAttribute = conversationNode.Attributes["speakertags"];
                    if (lineAttribute != null && lineAttribute.Value != "")
                    {
                        string originalLine = lineAttribute.Value;
                        
                        if (!cyrillicRegex.IsMatch(originalLine))
                        {
                            string translatedLine = ExternalFunctions.WriteLine(originalLine, traitAttribute, config.ShowOriginalNodes, conversationNode);

                            if (translatedLine.Equals("ExitTranslation", OIC) ||
                                translatedLine.Equals("ExitT", OIC) ||
                                translatedLine.Equals("Exit", OIC) ||
                                translatedLine.Equals("", OIC))
                            {
                                break;
                            }
                            if (translatedLine.Equals("Settings", OIC))
                            {
                                ExternalFunctions.Settings(ConfigFilePath, config, OIC, AppVersion);
                                config = ExternalFunctions.ReadConfig(ConfigFilePath);
                                break;
                            }
                            else
                            {
                                lineAttribute.Value = translatedLine;
                                TranslatedLinesCount++;
                            }
                        }
                        else
                        {
                            if (config.ShowLoadedStrings == true)
                            {
                                Console.WriteLine($"Skipping: {originalLine}");
                            }
                        }
                    }
                }
            }

            config.LastFile = FilePath;
            ExternalFunctions.WriteConfig(ConfigFilePath, config);

            xmlDoc.Save(FilePath);

            int EngLines = ExternalFunctions.EnglishCounter(FilePath);
            int TransLines = ExternalFunctions.TransCounter(FilePath);

            double Time = DateTime.Now.Subtract(StartDateTime).Minutes;
            double TimeInSeconds = Time * 60;
            double SecPerLineRounded = Math.Round(TimeInSeconds / TranslatedLinesCount, 1);

            ExternalFunctions.WriteColor($"The translated XML has been saved as <=Green>'{FilePath}'</>.\n");
            ExternalFunctions.WriteColor($"Translated <=Green>{TranslatedLinesCount}</> lines, <=Green>{TransLines}</> in total.\n");
            ExternalFunctions.WriteColor($"<=Green>{EngLines}</> English lines left.\n");
            ExternalFunctions.WriteColor($"You've been working for <=Green>{Time}</> minutes.\n");
            ExternalFunctions.WriteColor($"That's roughly <=Green>{SecPerLineRounded}</> seconds per line.\n");
            Console.ReadKey();
        }
    }
}
