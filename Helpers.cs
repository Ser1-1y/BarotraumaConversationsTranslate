namespace Translate;

public static class Helpers
{
    public static string GetFilePath(Config config, string configPath, StringComparison oic)
    {
        Console.Clear();
        if (!config.FirstTimeUsing && !string.IsNullOrEmpty(config.LastFile))
            return config.LastFile;
        
        Console.Write("Input XML file path: ");
        var path = Console.ReadLine() ?? throw new InvalidOperationException();

        switch (path.EndsWith(".xml", oic))
        {
            case false when File.Exists(path + ".xml"):
                path += ".xml";
                break;
            case false:
                Console.WriteLine("Could not find such file");
                Console.ReadKey();
                GetFilePath(config, configPath, oic);
                break;
        }
            
        Config.Write(configPath, config);
        return path;
    }
}

public static class Color
{
    public static void Write(string text)
    {
        var parts = text.Split('<', '>');
        foreach (var part in parts)
        {
            if (part.StartsWith("/")) Console.ResetColor();
            else if (part.StartsWith("=") && Enum.TryParse(part[1..], out ConsoleColor color)) Console.ForegroundColor = color;
            else Console.Write(part);
        }
    }
}
