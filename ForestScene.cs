using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CatppuccinCoast;

// ---------------------------------------------------------------------------
// Tree silhouettes — layered procedural outlines
// ---------------------------------------------------------------------------

sealed class TreeLayer
{
    readonly StreamGeometry _geom;
    readonly SolidColorBrush _brush;

    public TreeLayer(double w, double h, double baseY, Color color, byte alpha, int treeCount, double minH, double maxH, int seed)
    {
        _brush = new SolidColorBrush(Palettes.WithAlpha(color, alpha));
        _brush.Freeze();
        var rng = new Random(seed);
        _geom = new StreamGeometry();
        using var ctx = _geom.Open();
        ctx.BeginFigure(new Point(0, h), true, true);
        ctx.LineTo(new Point(0, baseY), false, false);

        double x = 0;
        while (x < w + 60)
        {
            double treeH = minH + rng.NextDouble() * (maxH - minH);
            double treeW = treeH * (0.3 + rng.NextDouble() * 0.35);
            double half = treeW / 2;
            double cx = x + half;
            // Trunk top
            double topY = baseY - treeH;
            // Build a simple triangular tree with slight variation
            ctx.LineTo(new Point(cx - half * 0.15, baseY), false, false);
            // Left slope with a notch
            ctx.LineTo(new Point(cx - half * 0.55, baseY - treeH * 0.45), false, false);
            ctx.LineTo(new Point(cx - half * 0.35, baseY - treeH * 0.40), false, false);
            ctx.LineTo(new Point(cx - half * 0.10, topY), false, false);
            // Right slope
            ctx.LineTo(new Point(cx + half * 0.10, topY + treeH * 0.06), false, false);
            ctx.LineTo(new Point(cx + half * 0.35, baseY - treeH * 0.42), false, false);
            ctx.LineTo(new Point(cx + half * 0.55, baseY - treeH * 0.47), false, false);
            ctx.LineTo(new Point(cx + half * 0.15, baseY), false, false);

            double gap = 8 + rng.NextDouble() * 30;
            x += treeW + gap;
            ctx.LineTo(new Point(Math.Min(x, w + 60), baseY), false, false);
        }
        ctx.LineTo(new Point(w, h), false, false);
        _geom.Freeze();
    }

    public void Draw(DrawingContext dc) => dc.DrawGeometry(_brush, null, _geom);
}

// ---------------------------------------------------------------------------
// Fireflies — glowing particles drifting through the clearing
// ---------------------------------------------------------------------------

sealed class FireflyField
{
    sealed class Fly
    {
        public double X, Y, Vx, Vy, Phase, GlowR, Speed;
        public Color Clr;
    }

    readonly List<Fly> _flies = [];
    readonly double _w, _h, _minY, _maxY;
    readonly Color[] _colors;
    double _t;

    public FireflyField(int count, double w, double h, double minY, double maxY, Palette p)
    {
        _w = w; _h = h; _minY = minY; _maxY = maxY;
        _colors = [p.Green, p.Yellow, p.Peach, p.Teal];
        for (int i = 0; i < count; i++) _flies.Add(Spawn(true));
    }

    Fly Spawn(bool init = false) => new()
    {
        X = Util.Rand(0, _w), Y = Util.Rand(_minY, _maxY),
        Vx = Util.Rand(-12, 12), Vy = Util.Rand(-8, 8),
        Phase = Util.Rand(0, Math.Tau), Speed = Util.Rand(0.8, 2.5),
        GlowR = Util.Rand(6, 16), Clr = Util.Pick(_colors)
    };

    public void Update(double dt)
    {
        _t += dt;
        foreach (var f in _flies)
        {
            f.X += f.Vx * dt + 4 * Math.Sin(_t * 0.3 + f.Phase) * dt;
            f.Y += f.Vy * dt + 3 * Math.Cos(_t * 0.4 + f.Phase) * dt;
            // Wrap around
            if (f.X < -20) f.X = _w + 10;
            if (f.X > _w + 20) f.X = -10;
            if (f.Y < _minY - 20) f.Y = _maxY;
            if (f.Y > _maxY + 20) f.Y = _minY;
        }
    }

