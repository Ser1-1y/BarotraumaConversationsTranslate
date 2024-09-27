using System;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json;
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
                Console.WriteLine("#########################");
                Console.WriteLine("No config file found");
                Console.WriteLine("Creating config file...");
                Config json = new Config();
                ExternalFunctions.WriteConfig(ConfigFilePath, json);
                Console.WriteLine("Would you like to edit the config file now? (Y/n)");
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
                    ExternalFunctions.WriteConfig(ConfigFilePath, config);
                    config = ExternalFunctions.ReadConfig(ConfigFilePath);
                }
                Console.WriteLine("#########################");
            }

            Console.WriteLine("┌───────────────────────────────────────────────────────────────────────────────────────┐\r\n│                                                                                       │\r\n│   ─────┬─────           ────────┐  ┌───────┐  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │       ───────      ────┤  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                ────────┘  └───────┘  └──────┘  └──────┘  └──────┘  └──────┘  │\r\n│                                                                                       │\r\n└───────────────────────────────────────────────────────────────────────────────────────┘");
            Console.Write("Type ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("'ExitTranslation'");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" to stop");

            Console.Write("Input XML file: ");
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
                    // Get the 'line' attribute
                    XmlAttribute lineAttribute = conversationNode.Attributes["line"];
                    XmlAttribute traitAttribute = conversationNode.Attributes["speakertags"];
                    if (lineAttribute != null && lineAttribute.Value != "")
                    {
                        string originalLine = lineAttribute.Value;

                        // Check if the line contains any Russian text
                        if (!cyrillicRegex.IsMatch(originalLine))
                        {
                            // If it doesn't contain Russian, prompt for translation
                            string translatedLine = ExternalFunctions.WriteLine(originalLine, traitAttribute);

                            // End translation session if user types 'ExitTranslation'
                            if (translatedLine.Equals("ExitTranslation", OIC) ||
                                translatedLine.Equals("ExitT", OIC) ||
                                translatedLine.Equals("Exit", OIC) ||
                                translatedLine.Equals("", OIC))
                            {
                                Console.WriteLine("Ending session...");
                                break;
                            }
                            //if (translatedLine.Equals("Settings", OIC))
                            //{
                            //
                            //}
                            else
                            {
                                // Set the translated value back to the 'line' attribute
                                lineAttribute.Value = translatedLine;
                                TranslatedLinesCount++;
                            }

                        }
                        else
                        {
                            // Skip lines with Russian text
                            if (config.ShowLoadedStrings == true)
                            {
                                Console.WriteLine($"Skipping: {originalLine}");
                            }
                            //
                        }
                    }
                }
            }

            // Save the modified XML document
            xmlDoc.Save(FilePath);

            int EngLines = ExternalFunctions.EnglishCounter(FilePath);
            int TransLines = ExternalFunctions.RussianCounter(FilePath);

            double Time = DateTime.Now.Subtract(StartDateTime).TotalMinutes;

            Console.WriteLine($"Translation complete. The translated XML has been saved as '{FilePath}'.");
            Console.WriteLine($"Translated {TranslatedLinesCount} lines, {TransLines} in total.");
            Console.WriteLine($"{EngLines} English lines left.");
            Console.WriteLine($"You've been working for {Time}.");
            Console.WriteLine($"This is roughly {Time / TransLines} per line.");
            Console.ReadLine();
        }
    }
}
