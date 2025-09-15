namespace Translate;

public static class Program
{
    private const string ConfigPath = "config.json";

    public static void Main()
    {
        var config = Config.TryGet(ConfigPath);

        Menu.StartMenu(ConfigPath, config);
        
        Console.ReadKey();
    }
}