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
        Console.CursorVisible = false;
        Console.Write("\r" + buffer + "\e[K");
    
        Console.SetCursorPosition(cursorPos, Console.CursorTop);
        Console.CursorVisible = true;
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

        int GetPrevWordPos(int pos)
        {
            while (pos > 0 && char.IsWhiteSpace(buffer[pos - 1])) pos--;
            while (pos > 0 && !char.IsWhiteSpace(buffer[pos - 1])) pos--;
            return pos;
        }

        int GetNextWordPos(int pos)
        {
            while (pos < buffer.Length && char.IsWhiteSpace(buffer[pos])) pos++;
            while (pos < buffer.Length && !char.IsWhiteSpace(buffer[pos])) pos++;
            return pos;
        }
        
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            var isCtrl = key.Modifiers.HasFlag(ConsoleModifiers.Control);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    return buffer.ToString();

                case ConsoleKey.Escape:
                    return ExitCommand;

                case ConsoleKey.Backspace when cursorPos > 0:
                    var backTo = isCtrl ? GetPrevWordPos(cursorPos) : cursorPos - 1;
                    buffer.Remove(backTo, cursorPos - backTo);
                    cursorPos = backTo;
                    RedrawInput(buffer, cursorPos);
                    break;

                case ConsoleKey.Delete when cursorPos < buffer.Length:
                    var delTo = isCtrl ? GetNextWordPos(cursorPos) : cursorPos + 1;
                    buffer.Remove(cursorPos, delTo - cursorPos);
                    RedrawInput(buffer, cursorPos);
                    break;

                case ConsoleKey.LeftArrow when cursorPos > 0:
                    cursorPos = isCtrl ? GetPrevWordPos(cursorPos) : cursorPos - 1;
                    RedrawInput(buffer, cursorPos);
                    break;

                case ConsoleKey.RightArrow when cursorPos < buffer.Length:
                    cursorPos = isCtrl ? GetNextWordPos(cursorPos) : cursorPos + 1;
                    RedrawInput(buffer, cursorPos);
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

        var startTime = DateTime.Now;
        
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
                    {
                        Console.Clear();
                        continue;
                    }

                    Document.Save(filePath, configPath, config, xmlDoc);
                    GetResults(filePath, translatedLinesCount, startTime);
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
        GetResults(filePath, translatedLinesCount, startTime);
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

    private static void GetResults(string filePath, int translatedLinesCount, DateTime startTime)
    {
        var transLines = Count.Translated(filePath);
        var engLines = Count.English(filePath);

        var elapsed = DateTime.Now - startTime;
        var secPerLine = translatedLinesCount > 0 ? Math.Round(elapsed.TotalSeconds / translatedLinesCount, 1) : 0;

        Color.Write($"Saved as <=Green>'{filePath}'</>.\n");
        Color.Write($"Translated <=Green>{translatedLinesCount}</> lines, <=Green>{transLines}</> in total.\n");
        Color.Write($"<=Green>{engLines}</> English lines left.\n");
        Color.Write($"Time: <=Green>{elapsed.TotalMinutes:F1}</> minutes.\n");
        Color.Write($"≈ <=Green>{secPerLine}</> sec/line.\n");

        Console.ReadKey();
    }
}