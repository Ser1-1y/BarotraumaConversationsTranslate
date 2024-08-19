using System;
using System.Xml;
using System.Text.RegularExpressions;

namespace XMLConversationTranslator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("┌───────────────────────────────────────────────────────────────────────────────────────┐\r\n│                                                                                       │\r\n│   ─────┬─────           ────────┐  ┌───────┐  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │       ───────      ────┤  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                        │  │       │  │      │  │      │  │      │  │      │  │\r\n│        │                ────────┘  └───────┘  └──────┘  └──────┘  └──────┘  └──────┘  │\r\n│                                                                                       │\r\n└───────────────────────────────────────────────────────────────────────────────────────┘");
            Console.Write("Type ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("'ExitTranslation'");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" to stop");

            // Prompt the user for the input file path
            Console.Write("Input XML file: ");
            string inputFilePath = Console.ReadLine();

            // Prompt the user for the output file path
            Console.Write("Output XML file: ");
            string outputFilePath = Console.ReadLine();

            int TranslatedLinesCount = 0;

            // Load the XML document
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(inputFilePath);

            // Get all Conversation elements
            XmlNodeList conversationNodes = xmlDoc.SelectNodes("//Conversation");

            // Regular expression to detect Russian (Cyrillic) characters
            Regex cyrillicRegex = new Regex(@"\p{IsCyrillic}");

            if (conversationNodes != null)
            {
                foreach (XmlNode conversationNode in conversationNodes)
                {
                    // Get the 'line' attribute
                    XmlAttribute lineAttribute = conversationNode.Attributes["line"];
                    XmlAttribute traitAttribute = conversationNode.Attributes["speakertags"];
                    if (lineAttribute != null)
                    {
                        string originalLine = lineAttribute.Value;

                        // Check if the line contains any Russian text
                        if (!cyrillicRegex.IsMatch(originalLine))
                        {
                            // If it doesn't contain Russian, prompt for translation
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

                            // End translation session if user types 'ExitTranslation'
                            if (translatedLine.Equals("ExitTranslation", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine("Ending session...");
                                break;
                            }
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
                            Console.WriteLine($"Skipping: {originalLine}");
                        }
                    }
                }
            }

            // Save the modified XML document
            xmlDoc.Save(outputFilePath);

            Console.WriteLine($"Translation complete. The translated XML has been saved as '{outputFilePath}'.");
            Console.WriteLine($"Translated {TranslatedLinesCount} lines.");
        }
    }
}
