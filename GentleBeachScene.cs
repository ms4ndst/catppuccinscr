using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CatppuccinCoast;

// ---------------------------------------------------------------------------
// Palm tree — tropical silhouette with swaying fronds
// ---------------------------------------------------------------------------

sealed class PalmTree
{
    readonly double _x, _baseY, _h;
    readonly SolidColorBrush _trunk, _frond;
    double _t;

    public PalmTree(double x, double baseY, double h, Palette p)
    {
        _x = x; _baseY = baseY; _h = h;
        _trunk = new SolidColorBrush(Palettes.WithAlpha(p.Maroon, 200));
        _frond = new SolidColorBrush(Palettes.WithAlpha(p.Green, 180));
        _trunk.Freeze();
        _frond.Freeze();
    }

    public void Update(double dt) => _t += dt;

    public void Draw(DrawingContext dc)
    {
        double sway = 12 * Math.Sin(_t * 0.5);
        double topX = _x + sway;
        double topY = _baseY - _h;

        // Trunk (curved)
        var trunk = new StreamGeometry();
        using (var ctx = trunk.Open())
        {
            ctx.BeginFigure(new Point(_x - 8, _baseY), false, false);
            ctx.BezierTo(
                new Point(_x - 4, _baseY - _h * 0.6),
                new Point(topX - 6, topY + _h * 0.3),
                new Point(topX, topY), true, false);
            ctx.BezierTo(
                new Point(topX + 6, topY + _h * 0.3),
                new Point(_x + 4, _baseY - _h * 0.6),
                new Point(_x + 8, _baseY), true, false);
        }
        trunk.Freeze();
        dc.DrawGeometry(_trunk, null, trunk);

        // Fronds (palm leaves)
        int frondCount = 6;
        for (int i = 0; i < frondCount; i++)
        {
            double angle = (i * Math.Tau / frondCount) + _t * 0.2;
            double len = _h * 0.6;
            double bendX = topX + Math.Cos(angle) * len * 0.7 + sway * 0.5;
            double bendY = topY + Math.Sin(angle) * len * 0.7 - Math.Abs(Math.Cos(angle)) * 20;

            var frond = new StreamGeometry();
            using (var ctx = frond.Open())
            {
                ctx.BeginFigure(new Point(topX, topY), false, false);
                ctx.QuadraticBezierTo(
                    new Point(topX + Math.Cos(angle) * len * 0.3, topY + Math.Sin(angle) * len * 0.3),
                    new Point(bendX, bendY), true, false);
            }
            frond.Freeze();
            dc.DrawGeometry(null, new Pen(_frond, 4), frond);
        }
    }
}

// ---------------------------------------------------------------------------
// Seashells — scattered on beach
// ---------------------------------------------------------------------------

sealed class ShellField
{
    sealed class Shell
    {
        public double X, Y, Size, Rotation, Phase;
        public Color Clr;
    }

    readonly List<Shell> _shells = [];
    double _t;

    public ShellField(double w, double beachY, int count, Palette p, int seed)
    {
        var colors = new[] { p.Rosewater, p.Flamingo, p.Pink, p.Peach, p.Yellow };
        var rng = new Random(seed);

        for (int i = 0; i < count; i++)
        {
            _shells.Add(new Shell
            {
                X = rng.NextDouble() * w,
                Y = beachY + rng.NextDouble() * 80,
                Size = 6 + rng.NextDouble() * 12,
                Rotation = rng.NextDouble() * Math.Tau,
                Phase = rng.NextDouble() * Math.Tau,
                Clr = colors[rng.Next(colors.Length)]
            });
        }
    }

    public void Update(double dt) => _t += dt;

    public void Draw(DrawingContext dc)
    {
        foreach (var s in _shells)
        {
            double shimmer = 0.7 + 0.3 * Math.Sin(_t * 0.5 + s.Phase);
            byte alpha = (byte)(150 * shimmer);
            
            dc.PushTransform(new RotateTransform(s.Rotation * 180 / Math.PI, s.X, s.Y));
            
            // Simple spiral shell shape
            var shell = new StreamGeometry();
            using (var ctx = shell.Open())
            {
                ctx.BeginFigure(new Point(s.X, s.Y), true, true);
                for (double a = 0; a < Math.Tau * 2; a += 0.3)
                {
                    double r = s.Size * a / (Math.Tau * 2);
                    ctx.LineTo(new Point(s.X + Math.Cos(a) * r, s.Y + Math.Sin(a) * r), false, false);
                }
            }
            shell.Freeze();
            dc.DrawGeometry(new SolidColorBrush(Palettes.WithAlpha(s.Clr, alpha)), null, shell);
            
            dc.Pop();
        }
    }
}

