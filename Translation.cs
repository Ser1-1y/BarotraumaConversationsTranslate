using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Translate;

public enum TranslationResult
{
    Completed,
    ExitRequested,
    BackToMenu,
}

internal static class Document
{
    public const string ExitCommand = "__EXIT__";
    public const string SettingsCommand = "Settings";

    private static void RedrawInput(StringBuilder buffer, int cursorPos)
    {
        Console.Write("\r");
        Console.Write(buffer + " ");
        Console.SetCursorPosition(cursorPos, Console.CursorTop);
    }

    internal static void Save(string filePath, string configPath, Config config, XmlDocument xmlDoc)
    {
        config.LastFile = filePath;
        Config.Write(configPath, config);
        xmlDoc.Save(filePath);
    }

    public static XmlDocument Load(string filePath)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(filePath);
        return xmlDoc;
    }

    public static string WriteXmlLine(string original, XmlAttribute? tags, bool showNodes, XmlNode node)
    {
        if (showNodes)
        {
            Console.Write($"<Conversation line=\"{original}\"");
            Console.WriteLine(tags != null ? $" speakertags=\"{tags.Value}\">" : ">");
        }

        Color.Write($"<=Green>Original:</> {original} | <=Green>Speaker:</> \"{node.Attributes?["speaker"]?.Value}\"");
        Color.Write(tags != null ? $" <=Green>Tags:</> {tags.Value}\n" : "\n");

        var buffer = new StringBuilder();
        var cursorPos = 0;

        while (true)
        {
            var key = Console.ReadKey(intercept: true);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    return buffer.ToString();

                case ConsoleKey.Escape:
                    return ExitCommand;

                case ConsoleKey.Backspace when cursorPos > 0:
                    buffer.Remove(--cursorPos, 1);
                    RedrawInput(buffer, cursorPos);
                    break;

                case ConsoleKey.Delete when cursorPos < buffer.Length:
                    buffer.Remove(cursorPos, 1);
                    RedrawInput(buffer, cursorPos);
                    break;

                case ConsoleKey.LeftArrow when cursorPos > 0:
                    cursorPos--;
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    break;

                case ConsoleKey.RightArrow when cursorPos < buffer.Length:
                    cursorPos++;
                    Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                    break;

                case ConsoleKey.Home:
                    Console.SetCursorPosition(Console.CursorLeft - cursorPos, Console.CursorTop);
                    cursorPos = 0;
                    break;

                case ConsoleKey.End:
                    Console.SetCursorPosition(Console.CursorLeft + (buffer.Length - cursorPos), Console.CursorTop);
                    cursorPos = buffer.Length;
                    break;

                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        buffer.Insert(cursorPos++, key.KeyChar);
                        RedrawInput(buffer, cursorPos);
                    }
                    break;
            }
        }
    }
}

public static class Translation
{
    private static readonly Regex Cyrillic = Count.CyrillicRegex();
    private static readonly Regex NumbersAndDots = Count.NumbersAndDotsRegex();

    public static TranslationResult Process(string filePath, Config config, string configPath)
    {
        Console.Clear();

        var translatedLinesCount = 0;
        var xmlDoc = Document.Load(filePath);
        config.FirstTimeUsing = false;

        var nodes = xmlDoc.SelectNodes("//Conversation");
        if (nodes is null)
        {
            Console.WriteLine("[ERROR] There are no nodes.");
            config.LastFile = null;
            Config.Write(configPath, config);
            Console.ReadKey();
            return TranslationResult.BackToMenu;
        }

        foreach (XmlNode conv in nodes)
        {
            var line = conv.Attributes?["line"];
            var tags = conv.Attributes?["speakertags"];

            if (ShouldSkipLine(line, config))
                continue;

            while (true)
            {
                var translatedLine = Document.WriteXmlLine(line!.Value, tags, config.ShowOriginalNodes, conv);

                if (translatedLine == Document.ExitCommand)
                {
                    if (!Menu.ExitMenu())
                        continue;

                    Document.Save(filePath, configPath, config, xmlDoc);
                    GetResults(filePath, translatedLinesCount);
                    return TranslationResult.ExitRequested;
                }

                if (translatedLine.Equals(Document.SettingsCommand, StringComparison.OrdinalIgnoreCase))
                {
                    Document.Save(filePath, configPath, config, xmlDoc);
                    Menu.SettingsMenu(configPath, config);
                    return TranslationResult.BackToMenu;
                }

                line.Value = translatedLine;
                translatedLinesCount++;
                break;
            }
        }

        Document.Save(filePath, configPath, config, xmlDoc);
        GetResults(filePath, translatedLinesCount);
        return TranslationResult.Completed;
    }

    private static bool ShouldSkipLine(XmlAttribute? line, Config config)
    {
        if (line == null || string.IsNullOrWhiteSpace(line.Value) || line.Value == "..." || NumbersAndDots.IsMatch(line.Value))
            return true;

        if (!Cyrillic.IsMatch(line.Value)) 
            return false;
        if (config.ShowLoadedStrings)
            Console.WriteLine($"Skipping: {line.Value}");
        return true;
    }

    private static void GetResults(string filePath, int translatedLinesCount)
    {
        var transLines = Count.Translated(filePath);
        var engLines = Count.English(filePath);

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