# Catppuccin Coast Screensaver

A Windows screensaver featuring a coastal night scene built with the
[Catppuccin Mocha](https://github.com/catppuccin/catppuccin) color palette.

## Scene

| Feature              | Description                                                   |
|----------------------|---------------------------------------------------------------|
| Night sky            | Gradient from Crust → Base, 220 twinkling stars               |
| Shooting stars       | 3 concurrent with fade trail                                  |
| Crescent moon        | Yellow body with Lavender glow, slowly pulses                 |
| Aurora borealis      | 4 sinusoidal bands in Mauve/Teal/Green/Lavender               |
| Ocean waves          | 5 depth layers, Sky/Sapphire/Blue/Teal, double-sine motion    |
| Moonlight reflection | 14 shimmering streaks on the water surface                    |
| Seafoam particles    | 90 foam dots riding the front wave, drifting upward           |
| Distant lights       | 2 lighthouse-style blinks on the horizon                      |
| Clock overlay        | Current time + date, bottom-right, Catppuccin Text color      |

## Files

```
catppuccinscr/
├── catppuccin_coast.py     Source code
├── catppuccin_coast.spec   PyInstaller build spec
├── build.ps1               Build script (compile → .scr)
├── install.ps1             Install script (copy to System32 + set registry)
├── dist/
│   └── catppuccin_coast.scr  Ready-to-install screensaver (~28 MB)
├── MOCKUP.md               Design mock-up with palette table
└── README.md               This file
```

## Quick Install (pre-built)

1. Open PowerShell **as Administrator**
2. Navigate to this folder
3. Run:
   ```powershell
   .\install.ps1
   ```
   This copies the `.scr` to `C:\Windows\System32\` and configures the
   registry to activate it after **5 minutes** of inactivity.

## Manual Install

If you prefer full control:

```powershell
# Copy the screensaver
Copy-Item dist\catppuccin_coast.scr "$env:SystemRoot\System32\" -Force

# Open screensaver picker and choose it from the list
Start-Process "control.exe" "desk.cpl,,@screensaver"
```

Then select **catppuccin_coast** from the drop-down.

## Preview Without Installing

```powershell
# Full-screen preview (press any key to exit)
& "dist\catppuccin_coast.scr" /s

# Config dialog
& "dist\catppuccin_coast.scr" /c
```

## Build From Source

Requirements:
```powershell
pip install pygame-ce pyinstaller
```

Build:
```powershell
.\build.ps1
```

The script runs PyInstaller and renames the output `.exe` to `.scr`.

## Customization

Open `catppuccin_coast.py` and adjust the constants at the top of the file:

| Variable          | Default | Effect                                  |
|-------------------|---------|-----------------------------------------|
| `Stars` count     | 220     | Number of stars in the sky              |
| `ShootingStar`    | 3       | Concurrent shooting stars               |
| Wave `amplitude`  | 14–30   | Wave height per layer                   |
| Wave `speed`      | 0.35–1  | Wave animation speed per layer          |
| Aurora `alpha`    | 18–45   | Aurora intensity (increase for vivid)   |
| `Foam` count      | 90      | Number of foam particles                |
| Clock font size   | 7.2% h  | Proportional to screen height           |

To switch flavors, replace the palette constants at the top of the file with
the hex values from any of the four Catppuccin flavors (Latte, Frappé,
Macchiato, Mocha). The full palette is in `MOCKUP.md`.

After editing, rebuild with `.\build.ps1` and re-run `.\install.ps1`.

## Compatibility

| Windows Version | Status   |
|-----------------|----------|
| Windows 11      | Tested   |
| Windows 10      | Expected |
| Windows 8.1     | Expected |

Requires no runtime dependencies — everything is bundled in the `.scr` file.

## License

MIT — matching the Catppuccin project license.
Colors and palette © [Catppuccin](https://github.com/catppuccin/catppuccin).