// ---------------------------------------------------------------------------
// Starfish — five-pointed beach decoration
// ---------------------------------------------------------------------------

sealed class StarfishField
{
    sealed class Starfish
    {
        public double X, Y, Size, Rotation;
        public Color Clr;
    }

    readonly List<Starfish> _starfish = [];

    public StarfishField(double w, double beachY, int count, Palette p, int seed)
    {
        var colors = new[] { p.Peach, p.Maroon, p.Red, p.Pink };
        var rng = new Random(seed);

        for (int i = 0; i < count; i++)
        {
            _starfish.Add(new Starfish
            {
                X = rng.NextDouble() * w,
                Y = beachY + rng.NextDouble() * 70,
                Size = 10 + rng.NextDouble() * 15,
                Rotation = rng.NextDouble() * Math.Tau,
                Clr = colors[rng.Next(colors.Length)]
            });
        }
    }

    public void Draw(DrawingContext dc)
    {
        foreach (var sf in _starfish)
        {
            dc.PushTransform(new RotateTransform(sf.Rotation * 180 / Math.PI, sf.X, sf.Y));
            
            var star = new StreamGeometry();
            using (var ctx = star.Open())
            {
                ctx.BeginFigure(new Point(sf.X, sf.Y - sf.Size), true, true);
                for (int i = 0; i < 5; i++)
                {
                    double outerAngle = (i * Math.Tau / 5) - Math.PI / 2;
                    double innerAngle = outerAngle + Math.Tau / 10;
                    
                    ctx.LineTo(new Point(
                        sf.X + Math.Cos(innerAngle) * sf.Size * 0.4,
                        sf.Y + Math.Sin(innerAngle) * sf.Size * 0.4), false, false);
                    ctx.LineTo(new Point(
                        sf.X + Math.Cos(outerAngle + Math.Tau / 5) * sf.Size,
                        sf.Y + Math.Sin(outerAngle + Math.Tau / 5) * sf.Size), false, false);
                }
            }
            star.Freeze();
            dc.DrawGeometry(new SolidColorBrush(Palettes.WithAlpha(sf.Clr, 180)), null, star);
            
            dc.Pop();
        }
    }
}

// ---------------------------------------------------------------------------
// Gentle waves — calmer, smaller waves than Coast scene
// ---------------------------------------------------------------------------

sealed class GentleWave
{
    readonly double _yBase, _speed, _amp, _freq, _phase;
    readonly SolidColorBrush _brush;
    double _t;

    public GentleWave(double yBase, Color color, byte alpha, double speed, double amp, double freq)
    {
        _yBase = yBase; _speed = speed; _amp = amp; _freq = freq;
        _phase = Util.Rand(0, Math.Tau);
        _brush = new SolidColorBrush(Palettes.WithAlpha(color, alpha));
        _brush.Freeze();
    }

    public void Update(double dt) => _t += dt * 0.5; // Gentler movement

    public double GetY(double x) =>
        _yBase
        + _amp * Math.Sin(_freq * x + _t * _speed + _phase)
        + _amp * 0.3 * Math.Sin(_freq * 1.8 * x - _t * _speed * 0.6 + _phase * 1.2);

    public void Draw(DrawingContext dc, double w, double h)
    {
        var geom = new StreamGeometry();
        using (var ctx = geom.Open())
        {
            ctx.BeginFigure(new Point(0, h), true, true);
            for (double x = 0; x <= w + 4; x += 4)
                ctx.LineTo(new Point(x, GetY(x)), false, false);
            ctx.LineTo(new Point(w, h), false, false);
        }
        geom.Freeze();
        dc.DrawGeometry(_brush, null, geom);
    }
}

// ---------------------------------------------------------------------------
// Surf foam — gentle white foam on beach
// ---------------------------------------------------------------------------

