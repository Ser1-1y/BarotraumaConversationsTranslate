using Newtonsoft.Json;

namespace Translate;

public class Config
{
    public string Version { get; set; } = "2.0.0.0";
    public bool FirstTimeUsing { get; set; } = true;
    public bool ShowLoadedStrings { get; set; }
    public string? LastFile { get; set; }
    public bool DebugMode { get; set; }
    public bool ShowOriginalNodes { get; set; }

    public static Config TryGet(string configPath)
    {
        if (File.Exists(configPath))
        {
            try
            {
                return Read(configPath);
            }
            catch
            {
                Color.Write("<=Red>Config file is corrupted. Resetting...</>\n");
                return new Config();
            }
        }

        Menu.ConfigMenu(configPath);
        return Read(configPath);
    }

    private static Config Read(string path) =>
        JsonConvert.DeserializeObject<Config>(File.ReadAllText(path)) ?? new Config();

    public static void Write(string path, Config config) =>
        File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
}