    public void Draw(DrawingContext dc)
    {
        foreach (var f in _flies)
        {
            double pulse = 0.3 + 0.7 * Math.Max(0, Math.Sin(_t * f.Speed + f.Phase));
            byte a = (byte)Util.Clamp(pulse * 200, 0, 200);
            byte ga = (byte)Util.Clamp(pulse * 60, 0, 60);
            // Outer glow
            dc.DrawEllipse(new RadialGradientBrush(
                Palettes.WithAlpha(f.Clr, ga), Palettes.WithAlpha(f.Clr, 0)),
                null, new Point(f.X, f.Y), f.GlowR, f.GlowR);
            // Core
            dc.DrawEllipse(new SolidColorBrush(Palettes.WithAlpha(f.Clr, a)),
                null, new Point(f.X, f.Y), 2.0, 2.0);
        }
    }
}

// ---------------------------------------------------------------------------
// Mist — low-alpha horizontal bands drifting slowly
// ---------------------------------------------------------------------------

sealed class MistLayer
{
    record struct Band(double Y, double Phase, double Speed, double H, byte Alpha);
    readonly Band[] _bands;
    readonly double _w;
    readonly Color _color;
    double _t;

    public MistLayer(double w, double minY, double maxY, int count, Color color)
    {
        _w = w; _color = color;
        _bands = new Band[count];
        for (int i = 0; i < count; i++)
            _bands[i] = new(Util.Rand(minY, maxY), Util.Rand(0, Math.Tau),
                Util.Rand(0.02, 0.08), Util.Rand(20, 55), (byte)Util.RandInt(8, 28));
    }

    public void Update(double dt) => _t += dt;

    public void Draw(DrawingContext dc)
    {
        foreach (var b in _bands)
        {
            double shift = 60 * Math.Sin(_t * b.Speed + b.Phase);
            double pulsedAlpha = b.Alpha * (0.6 + 0.4 * Math.Sin(_t * b.Speed * 0.5 + b.Phase));
            var brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0); brush.EndPoint = new Point(1, 0);
            brush.GradientStops.Add(new GradientStop(Palettes.WithAlpha(_color, 0), 0.0));
            brush.GradientStops.Add(new GradientStop(Palettes.WithAlpha(_color, (byte)pulsedAlpha), 0.35));
            brush.GradientStops.Add(new GradientStop(Palettes.WithAlpha(_color, (byte)pulsedAlpha), 0.65));
            brush.GradientStops.Add(new GradientStop(Palettes.WithAlpha(_color, 0), 1.0));
            dc.DrawRectangle(brush, null, new Rect(shift - 40, b.Y, _w + 80, b.H));
        }
    }
}

// ---------------------------------------------------------------------------
// Pond — reflective band at the bottom with subtle ripples
// ---------------------------------------------------------------------------

sealed class Pond
{
    readonly double _w, _y, _h;
    readonly Palette _p;
    double _t;

    public Pond(double w, double pondY, double pondH, Palette p)
    { _w = w; _y = pondY; _h = pondH; _p = p; }

    public void Update(double dt) => _t += dt;

    public void Draw(DrawingContext dc)
    {
        // Water body
        var waterBrush = new LinearGradientBrush();
        waterBrush.StartPoint = new Point(0, 0); waterBrush.EndPoint = new Point(0, 1);
        waterBrush.GradientStops.Add(new GradientStop(
            Palettes.WithAlpha(Palettes.Lerp(_p.Sapphire, _p.Teal, 0.3), 120), 0.0));
        waterBrush.GradientStops.Add(new GradientStop(
            Palettes.WithAlpha(Palettes.Lerp(_p.Base, _p.Crust, 0.5), 180), 1.0));
        dc.DrawRectangle(waterBrush, null, new Rect(0, _y, _w, _h));

        // Ripple lines
        for (int i = 0; i < 8; i++)
        {
            double ry = _y + _h * (0.1 + i * 0.11);
            double phase = _t * 0.4 + i * 1.2;
            double cx = _w * 0.5 + _w * 0.15 * Math.Sin(phase);
            double rw = 40 + 25 * Math.Sin(phase * 0.7 + i);
            byte a = (byte)(15 + 10 * Math.Sin(_t * 0.6 + i * 0.8));
            dc.DrawLine(new Pen(new SolidColorBrush(Palettes.WithAlpha(_p.Lavender, a)), 1),
                new Point(cx - rw, ry), new Point(cx + rw, ry));
        }
    }
}

