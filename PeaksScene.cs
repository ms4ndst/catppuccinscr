using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CatppuccinCoast;

// ---------------------------------------------------------------------------
// Mountain range — procedural ridgeline silhouette
// ---------------------------------------------------------------------------

sealed class MountainRange
{
    readonly StreamGeometry _geom;
    readonly SolidColorBrush _brush;

    public MountainRange(double w, double h, double baseY, Color color, byte alpha,
                         double peakMin, double peakMax, double jagged, int seed)
    {
        _brush = new SolidColorBrush(Palettes.WithAlpha(color, alpha));
        _brush.Freeze();
        var rng = new Random(seed);
        _geom = new StreamGeometry();
        using var ctx = _geom.Open();
        ctx.BeginFigure(new Point(0, h), true, true);

        // Generate ridgeline points
        double step = 6;
        double prevY = baseY;
        for (double x = 0; x <= w + step; x += step)
        {
            // Multi-octave noise approximation
            double n1 = Math.Sin(x * 0.003 + seed) * peakMax * 0.6;
            double n2 = Math.Sin(x * 0.009 + seed * 1.7) * peakMax * 0.25;
            double n3 = Math.Sin(x * 0.025 + seed * 3.1) * peakMax * jagged;
            double y = baseY - peakMin - Math.Max(0, n1 + n2 + n3);
            // Smooth a little
            double sy = prevY * 0.3 + y * 0.7;
            ctx.LineTo(new Point(x, sy), false, false);
            prevY = sy;
        }
        ctx.LineTo(new Point(w, h), false, false);
        _geom.Freeze();
    }

    public void Draw(DrawingContext dc) => dc.DrawGeometry(_brush, null, _geom);
}

// ---------------------------------------------------------------------------
// Snow particles — gently falling and drifting
// ---------------------------------------------------------------------------

sealed class SnowField
{
    record struct Flake(double X, double Y, double Vy, double Drift, double Size, double Phase);
    Flake[] _flakes;
    readonly double _w, _h;
    readonly Color _color;
    double _t;

    public SnowField(int count, double w, double h, Palette p)
    {
        _w = w; _h = h;
        _color = p.Text;
        _flakes = new Flake[count];
        for (int i = 0; i < count; i++) _flakes[i] = Spawn(true);
    }

    Flake Spawn(bool init = false) => new(
        Util.Rand(0, _w),
        init ? Util.Rand(0, _h) : Util.Rand(-30, -2),
        Util.Rand(18, 50),
        Util.Rand(-15, 15),
        Util.Rand(1.0, 2.8),
        Util.Rand(0, Math.Tau));

    public void Update(double dt)
    {
        _t += dt;
        for (int i = 0; i < _flakes.Length; i++)
        {
            var f = _flakes[i];
            double nx = f.X + (f.Drift + 8 * Math.Sin(_t * 0.5 + f.Phase)) * dt;
            double ny = f.Y + f.Vy * dt;
            if (ny > _h + 10 || nx < -30 || nx > _w + 30)
                _flakes[i] = Spawn();
            else
                _flakes[i] = f with { X = nx, Y = ny };
        }
    }

    public void Draw(DrawingContext dc)
    {
        foreach (var f in _flakes)
        {
            byte a = (byte)Util.Clamp(120 + 60 * Math.Sin(_t * 0.8 + f.Phase), 60, 200);
            dc.DrawEllipse(new SolidColorBrush(Palettes.WithAlpha(_color, a)),
                null, new Point(f.X, f.Y), f.Size, f.Size);
        }
    }
}

// ---------------------------------------------------------------------------
// Mountain fog — low gradient bands at mountain bases
// ---------------------------------------------------------------------------

sealed class MountainFog
{
    readonly double _w;
    readonly (double Y, double H, byte A, double Phase)[] _bands;
    readonly Color _color;
    double _t;

    public MountainFog(double w, double baseY, int count, Color color)
    {
        _w = w; _color = color;
        _bands = Enumerable.Range(0, count).Select(_ => (
            Util.Rand(baseY - 40, baseY + 30),
            Util.Rand(18, 45),
            (byte)Util.RandInt(12, 35),
            Util.Rand(0, Math.Tau)
        )).ToArray();
    }

    public void Update(double dt) => _t += dt;

    public void Draw(DrawingContext dc)
    {
        foreach (var (y, h, a, phase) in _bands)
        {
            double pa = a * (0.5 + 0.5 * Math.Sin(_t * 0.06 + phase));
            var brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0); brush.EndPoint = new Point(0, 1);
            brush.GradientStops.Add(new GradientStop(Palettes.WithAlpha(_color, 0), 0.0));
            brush.GradientStops.Add(new GradientStop(Palettes.WithAlpha(_color, (byte)pa), 0.5));
            brush.GradientStops.Add(new GradientStop(Palettes.WithAlpha(_color, 0), 1.0));
            dc.DrawRectangle(brush, null, new Rect(0, y, _w, h));
        }
    }
}

// ---------------------------------------------------------------------------
// Boosted Aurora — more bands, higher alpha, wider
// ---------------------------------------------------------------------------

sealed class VividAurora
{
    record struct Band(double Y, Color Clr, double Phase, double Speed, double Amp, double Freq, byte Alpha);
    readonly Band[] _bands;
    readonly double _w;
    double _t;

