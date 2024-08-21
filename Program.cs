using System.Text.RegularExpressions;
using System.Xml;

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

            Console.Write("Input XML file: ");
            string FilePath = Console.ReadLine();
            if (FilePath is null || !System.IO.File.Exists(FilePath)) {
                throw new ArgumentNullException("Could not find file", nameof(FilePath));
            }

            int TranslatedLinesCount = 0;

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
                            if (translatedLine.Equals("ExitTranslation", StringComparison.OrdinalIgnoreCase) || translatedLine.Equals("ExitT", StringComparison.OrdinalIgnoreCase) || translatedLine.Equals("Exit", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine("Are you sure to exit? (y/n)");
                                string answer = Console.ReadLine();
                                if (answer == "y" || answer == "yes")
                                {
                                    Console.WriteLine("Ending session...");
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("Continuing session...");
                                    lineAttribute.Value = ExternalFunctions.WriteLine(originalLine, traitAttribute); ;
                                    TranslatedLinesCount++;
                                }
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
            xmlDoc.Save(FilePath);

            int EngLines = ExternalFunctions.EnglishCounter(FilePath);
            int RusLines = ExternalFunctions.RussianCounter(FilePath);

            Console.WriteLine($"Translation complete. The translated XML has been saved as '{FilePath}'.");
            Console.WriteLine($"Translated {TranslatedLinesCount} lines, {RusLines} in total.");
            Console.WriteLine($"{EngLines} English lines left.");
            Console.ReadLine();
        }
    }
}