// ---------------------------------------------------------------------------
// Forest scene orchestrator
// ---------------------------------------------------------------------------

public sealed class ForestScene : IScene
{
    readonly StarField       _stars;
    readonly ShootingStarField _shooting;
    readonly CelestialBody   _celestial;
    readonly Aurora           _aurora;
    readonly TreeLayer       _treesBack, _treesMid, _treesFront;
    readonly FireflyField    _fireflies;
    readonly MistLayer       _mist;
    readonly Pond            _pond;
    readonly AppSettings     _s;
    readonly Palette         _p;
    readonly string          _tod;
    readonly double          _w, _h;
    readonly double          _clearingY;   // where sky meets treeline
    readonly double          _groundY;     // where trees meet pond

    public ForestScene(AppSettings s, double w, double h)
    {
        _s = s; _w = w; _h = h;
        _p = Palettes.All.GetValueOrDefault(s.Flavor, Palettes.All["mocha"]);
        _tod = s.EffectiveTimeOfDay;
        _clearingY = h * 0.32;
        _groundY   = h * 0.72;

        int starCount = s.StarDensity switch { "sparse" => 50, "dense" => 300, _ => 160 };
        _stars    = new StarField(starCount, w, h, _p);
        _shooting = new ShootingStarField(2, w, h, _p);
        _celestial = new CelestialBody(w * 0.72, h * 0.12, _p, _tod);
        _aurora   = new Aurora(w, h, _p);

        _treesBack  = new TreeLayer(w, h, _clearingY + 20, _p.Crust,  200, 14, h * 0.22, h * 0.38, 42);
        _treesMid   = new TreeLayer(w, h, _clearingY + 50, _p.Mantle, 220, 10, h * 0.28, h * 0.48, 87);
        _treesFront = new TreeLayer(w, h, _groundY,        _p.Crust,  240, 8,  h * 0.18, h * 0.35, 123);

        _fireflies = new FireflyField(60, w, h, _clearingY, _groundY, _p);
        _mist      = new MistLayer(w, _groundY - h * 0.12, _groundY + 10, 6, _p.Overlay0);
        _pond      = new Pond(w, _groundY, h - _groundY, _p);
    }

    public void Update(double dt)
    {
        _stars.Update(dt);
        if (_s.ShowShooting) _shooting.Update(dt);
        _celestial.Update(dt);
        if (_s.ShowAurora) _aurora.Update(dt);
        _fireflies.Update(dt);
        _mist.Update(dt);
        _pond.Update(dt);
    }

    public void Draw(DrawingContext dc, double w, double h, double ppd)
    {
        // 1. Sky
        Background.DrawSky(dc, w, _groundY, _p, _tod);

        // 2. Aurora
        if (_s.ShowAurora && _tod is "night" or "dusk") _aurora.Draw(dc);

        // 3. Stars
        double starOp = _tod switch { "day" or "morning" => 0.0, "dusk" => 0.2, _ => 1.0 };
        _stars.Draw(dc, starOp);
        if (_s.ShowShooting && _tod == "night") _shooting.Draw(dc);

        // 4. Celestial body
        _celestial.Draw(dc);

        // 5. Back trees
        _treesBack.Draw(dc);

        // 6. Mid trees
        _treesMid.Draw(dc);

        // 7. Pond
        _pond.Draw(dc);

        // 8. Front trees (frame the clearing)
        _treesFront.Draw(dc);

        // 9. Fireflies (in front of trees for glow visibility)
        if (_tod is "night" or "dusk") _fireflies.Draw(dc);

        // 10. Mist
        _mist.Draw(dc);

        // 11. Clock
        if (_s.ShowClock)
            ClockOverlay.Draw(dc, w, h, _p, ppd, _s.ClockPos, _s.ClockFormat);
    }
}
