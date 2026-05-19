# Catppuccin Coast Screensaver

A Windows screensaver built with the
[Catppuccin](https://github.com/catppuccin/catppuccin) pastel color palette.
Choose from four animated scenes, all four Catppuccin flavors, and five
time-of-day moods — or let it follow the system clock automatically.

## Scenes

### Coast (default)

Coastal scene with layered ocean waves, crescent moon, aurora borealis,
lighthouse beam, bioluminescent glow, seafoam particles, distant lights, and a
floating cat mascot.

### Firefly Forest

A tranquil forest clearing with three layers of procedural tree silhouettes,
60 pulsing firefly particles (Green/Yellow/Peach/Teal), a still pond with
ripple reflections, and drifting mist bands.

### Aurora Peaks

Mountain range silhouettes beneath a vivid 6-band aurora. Three parallax
mountain layers, 100 gently drifting snow particles, and fog bands between
the ridgelines.

### Lofi Room

A cozy interior: a window showing the night sky with rain streaks, a desk with
monitor, keyboard, mug (with rising steam), potted plant, and a cat silhouette
with glowing blinking eyes. String lights with catenary sag drape across the
top in Rosewater/Flamingo/Yellow/Peach/Pink.

### Meadow (NEW)

A peaceful meadow with three layers of swaying grass blades, colorful
butterflies (Pink/Mauve/Peach/Yellow) following erratic flight paths,
wildflowers with animated petals, drifting clouds, and flying birds (visible
during day/morning). All nature elements animate in harmony with the breeze.

### SummerSky (NEW)

Bright summer sky with radial sun rays, fluffy cumulus clouds, colorful hot
air balloons drifting across the horizon, diamond-shaped kites with wavy tails,
and birds soaring through the brilliant blue gradient. Pure daytime scene with
vibrant Sky/Sapphire/Teal palette.

### GentleBeach (NEW)

Serene coastal scene with gentle rolling waves (calmer than Coast), swaying
palm trees, scattered seashells and starfish on sandy shore, surf foam bubbles,
colorful beach umbrellas, and seagulls gliding overhead. Features warm
Yellow/Peach sand gradients and tranquil Sapphire/Teal ocean.

## Time of Day

| Mode    | Auto hours  | Sky palette                                       | Celestial |
|---------|-------------|---------------------------------------------------|-----------|
| Night   | 21:00–5:59  | Crust → Base → Surface1/Sapphire                  | Moon      |
| Morning | 6:00–9:59   | Sky/Lavender → Rosewater/Flamingo → Peach/Yellow  | Sun       |
| Day     | 10:00–16:59 | Sky/White → Sapphire → Teal                       | Sun       |
| Dusk    | 17:00–20:59 | Mantle/Mauve → Peach → Yellow                     | Moon      |
| Auto    | —           | Selects one of the above from the system clock    | —         |

Stars, aurora, fireflies, bioluminescence, and shooting stars are only
visible at night (and faintly at dusk).

## Settings

The screensaver has a built-in settings dialog (accessible from the Windows
screensaver picker or via `/c`). Available options:

- **Scene** — Coast, Forest, Peaks, Lofi, Meadow, SummerSky, GentleBeach
- **Flavor** — Latte, Frappé, Macchiato, Mocha
- **Time of Day** — Auto, Night, Morning, Day, Dusk
- **Features** — Toggle clock, aurora, shooting stars, seafoam, bioluminescence, lighthouse, rain, butterflies, wildflowers, flying birds, clouds, sun rays, hot air balloons, kites, palm trees, seashells, starfish, beach umbrellas, surf foam
- **Wave Speed** — Calm, Normal, Stormy (rain only appears in Stormy)
- **Wave Layers** — Few (3), Normal (5), Many (7)
- **Star Density** — Sparse, Normal, Dense
- **Cat Size** — Small, Medium, Large
- **Clock** — 24h / 12h format; four corner positions

Settings are saved to `%AppData%\CatppuccinCoast\settings.json`.

## Files

```text
catppuccinscr/
├── CoastScene.cs          Coast scene + shared visual elements
├── ForestScene.cs         Firefly Forest scene
├── PeaksScene.cs          Aurora Peaks scene
├── LofiScene.cs           Lofi Room scene
├── MeadowScene.cs         Meadow scene
├── SummerSkyScene.cs      SummerSky scene
├── GentleBeachScene.cs    GentleBeach scene
├── IScene.cs              Scene interface
├── SceneFactory.cs        Scene selector
├── Palettes.cs            All 4 Catppuccin flavors (26 colors each)
├── AppSettings.cs         Settings model + JSON persistence
├── SettingsWindow.cs      WPF settings dialog
├── ScreensaverWindow.cs   Fullscreen host + SceneHost renderer
├── Program.cs             Entry point (/s, /c, /p modes)
├── NativeMethods.cs       Win32 interop
├── CatppuccinCoast.csproj .NET 10 WPF project
├── assets/
│   └── catppuccin_cat.png Cat mascot sprite
├── build_cs.ps1           Build script (dotnet publish → dist_cs/)
├── install.ps1            Install script (copy + registry, run as Admin)
├── MOCKUP.md              Original design mock-up
└── README.md              This file
```

## Quick Install

1. Open PowerShell **as Administrator**
2. Navigate to this folder
3. Build and install:

   ```powershell
   .\build_cs.ps1
   .\install.ps1
   ```

   This publishes to `dist_cs\`, copies files to
   `C:\Program Files\CatppuccinCoast\`, and sets the registry to activate
   the screensaver after **5 minutes** of inactivity.

## Preview Without Installing

```powershell
# Full-screen (press any key / move mouse to exit)
& "dist_cs\catppuccin_coast.exe" /s

# Settings dialog
& "dist_cs\catppuccin_coast.exe" /c
```

## Build From Source

Requirements: [.NET 10 SDK](https://dotnet.microsoft.com/download)

```powershell
.\build_cs.ps1
```

This runs `dotnet publish` targeting `win-x64` (framework-dependent) and
outputs to `dist_cs\`.

## Compatibility

| Windows Version | Status   |
|-----------------|----------|
| Windows 11      | Tested   |
| Windows 10      | Expected |

Requires the .NET 10 runtime (framework-dependent deployment).

## License

MIT — matching the Catppuccin project license.
Colors and palette © [Catppuccin](https://github.com/catppuccin/catppuccin).