    public VividAurora(double w, double h, Palette p, int bandCount = 6)
    {
        _w = w;
        var colors = new[] { p.Mauve, p.Teal, p.Blue, p.Green, p.Lavender, p.Pink };
        _bands = new Band[bandCount];
        for (int i = 0; i < bandCount; i++)
            _bands[i] = new(h * Util.Rand(0.05, 0.42), Util.Pick(colors),
                Util.Rand(0, Math.Tau), Util.Rand(0.12, 0.35),
                Util.Rand(22, 50), Util.Rand(0.002, 0.006), (byte)Util.RandInt(28, 65));
    }

    public void Update(double dt) => _t += dt;

    public void Draw(DrawingContext dc)
    {
        foreach (var b in _bands)
        {
            var geom = new StreamGeometry();
            using (var ctx = geom.Open())
            {
                var top = new List<Point>(); var bot = new List<Point>();
                for (double x = 0; x <= _w + 8; x += 8)
                {
                    double off = b.Amp * Math.Sin(b.Freq * x + _t * b.Speed + b.Phase);
                    double wave2 = b.Amp * 0.3 * Math.Sin(b.Freq * 2.3 * x - _t * b.Speed * 0.6 + b.Phase * 1.5);
                    top.Add(new(x, b.Y + off + wave2 - 24));
                    bot.Add(new(x, b.Y + off + wave2 + 24));
                }
                ctx.BeginFigure(top[0], true, true);
                for (int i = 1; i < top.Count; i++) ctx.LineTo(top[i], false, false);
                for (int i = bot.Count - 1; i >= 0; i--) ctx.LineTo(bot[i], false, false);
            }
            geom.Freeze();
            dc.DrawGeometry(new SolidColorBrush(Palettes.WithAlpha(b.Clr, b.Alpha)), null, geom);
        }
    }
}

// ---------------------------------------------------------------------------
// Peaks scene orchestrator
// ---------------------------------------------------------------------------

public sealed class PeaksScene : IScene
{
    readonly StarField       _stars;
    readonly ShootingStarField _shooting;
    readonly CelestialBody   _celestial;
    readonly VividAurora     _aurora;
    readonly MountainRange   _mtnFar, _mtnMid, _mtnNear;
    readonly SnowField       _snow;
    readonly MountainFog     _fog;
    readonly AppSettings     _s;
    readonly Palette         _p;
    readonly string          _tod;
    readonly double          _w, _h;
    readonly double          _horizonY;

    public PeaksScene(AppSettings s, double w, double h)
    {
        _s = s; _w = w; _h = h;
        _p = Palettes.All.GetValueOrDefault(s.Flavor, Palettes.All["mocha"]);
        _tod = s.EffectiveTimeOfDay;
        _horizonY = h * 0.62;

        int starCount = s.StarDensity switch { "sparse" => 60, "dense" => 450, _ => 250 };
        _stars    = new StarField(starCount, w, h, _p);
        _shooting = new ShootingStarField(3, w, h, _p);
        _celestial = new CelestialBody(w * 0.80, h * 0.11, _p, _tod);
        _aurora   = new VividAurora(w, h, _p, 6);

        _mtnFar  = new MountainRange(w, h, _horizonY - h * 0.06, _p.Surface1, 160, h * 0.08, h * 0.22, 0.08, 17);
        _mtnMid  = new MountainRange(w, h, _horizonY,            _p.Surface0, 200, h * 0.05, h * 0.28, 0.10, 53);
        _mtnNear = new MountainRange(w, h, _horizonY + h * 0.08, _p.Mantle,   230, h * 0.03, h * 0.18, 0.12, 91);

        _snow = new SnowField(100, w, h, _p);
        _fog  = new MountainFog(w, _horizonY, 5, _p.Overlay0);
    }

    public void Update(double dt)
    {
        _stars.Update(dt);
        if (_s.ShowShooting) _shooting.Update(dt);
        _celestial.Update(dt);
        _aurora.Update(dt);
        _snow.Update(dt);
        _fog.Update(dt);
    }

    public void Draw(DrawingContext dc, double w, double h, double ppd)
    {
        // 1. Sky gradient
        Background.DrawSky(dc, w, h, _p, _tod);

        // 2. Aurora (always prominent in this scene)
        if (_tod is "night" or "dusk") _aurora.Draw(dc);

        // 3. Stars
        double starOp = _tod switch { "day" or "morning" => 0.0, "dusk" => 0.3, _ => 1.0 };
        _stars.Draw(dc, starOp);
        if (_s.ShowShooting && _tod == "night") _shooting.Draw(dc);

        // 4. Celestial body
        _celestial.Draw(dc);

        // 5. Far mountains
        _mtnFar.Draw(dc);

        // 6. Mid mountains
        _mtnMid.Draw(dc);

        // 7. Fog between layers
        _fog.Draw(dc);

        // 8. Near mountains
        _mtnNear.Draw(dc);

        // 9. Ground fill below near mountains
        dc.DrawRectangle(new SolidColorBrush(Palettes.WithAlpha(_p.Crust, 240)),
            null, new Rect(0, _horizonY + h * 0.20, w, h));

        // 10. Snow
        _snow.Draw(dc);

        // 11. Clock
        if (_s.ShowClock)
            ClockOverlay.Draw(dc, w, h, _p, ppd, _s.ClockPos, _s.ClockFormat);
    }
}
