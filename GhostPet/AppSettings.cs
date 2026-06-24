using System.IO;
using System.Text.Json;

namespace GhostPet;

public sealed class AppSettings
{
    public double? Left { get; set; }
    public double? Top { get; set; }
    public double SpriteScale { get; set; } = 1.0;

    private static readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GhostPet", "settings.json");

    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(_path)) return new();
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_path), _json) ?? new();
        }
        catch { return new(); }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(this, _json));
        }
        catch { }
    }
}
