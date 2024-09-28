﻿using System;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json;
using System.Threading;
//using System.Configuration;

namespace XMLConversationTranslator
{
    class Program
    {
        static void Main(string[] args)
        {
            System.StringComparison OIC = StringComparison.OrdinalIgnoreCase;

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
                    ExternalFunctions.Settings(ConfigFilePath, json, OIC);
                    config = ExternalFunctions.ReadConfig(ConfigFilePath);
                }
                else
                {
                    config = new Config();
                    config.ShowLoadedStrings = true;
                    config.HasRecentlyLaunched = true;
                    config.DebugMode = false;
                    ExternalFunctions.WriteConfig(ConfigFilePath, config);
                    config = ExternalFunctions.ReadConfig(ConfigFilePath);
                }
            }

            Console.WriteLine("┌───────────────────────────────────────────────────────────────────────────────────────┐\r\n│                                                                                       │\r\n│   ─────┬─────           ────────┐  ┌───────┐  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │       ───────      ────┤  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                ────────┘  └───────┘  └──────┘  └──────┘  └──────┘  └──────┘  │\r\n│                                                                                       │\r\n└───────────────────────────────────────────────────────────────────────────────────────┘");

            ExternalFunctions.StartMenu(OIC, config);
            
            ExternalFunctions.WriteColor("Type <=Green>'Exit'</> to stop.\n");

            Console.Write("Input XML file path: ");
            string FilePath = Console.ReadLine();
            if (!FilePath.EndsWith(".xml")) {
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

            Console.WriteLine($"Loading {FilePath}...");

            config.LastFile = FilePath;

            int TranslatedLinesCount = 0;

            DateTime StartDateTime = DateTime.Now;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(FilePath);

            XmlNodeList conversationNodes = xmlDoc.SelectNodes("//Conversation");

            Regex cyrillicRegex = new Regex(@"\p{IsCyrillic}");

            if (conversationNodes != null)
            {
                foreach (XmlNode conversationNode in conversationNodes)
                {
                    XmlAttribute lineAttribute = conversationNode.Attributes["line"];
                    XmlAttribute traitAttribute = conversationNode.Attributes["speakertags"];
                    if (lineAttribute != null && lineAttribute.Value != "")
                    {
                        string originalLine = lineAttribute.Value;

                        // Check if the line contains any Russian text
                        if (!cyrillicRegex.IsMatch(originalLine))
                        {
                            if (config.ShowOriginalNodes) { Console.WriteLine(conversationNode.OuterXml); }
                            string translatedLine = ExternalFunctions.WriteLine(originalLine, traitAttribute, config.DebugMode, conversationNode);

                            if (translatedLine.Equals("ExitTranslation", OIC) ||
                                translatedLine.Equals("ExitT", OIC) ||
                                translatedLine.Equals("Exit", OIC) ||
                                translatedLine.Equals("", OIC))
                            {
                                Console.Write("Ending session...");
                                break;
                            }
                            if (translatedLine.Equals("Settings", OIC))
                            {
                                ExternalFunctions.Settings(ConfigFilePath, config, OIC);
                                config = ExternalFunctions.ReadConfig(ConfigFilePath);
                                Console.WriteLine("Ending session...");
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

            xmlDoc.Save(FilePath);

            int EngLines = ExternalFunctions.EnglishCounter(FilePath);
            int TransLines = ExternalFunctions.TransCounter(FilePath);

            double Time = DateTime.Now.Subtract(StartDateTime).Minutes;
            double TimeInSeconds = Time * 60;
            double SecPerLineRounded = Math.Round(TimeInSeconds / TranslatedLinesCount, 1);

            ExternalFunctions.WriteColor($"Translation complete. The translated XML has been saved as <=Green>'{FilePath}'</>.\n");
            ExternalFunctions.WriteColor($"Translated <=Green>{TranslatedLinesCount}</> lines, <=Green>{TransLines}</> in total.\n");
            ExternalFunctions.WriteColor($"<=Green>{EngLines}</> English lines left.\n");
            ExternalFunctions.WriteColor($"You've been working for <=Green>{Time}</> minutes.\n");
            ExternalFunctions.WriteColor($"That's roughly <=Green>{SecPerLineRounded}</> seconds per line.\n");
            Console.ReadKey();
        }
    }
}
