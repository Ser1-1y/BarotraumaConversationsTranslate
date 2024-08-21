using System.Text.RegularExpressions;
using System.Xml;

public static class ExternalFunctions
{
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