sealed class SurfFoam
{
    sealed class Bubble
    {
        public double X, Y, R, Phase, Speed;
    }

    readonly List<Bubble> _bubbles = [];
    readonly double _w, _beachY;
    readonly Color _color;
    double _t;

    public SurfFoam(double w, double beachY, int count, Color color, int seed)
    {
        _w = w; _beachY = beachY; _color = color;
        var rng = new Random(seed);

        for (int i = 0; i < count; i++)
        {
            _bubbles.Add(new Bubble
            {
                X = rng.NextDouble() * w,
                Y = beachY + rng.NextDouble() * 30,
                R = 2 + rng.NextDouble() * 6,
                Phase = rng.NextDouble() * Math.Tau,
                Speed = 0.5 + rng.NextDouble() * 1.5
            });
        }
    }

    public void Update(double dt)
    {
        _t += dt;
        foreach (var b in _bubbles)
        {
            b.X += b.Speed * dt * 3;
            b.Y += Math.Sin(_t * 0.5 + b.Phase) * dt * 2;
            if (b.X > _w) { b.X = 0; b.Y = _beachY + Util.Rand(0, 30); }
        }
    }

    public void Draw(DrawingContext dc)
    {
        foreach (var b in _bubbles)
        {
            double alpha = 80 + 60 * Math.Sin(_t * 0.8 + b.Phase);
            dc.DrawEllipse(
                new SolidColorBrush(Palettes.WithAlpha(_color, (byte)alpha)),
                null, new Point(b.X, b.Y), b.R, b.R);
        }
    }
}

// ---------------------------------------------------------------------------
// Beach umbrella — colorful shade on beach
// ---------------------------------------------------------------------------

sealed class BeachUmbrella
{
    readonly double _x, _y, _size;
    readonly Color _color;

    public BeachUmbrella(double x, double y, double size, Color color)
    {
        _x = x; _y = y; _size = size; _color = color;
    }

    public void Draw(DrawingContext dc)
    {
        // Pole
        dc.DrawLine(
            new Pen(new SolidColorBrush(Palettes.WithAlpha(Color.FromRgb(0x9c, 0xa0, 0xb0), 200)), 3),
            new Point(_x, _y),
            new Point(_x, _y - _size));

        // Canopy (semi-circle with segments)
        int segments = 8;
        for (int i = 0; i < segments; i++)
        {
            double angle1 = Math.PI + (i * Math.PI / segments);
            double angle2 = Math.PI + ((i + 1) * Math.PI / segments);
            
            var segment = new StreamGeometry();
            using (var ctx = segment.Open())
            {
                ctx.BeginFigure(new Point(_x, _y - _size), true, true);
                ctx.LineTo(new Point(_x + Math.Cos(angle1) * _size, _y - _size + Math.Sin(angle1) * _size * 0.4), false, false);
                ctx.ArcTo(
                    new Point(_x + Math.Cos(angle2) * _size, _y - _size + Math.Sin(angle2) * _size * 0.4),
                    new Size(_size, _size * 0.4), 0, false, System.Windows.Media.SweepDirection.Clockwise, true, false);
            }
            segment.Freeze();
            
            byte alpha = (byte)(i % 2 == 0 ? 200 : 180);
            dc.DrawGeometry(new SolidColorBrush(Palettes.WithAlpha(_color, alpha)), null, segment);
        }
    }
}

// ---------------------------------------------------------------------------
// GentleBeach scene orchestrator
// ---------------------------------------------------------------------------

public sealed class GentleBeachScene : IScene
{
    readonly CelestialBody    _celestial;
    readonly List<GentleWave> _waves = [];
    readonly PalmTree         _palmLeft, _palmRight;
    readonly ShellField       _shells;
    readonly StarfishField    _starfish;
    readonly SurfFoam         _foam;
    readonly BeachUmbrella    _umbrellaLeft, _umbrellaRight;
    readonly BirdFlock        _seagulls;
    readonly AppSettings      _s;
    readonly Palette          _p;
    readonly string           _tod;
    readonly double           _w, _h;
    readonly double           _horizonY, _beachY;

