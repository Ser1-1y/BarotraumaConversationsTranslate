namespace Translate;

public static class Program
{
    private const StringComparison Oic = StringComparison.OrdinalIgnoreCase;
    private const string ConfigPath = "config.json";

    public static void Main()
    {
        var config = Config.TryGet(ConfigPath);
        var filePath = Helpers.GetFilePath(config, ConfigPath, Oic);

        Menu.StartMenu(ConfigPath, config, filePath);
        
        Console.ReadKey();
    }
    
    public static void StartTranslation(string filePath, Config config)
    {
        Console.WriteLine("Press 'Escape' to quit.\n" +
                          "Write 'Settings' to access Settings.");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        
        Translation.Process(filePath, config, ConfigPath, Oic);
    }
}