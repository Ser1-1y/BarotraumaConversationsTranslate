using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Translate;

public enum TranslationResult
{
    Completed,
    ExitRequested,
    BackToMenu
}

internal static class Document
{
    private static void RedrawInput(StringBuilder buffer, int cursorPos)
    {
        Console.Write("\r"); // Move to start of line
        Console.Write(buffer.ToString() + " "); // Draw text + clear leftover char
        Console.SetCursorPosition(cursorPos, Console.CursorTop);
    }

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
        if (tags != null)
            Color.Write($" <=Green>Tags:</> {tags.Value}\n");
        else
            Console.WriteLine();

        var buffer = new StringBuilder();
        var cursorPos = 0;

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
                return "__EXIT__";

            if (key.Key == ConsoleKey.Backspace)
            {
                if (cursorPos > 0)
                {
                    buffer.Remove(cursorPos - 1, 1);
                    cursorPos--;
                    RedrawInput(buffer, cursorPos);
                }
                continue;
            }

            if (key.Key == ConsoleKey.Delete)
            {
                if (cursorPos < buffer.Length)
                {
                    buffer.Remove(cursorPos, 1);
                    RedrawInput(buffer, cursorPos);
                }
                continue;
            }

            if (key.Key == ConsoleKey.LeftArrow)
            {
                if (cursorPos > 0)
                {
                    cursorPos--;
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                }
                continue;
            }

            if (key.Key == ConsoleKey.RightArrow)
            {
                if (cursorPos < buffer.Length)
                {
                    cursorPos++;
                    Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                }
                continue;
            }

            if (key.Key == ConsoleKey.Home)
            {
                Console.SetCursorPosition(Console.CursorLeft - cursorPos, Console.CursorTop);
                cursorPos = 0;
                continue;
            }

            if (key.Key == ConsoleKey.End)
            {
                Console.SetCursorPosition(Console.CursorLeft + (buffer.Length - cursorPos), Console.CursorTop);
                cursorPos = buffer.Length;
                continue;
            }

            if (char.IsControl(key.KeyChar)) 
                continue;
            buffer.Insert(cursorPos, key.KeyChar);
            cursorPos++;
            RedrawInput(buffer, cursorPos);
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
            return TranslationResult.BackToMenu;
        }

        foreach (XmlNode conv in nodes)
        {
            var line = conv.Attributes?["line"];
            var tags = conv.Attributes?["speakertags"];
            if (line == null
                || string.IsNullOrWhiteSpace(line.Value)
                || line.Value == "..."
                || NumbersAndDots.IsMatch(line.Value))
            {
                continue;
            }

            if (Cyrillic.IsMatch(line.Value))
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
                return TranslationResult.ExitRequested;
            }

            if (translatedLine.Equals("Settings", StringComparison.OrdinalIgnoreCase))
            {
                Document.Save(filePath, configPath, config, xmlDoc);
                Menu.SettingsMenu(configPath, config);
                return TranslationResult.BackToMenu;
            }

            line.Value = translatedLine;
            translatedLinesCount++;
        }

        Document.Save(filePath, configPath, config, xmlDoc);
        GetResults(filePath, translatedLinesCount);
        return TranslationResult.Completed;
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