    public GentleBeachScene(AppSettings s, double w, double h)
    {
        _s = s; _w = w; _h = h;
        _p = Palettes.All.GetValueOrDefault(s.Flavor, Palettes.All["mocha"]);
        _tod = s.EffectiveTimeOfDay;
        _horizonY = h * 0.5;
        _beachY   = h * 0.7;

        _celestial = new CelestialBody(w * 0.7, h * 0.25, _p, _tod);

        // Gentle waves
        int waveCount = 4;
        for (int i = 0; i < waveCount; i++)
        {
            double y = _horizonY + (i * (_beachY - _horizonY) / waveCount);
            double depth = (double)i / waveCount;
            var waveColor = Palettes.Lerp(_p.Sapphire, _p.Teal, depth);
            byte alpha = (byte)(120 + depth * 100);
            _waves.Add(new GentleWave(y, waveColor, alpha, 0.3, 8 + i * 3, 0.01 - i * 0.002));
        }

        _palmLeft  = new PalmTree(w * 0.15, _beachY, h * 0.35, _p);
        _palmRight = new PalmTree(w * 0.85, _beachY, h * 0.38, _p);

        _shells   = new ShellField(w, _beachY, 25, _p, 2020);
        _starfish = new StarfishField(w, _beachY, 8, _p, 2121);
        _foam     = new SurfFoam(w, _beachY - 5, 60, _p.Text, 2222);

        _umbrellaLeft  = new BeachUmbrella(w * 0.35, _beachY, 60, _p.Red);
        _umbrellaRight = new BeachUmbrella(w * 0.65, _beachY, 55, _p.Blue);

        _seagulls = new BirdFlock(6, w, h * 0.6, _p.Overlay0, 2323);
    }

    public void Update(double dt)
    {
        _celestial.Update(dt);
        foreach (var wave in _waves) wave.Update(dt);
        _palmLeft.Update(dt);
        _palmRight.Update(dt);
        _shells.Update(dt);
        if (_s.ShowSurfFoam) _foam.Update(dt);
        if (_s.ShowBirds) _seagulls.Update(dt);
    }

    public void Draw(DrawingContext dc, double w, double h, double ppd)
    {
        // 1. Sky
        Background.DrawSky(dc, w, _horizonY, _p, _tod);

        // 2. Celestial
        _celestial.Draw(dc);

        // 3. Ocean horizon
        var oceanBrush = new LinearGradientBrush();
        oceanBrush.StartPoint = new Point(0, 0);
        oceanBrush.EndPoint = new Point(0, 1);
        oceanBrush.GradientStops.Add(new GradientStop(Palettes.Lerp(_p.Sky, _p.Sapphire, 0.4), 0));
        oceanBrush.GradientStops.Add(new GradientStop(_p.Teal, 1));
        dc.DrawRectangle(oceanBrush, null, new Rect(0, _horizonY, w, _beachY - _horizonY));

        // 4. Waves
        foreach (var wave in _waves) wave.Draw(dc, w, h);

        // 5. Beach (sand)
        var sandBrush = new LinearGradientBrush();
        sandBrush.StartPoint = new Point(0, 0);
        sandBrush.EndPoint = new Point(0, 1);
        sandBrush.GradientStops.Add(new GradientStop(Palettes.Lerp(_p.Yellow, _p.Peach, 0.3), 0));
        sandBrush.GradientStops.Add(new GradientStop(Palettes.Lerp(_p.Peach, _p.Maroon, 0.2), 1));
        dc.DrawRectangle(sandBrush, null, new Rect(0, _beachY, w, h - _beachY));

        // 6. Seagulls
        if (_s.ShowBirds) _seagulls.Draw(dc);

        // 7. Surf foam
        if (_s.ShowSurfFoam) _foam.Draw(dc);

        // 8. Shells
        if (_s.ShowShells) _shells.Draw(dc);

        // 9. Starfish
        if (_s.ShowStarfish) _starfish.Draw(dc);

        // 10. Beach umbrellas
        if (_s.ShowUmbrellas)
        {
            _umbrellaLeft.Draw(dc);
            _umbrellaRight.Draw(dc);
        }

        // 11. Palm trees (foreground)
        if (_s.ShowPalms)
        {
            _palmLeft.Draw(dc);
            _palmRight.Draw(dc);
        }

        // 12. Clock
        if (_s.ShowClock)
            ClockOverlay.Draw(dc, w, h, _p, ppd, _s.ClockPos, _s.ClockFormat);
    }
}
