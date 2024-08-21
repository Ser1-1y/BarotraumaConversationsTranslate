using System.Text.RegularExpressions;
using System.Xml;

public static class ConversationCounter
{
    public static XmlNodeList XmlDocAnalysis(string FilePath)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(FilePath);
        XmlNodeList conversationNodes = xmlDoc.SelectNodes("//Conversation");

        return conversationNodes;
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
