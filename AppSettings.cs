using System;
using System.IO;
using System.Text.Json;

namespace CatppuccinCoast;

public sealed class AppSettings
{
    // Existing
    public string Scene        { get; set; } = "coast";     // coast | forest | peaks | lofi
    public string Flavor       { get; set; } = "mocha";
    public bool   ShowClock    { get; set; } = true;
    public bool   ShowAurora   { get; set; } = true;
    public bool   ShowShooting { get; set; } = true;
    public bool   ShowFoam     { get; set; } = true;
    public double WaveSpeed    { get; set; } = 1.0;
    public string CatSize      { get; set; } = "medium";

    // New
    public string TimeOfDay    { get; set; } = "auto";         // auto | night | morning | dusk | day

    /// <summary>Resolves "auto" to a concrete time-of-day based on the system clock.</summary>
    public string EffectiveTimeOfDay
    {
        get
        {
            if (TimeOfDay != "auto") return TimeOfDay;
            int h = DateTime.Now.Hour;
            return h switch
            {
                >= 6 and < 10  => "morning",
                >= 10 and < 17 => "day",
                >= 17 and < 21 => "dusk",
                _              => "night",
            };
        }
    }
    public string ClockPos     { get; set; } = "bottom-right"; // bottom-right | bottom-left | top-right | top-left
    public string ClockFormat  { get; set; } = "24h";          // 24h | 12h
    public string StarDensity  { get; set; } = "normal";       // sparse | normal | dense
    public string WaveLayers   { get; set; } = "normal";       // few | normal | many
    public bool   ShowBio      { get; set; } = true;           // bioluminescence glow in waves
    public bool   ShowLighthouse { get; set; } = true;         // lighthouse + rotating beam
    public bool   ShowRain     { get; set; } = true;           // rain when WaveSpeed is stormy
    public bool   ShowButterflies { get; set; } = true;        // butterflies in meadow scene
    public bool   ShowFlowers  { get; set; } = true;           // wildflowers in meadow scene
    public bool   ShowBirds    { get; set; } = true;           // flying birds in meadow/summersky/gentlebeach scenes
    public bool   ShowClouds   { get; set; } = true;           // drifting clouds in meadow/summersky scenes
    public bool   ShowSunRays  { get; set; } = true;           // sun rays in summersky scene
    public bool   ShowBalloons { get; set; } = true;           // hot air balloons in summersky scene
    public bool   ShowKites    { get; set; } = true;           // kites in summersky scene
    public bool   ShowPalms    { get; set; } = true;           // palm trees in gentlebeach scene
    public bool   ShowShells   { get; set; } = true;           // seashells in gentlebeach scene
    public bool   ShowStarfish { get; set; } = true;           // starfish in gentlebeach scene
    public bool   ShowUmbrellas { get; set; } = true;          // beach umbrellas in gentlebeach scene
    public bool   ShowSurfFoam { get; set; } = true;           // surf foam in gentlebeach scene

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
