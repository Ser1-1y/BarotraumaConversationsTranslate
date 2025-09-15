namespace Translate;

public static class Helpers
{
    public static string GetFilePath(Config config, string configPath)
    {
        if (!config.FirstTimeUsing && !string.IsNullOrEmpty(config.LastFile))
            return config.LastFile;

        while (true)
        {
            var path = Menu.ChangeFilePath();

            if (path.StartsWith("\"") && path.EndsWith("\""))
                path = path[1..^1];

            if (!path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                path += ".xml";

            if (File.Exists(path))
            {
                config.LastFile = path;
                Config.Write(configPath, config);
                return path;
            }

            Console.WriteLine("Could not find such file. Press any key to retry.");
            Console.ReadKey();
        }
    }
}

public static class Color
{
    public static void Write(string text)
    {
        var parts = text.Split('<', '>');
        foreach (var part in parts)
        {
            if (part.StartsWith("/"))
                Console.ResetColor();
            else if (part.StartsWith("=") && Enum.TryParse(part[1..], out ConsoleColor color))
                Console.ForegroundColor = color;
            else
                Console.Write(part);
        }
    }
}