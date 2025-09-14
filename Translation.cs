using System.Text;
using System.Xml;

namespace Translate;

internal static class Document
{
    internal static void Save(string filePath, string configPath, Config config, XmlDocument xmlDoc)
    {
        config.LastFile = filePath;
        Config.Write(configPath, config);
        xmlDoc.Save(filePath);
    }

    public static XmlDocument Load(string filePath, XmlDocument xmlDoc)
    {
        xmlDoc.Load(filePath);
        return xmlDoc;
    }
    
    public static string WriteXmlLine(string original, XmlAttribute? tags, bool showNodes, XmlNode node)
    {
        if (showNodes)
        {
            Console.Write($"<Conversation line=\"{original}\" speaker=\"{node.Attributes!["speaker"]!.Value}\"");
            Console.WriteLine(tags != null ? $" speakertags=\"{tags.Value}\">" : ">");
        }

        Color.Write($"<=Green>Original:</> {original}");
        if (tags != null) Color.Write($" <=Green>Tags:</> {tags.Value}\n");
        else Console.WriteLine();

        var buffer = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return buffer.ToString();
            }
            if (key.Key == ConsoleKey.Escape)
            {
                // Save and exit immediately
                return "__EXIT__";
            }

            buffer.Append(key.KeyChar);
            Console.Write(key.KeyChar);
        }
    }

}

public static class Translation
{
    public static void Process(string filePath, Config config, string configPath, StringComparison oic)
    {
        Console.Clear();
            
        var translatedLinesCount = 0;
        var xmlDoc = new XmlDocument();
        xmlDoc = Document.Load(filePath, xmlDoc);
        config.FirstTimeUsing = false;

        var nodes = xmlDoc.SelectNodes("//Conversation");

        if (nodes is null)
        {
            Console.WriteLine("[ERROR] There are no nodes.");
            config.LastFile = null;
            Config.Write(configPath, config);
            Console.ReadKey();
            Environment.Exit(1);
        }

        foreach (XmlNode conv in nodes)
        {
            var line = conv.Attributes?["line"];
            var tags = conv.Attributes?["speakertags"];
            if (line == null 
                || string.IsNullOrWhiteSpace(line.Value) 
                || line.Value == "..." 
                || Count.NumbersAndDotsRegex().IsMatch(line.Value))
            {
                continue;
            }

            if (Count.CyrillicRegex().IsMatch(line.Value))
            {
                if (config.ShowLoadedStrings) 
                    Console.WriteLine($"Skipping: {line.Value}");
                continue;
            }
            
            linecheck:
            var translatedLine = Document.WriteXmlLine(line.Value, tags, config.ShowOriginalNodes, conv);

            if (translatedLine == "__EXIT__")
            {
                if (!Menu.ExitMenu())
                    goto linecheck;
                Document.Save(filePath, configPath, config, xmlDoc);
                GetResults(filePath, translatedLinesCount);
                Environment.Exit(0);
            }
                
            if (translatedLine.Equals("Settings", oic))
            {
                Document.Save(filePath, configPath, config, xmlDoc);
                Menu.SettingsMenu(configPath, config);
                break;
            }

            line.Value = translatedLine;
            translatedLinesCount++;
        }

        Document.Save(filePath, configPath, config, xmlDoc);
        GetResults(filePath, translatedLinesCount);
    }

    private static void GetResults(string filePath, int translatedLinesCount)
    {
        var engLines = Count.English(filePath);
        var transLines = Count.Translated(filePath);

        var elapsed = DateTime.Now - File.GetLastWriteTime(filePath);
        var minutes = elapsed.TotalMinutes;
        var secPerLine = translatedLinesCount > 0 ? Math.Round(elapsed.TotalSeconds / translatedLinesCount, 1) : 0;

        Color.Write($"Saved as <=Green>'{filePath}'</>.\n");
        Color.Write($"Translated <=Green>{translatedLinesCount}</> lines, <=Green>{transLines}</> in total.\n");
        Color.Write($"<=Green>{engLines}</> English lines left.\n");
        Color.Write($"Time: <=Green>{minutes:F1}</> minutes.\n");
        Color.Write($"≈ <=Green>{secPerLine}</> sec/line.\n");

        Console.ReadKey();
    }
}