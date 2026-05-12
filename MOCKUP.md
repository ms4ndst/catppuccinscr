# Catppuccin Coast — Design Mockup

## Visual Layout

```
╔══════════════════════════════════════════════════════════════════════════════╗
║  #11111b (Crust)                                                             ║
║                    ★  ✦   ★      ★                                           ║
║        ★    ✦          ★             ✦      ★                                ║
║  ─ ─ ─ ─ ─ ─ ─ ─ Aurora shimmer (Mauve/Teal/Lavender, α=30) ─ ─ ─ ─ ─ ─  ║
║  ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─  ║
║                                                                              ║
║         ◉  ← Moon (Yellow glow + Lavender halo)           [shooting star ↘] ║
║                                                                              ║
║  #1e1e2e (Base) sky gradient                                                 ║
║                                                                              ║
║  ─────────────── horizon line ───────────────── · · ← distant lights (·)   ║
║  ~~~~~~~~~~~~~~~ Wave 5 (Sapphire, depth 4) ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  ║
║  ─│─│─│─│─ moonlight reflection streaks (Yellow, α=30) ─│─│─│─│─│─│─│─│─  ║
║  ~~~~~~~~~~~~~ Wave 4 (Blue, depth 3) ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ∿  ║
║  ~~~~~~~~~~~ Wave 3 (Teal, depth 2) ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ∿  ║
║  ~~~~~~~~~ Wave 2 (Sapphire, depth 1) ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ∿  ║
║  ~~~~~~~ Wave 1 (Sky, front) ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ∿  ║
║  ·   · ·  ← seafoam particles (Sky/Teal/Lavender, drifting upward)          ║
║                                                                              ║
║  #11111b (Crust) deep ocean                              22:47  ←─ clock    ║
║                                                      Monday, May 12         ║
╚══════════════════════════════════════════════════════════════════════════════╝
```

## Color Palette Used — Catppuccin Mocha

| Role              | Color Name | Hex       | RGB              |
|-------------------|------------|-----------|------------------|
| Sky top           | Crust      | `#11111b` | (17,  17,  27)   |
| Sky mid           | Base       | `#1e1e2e` | (30,  30,  46)   |
| Sky horizon       | Surface1   | `#45475a` | (69,  71,  90)   |
| Ocean surface     | Sapphire   | `#74c7ec` | (116, 199, 236)  |
| Ocean mid         | Blue       | `#89b4fa` | (137, 180, 250)  |
| Ocean shallow     | Sky        | `#89dceb` | (137, 220, 235)  |
| Seafoam           | Teal       | `#94e2d5` | (148, 226, 213)  |
| Moon body         | Yellow     | `#f9e2af` | (249, 226, 175)  |
| Moon halo         | Lavender   | `#b4befe` | (180, 190, 254)  |
| Stars / text      | Text       | `#cdd6f4` | (205, 214, 244)  |
| Star dim          | Subtext0   | `#a6adc8` | (166, 173, 200)  |
| Aurora 1          | Mauve      | `#cba6f7` | (203, 166, 247)  |
| Aurora 2          | Green      | `#a6e3a1` | (166, 227, 161)  |
| Clock / glow      | Lavender   | `#b4befe` | (180, 190, 254)  |
| Deep ocean        | Crust      | `#11111b` | (17,  17,  27)   |

## Scene Elements

### Night Sky
- Vertical gradient: Crust → Base → Surface1 tinted with Sapphire
- 220 twinkling stars in Lavender/Blue/Sky/Text/Subtext0
- 3 concurrent shooting stars with trail fade
- Crescent moon with Yellow body + Lavender glow rings

### Aurora Borealis
- 4 overlapping sinusoidal bands at random heights
- Colors: Mauve, Teal, Blue, Green, Lavender
- Very low alpha (18–45) for subtle shimmer
- Animated phase offset per band

### Ocean
- 5 layered wave polygons using double sine for organic motion
- Back-to-front depth order: Sapphire → Blue → Teal → Sapphire → Sky
- Alpha increases toward the viewer (80 → 145)
- Moonlight reflection: 14 shimmering streaks under the moon
- 90 foam particles on the front wave crest, drifting upward

### Atmosphere
- 2 distant lighthouse/port lights blinking on the horizon
- Clock overlay (bottom-right): time in large Text, date in Subtext0
- Shadow offset on clock text for legibility against the ocean

## Interaction

| Input           | Action                  |
|-----------------|-------------------------|
| Any key         | Exit screensaver        |
| Mouse move >6px | Exit screensaver        |
| Mouse click     | Exit screensaver        |
| `/s` flag       | Run full screensaver    |
| `/c` flag       | Open config dialog      |
| `/p hwnd` flag  | Preview in small window |
