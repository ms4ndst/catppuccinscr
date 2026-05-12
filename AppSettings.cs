using System;
using System.IO;
using System.Text.Json;

namespace CatppuccinCoast;

public sealed class AppSettings
{
    // Existing
    public string Flavor       { get; set; } = "mocha";
    public bool   ShowClock    { get; set; } = true;
    public bool   ShowAurora   { get; set; } = true;
    public bool   ShowShooting { get; set; } = true;
    public bool   ShowFoam     { get; set; } = true;
    public double WaveSpeed    { get; set; } = 1.0;
    public string CatSize      { get; set; } = "medium";

    // New
    public string TimeOfDay    { get; set; } = "night";        // night | dusk | day
    public string ClockPos     { get; set; } = "bottom-right"; // bottom-right | bottom-left | top-right | top-left
    public string ClockFormat  { get; set; } = "24h";          // 24h | 12h
    public string StarDensity  { get; set; } = "normal";       // sparse | normal | dense
    public string WaveLayers   { get; set; } = "normal";       // few | normal | many
    public bool   ShowBio      { get; set; } = true;           // bioluminescence glow in waves
    public bool   ShowLighthouse { get; set; } = true;         // lighthouse + rotating beam
    public bool   ShowRain     { get; set; } = true;           // rain when WaveSpeed is stormy

    static readonly string Dir  = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CatppuccinCoast");
    static readonly string File = Path.Combine(Dir, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            var json = System.IO.File.ReadAllText(File);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new();
        }
        catch { return new(); }
    }

    public void Save()
    {
        Directory.CreateDirectory(Dir);
        System.IO.File.WriteAllText(File,
            JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
