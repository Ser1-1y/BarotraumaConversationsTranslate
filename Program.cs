namespace Translate;

public static class Program
{
    private const string ConfigPath = "config.json";

    public static void Main()
    {
        var config = Config.TryGet(ConfigPath);

        var running = true;
        while (running)
        {
            var result = Menu.StartMenu(ConfigPath, config);
            if (result == MenuResult.Exit)
                running = false;
        }
    }
}