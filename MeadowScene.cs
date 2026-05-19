using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CatppuccinCoast;

// ---------------------------------------------------------------------------
// Grass blade — individual swaying grass stems
// ---------------------------------------------------------------------------

sealed class GrassLayer
{
    sealed class Blade
    {
        public double X, BaseY, H, W, Phase, Speed, Bend;
        public Color Clr;
    }

    readonly List<Blade> _blades = [];
    readonly double _w, _h;
    double _t;

    public GrassLayer(double w, double h, double minY, int count, Color[] colors, double minH, double maxH, int seed)
    {
        _w = w; _h = h;
        var rng = new Random(seed);
        for (int i = 0; i < count; i++)
        {
            _blades.Add(new Blade
            {
                X = rng.NextDouble() * w,
                BaseY = minY,
                H = minH + rng.NextDouble() * (maxH - minH),
                W = 2 + rng.NextDouble() * 2,
                Phase = rng.NextDouble() * Math.Tau,
                Speed = 0.4 + rng.NextDouble() * 0.8,
                Bend = 0.15 + rng.NextDouble() * 0.25,
                Clr = colors[rng.Next(colors.Length)]
            });
        }
    }

    public void Update(double dt) => _t += dt;

    public void Draw(DrawingContext dc)
    {
        foreach (var b in _blades)
        {
            double sway = b.Bend * b.H * Math.Sin(_t * b.Speed + b.Phase);
            double topX = b.X + sway;
            double topY = b.BaseY - b.H;
            
            // Draw grass blade as thin bezier curve
            var geom = new StreamGeometry();
            using (var ctx = geom.Open())
            {
                ctx.BeginFigure(new Point(b.X, b.BaseY), false, false);
                double ctrlY = b.BaseY - b.H * 0.6;
                double ctrlX = b.X + sway * 0.5;
                ctx.BezierTo(
                    new Point(ctrlX, ctrlY),
                    new Point(topX - sway * 0.2, topY + b.H * 0.3),
                    new Point(topX, topY), true, false);
            }
            geom.Freeze();
            
            byte alpha = (byte)(180 + 40 * Math.Sin(_t * b.Speed * 0.5 + b.Phase));
            dc.DrawGeometry(null, new Pen(new SolidColorBrush(Palettes.WithAlpha(b.Clr, alpha)), b.W), geom);
        }
    }
}

// ---------------------------------------------------------------------------
// Butterfly — fluttering insects following erratic paths
// ---------------------------------------------------------------------------

sealed class ButterflyField
{
    sealed class Butterfly
    {
        public double X, Y, Vx, Vy, Phase, WingPhase, Size;
        public Color Clr;
    }

    readonly List<Butterfly> _butterflies = [];
    readonly double _w, _h, _minY, _maxY;
    readonly Color[] _colors;
    double _t;

    public ButterflyField(int count, double w, double h, double minY, double maxY, Palette p)
    {
        _w = w; _h = h; _minY = minY; _maxY = maxY;
        _colors = [p.Pink, p.Mauve, p.Peach, p.Yellow, p.Rosewater, p.Flamingo];
        for (int i = 0; i < count; i++) _butterflies.Add(Spawn());
    }

    Butterfly Spawn() => new()
    {
        X = Util.Rand(0, _w), Y = Util.Rand(_minY, _maxY),
        Vx = Util.Rand(-15, 15), Vy = Util.Rand(-10, 10),
        Phase = Util.Rand(0, Math.Tau), WingPhase = 0,
        Size = Util.Rand(8, 16), Clr = Util.Pick(_colors)
    };

    public void Update(double dt)
    {
        _t += dt;
        foreach (var b in _butterflies)
        {
            // Erratic flight pattern
            b.X += b.Vx * dt + 8 * Math.Sin(_t * 1.2 + b.Phase) * dt;
            b.Y += b.Vy * dt + 12 * Math.Sin(_t * 0.8 + b.Phase * 1.3) * dt;
            b.WingPhase = _t * 8 + b.Phase;
            
            // Wrap around
            if (b.X < -20) b.X = _w + 10;
            if (b.X > _w + 20) b.X = -10;
            if (b.Y < _minY - 20) b.Y = _maxY;
            if (b.Y > _maxY + 20) b.Y = _minY;
        }
    }

    public void Draw(DrawingContext dc)
    {
        foreach (var b in _butterflies)
        {
            double wingAngle = 0.4 + 0.6 * Math.Abs(Math.Sin(b.WingPhase));
            byte alpha = 200;
            
            // Body
            dc.DrawEllipse(
                new SolidColorBrush(Palettes.WithAlpha(b.Clr, alpha)),
                null, new Point(b.X, b.Y), b.Size * 0.15, b.Size * 0.4);
            
            // Wings (simplified as ellipses)
            var wingBrush = new SolidColorBrush(Palettes.WithAlpha(b.Clr, (byte)(alpha * 0.8)));
            
            // Left wing
            double lx = b.X - b.Size * 0.5 * wingAngle;
            dc.DrawEllipse(wingBrush, null, new Point(lx, b.Y - b.Size * 0.2), b.Size * 0.4, b.Size * 0.5);
            
            // Right wing
            double rx = b.X + b.Size * 0.5 * wingAngle;
            dc.DrawEllipse(wingBrush, null, new Point(rx, b.Y - b.Size * 0.2), b.Size * 0.4, b.Size * 0.5);
        }
    }
}

// ---------------------------------------------------------------------------
// Flowers — scattered wildflowers
// ---------------------------------------------------------------------------

sealed class FlowerField
{
    sealed class Flower
    {
        public double X, Y, Size, Phase, StemH;
        public Color Clr;
    }

    readonly List<Flower> _flowers = [];
    double _t;

    public FlowerField(double w, double groundY, int count, Color[] colors, int seed)
    {
        var rng = new Random(seed);
        for (int i = 0; i < count; i++)
        {
            _flowers.Add(new Flower
            {
                X = rng.NextDouble() * w,
                Y = groundY - rng.NextDouble() * 15,
                Size = 4 + rng.NextDouble() * 8,
                Phase = rng.NextDouble() * Math.Tau,
                StemH = 10 + rng.NextDouble() * 25,
                Clr = colors[rng.Next(colors.Length)]
            });
        }
    }

    public void Update(double dt) => _t += dt;

    public void Draw(DrawingContext dc)
    {
        foreach (var f in _flowers)
        {
            double sway = 4 * Math.Sin(_t * 0.6 + f.Phase);
            
            // Stem
            dc.DrawLine(
                new Pen(new SolidColorBrush(Palettes.WithAlpha(Color.FromRgb(0x40, 0xa0, 0x2b), 180)), 1.5),
                new Point(f.X, f.Y),
                new Point(f.X + sway, f.Y - f.StemH));
            
            // Flower head (simple circle with petals)
            double headX = f.X + sway;
            double headY = f.Y - f.StemH;
            
            // Petals (5 circles around center)
            byte petalAlpha = 200;
            for (int i = 0; i < 5; i++)
            {
                double angle = i * Math.Tau / 5 + _t * 0.3 + f.Phase;
                double px = headX + Math.Cos(angle) * f.Size * 0.5;
                double py = headY + Math.Sin(angle) * f.Size * 0.5;
                dc.DrawEllipse(
                    new SolidColorBrush(Palettes.WithAlpha(f.Clr, petalAlpha)),
                    null, new Point(px, py), f.Size * 0.35, f.Size * 0.35);
            }
            
            // Center
            dc.DrawEllipse(
                new SolidColorBrush(Palettes.WithAlpha(Color.FromRgb(0xf9, 0xe2, 0xaf), 255)),
                null, new Point(headX, headY), f.Size * 0.25, f.Size * 0.25);
        }
    }
}

// ---------------------------------------------------------------------------
// Clouds — drifting fluffy shapes
// ---------------------------------------------------------------------------

sealed class CloudLayer
{
    sealed class Cloud
    {
        public double X, Y, W, H, Speed, Phase;
        public Color Clr;
    }

    readonly List<Cloud> _clouds = [];
    readonly double _w;
    double _t;

    public CloudLayer(double w, double h, int count, Color color, byte alpha, int seed)
    {
        _w = w;
        var rng = new Random(seed);
        for (int i = 0; i < count; i++)
        {
            _clouds.Add(new Cloud
            {
                X = rng.NextDouble() * w,
                Y = h * 0.1 + rng.NextDouble() * h * 0.25,
                W = 80 + rng.NextDouble() * 120,
                H = 30 + rng.NextDouble() * 50,
                Speed = 3 + rng.NextDouble() * 7,
                Phase = rng.NextDouble() * Math.Tau,
                Clr = Palettes.WithAlpha(color, alpha)
            });
        }
    }

    public void Update(double dt)
    {
        _t += dt;
        foreach (var c in _clouds)
        {
            c.X += c.Speed * dt;
            if (c.X > _w + c.W) c.X = -c.W;
        }
    }

    public void Draw(DrawingContext dc)
    {
        foreach (var c in _clouds)
        {
            var brush = new RadialGradientBrush(c.Clr, Palettes.WithAlpha(c.Clr, 0));
            brush.GradientOrigin = new Point(0.5, 0.5);
            brush.Center = new Point(0.5, 0.5);
            
            // Draw multiple overlapping ellipses for cloud effect
            dc.DrawEllipse(brush, null, new Point(c.X, c.Y), c.W * 0.4, c.H * 0.6);
            dc.DrawEllipse(brush, null, new Point(c.X + c.W * 0.3, c.Y), c.W * 0.35, c.H * 0.5);
            dc.DrawEllipse(brush, null, new Point(c.X - c.W * 0.25, c.Y + c.H * 0.2), c.W * 0.3, c.H * 0.4);
            dc.DrawEllipse(brush, null, new Point(c.X + c.W * 0.15, c.Y - c.H * 0.15), c.W * 0.25, c.H * 0.45);
        }
    }
}

// ---------------------------------------------------------------------------
// Birds — simple silhouettes flying across the sky
// ---------------------------------------------------------------------------

sealed class BirdFlock
{
    sealed class Bird
    {
        public double X, Y, Speed, WingPhase, Size;
        public Color Clr;
    }

    readonly List<Bird> _birds = [];
    readonly double _w, _h;
    double _t;

    public BirdFlock(int count, double w, double h, Color color, int seed)
    {
        _w = w; _h = h;
        var rng = new Random(seed);
        for (int i = 0; i < count; i++)
        {
            _birds.Add(new Bird
            {
                X = rng.NextDouble() * w,
                Y = h * 0.15 + rng.NextDouble() * h * 0.25,
                Speed = 20 + rng.NextDouble() * 30,
                WingPhase = rng.NextDouble() * Math.Tau,
                Size = 8 + rng.NextDouble() * 12,
                Clr = color
            });
        }
    }

    public void Update(double dt)
    {
        _t += dt;
        foreach (var b in _birds)
        {
            b.X += b.Speed * dt;
            b.Y += 3 * Math.Sin(_t * 0.5 + b.WingPhase) * dt;
            if (b.X > _w + 50) { b.X = -50; b.Y = _h * 0.15 + Util.Rand(0, _h * 0.25); }
        }
    }

    public void Draw(DrawingContext dc)
    {
        foreach (var b in _birds)
        {
            double wingAngle = Math.Sin(_t * 6 + b.WingPhase) * 0.5;
            var pen = new Pen(new SolidColorBrush(Palettes.WithAlpha(b.Clr, 180)), 2);
            
            // Simple V-shape for bird
            dc.DrawLine(pen, new Point(b.X - b.Size, b.Y + b.Size * wingAngle), new Point(b.X, b.Y));
            dc.DrawLine(pen, new Point(b.X, b.Y), new Point(b.X + b.Size, b.Y + b.Size * wingAngle));
        }
    }
}

// ---------------------------------------------------------------------------
// Meadow scene orchestrator
// ---------------------------------------------------------------------------

public sealed class MeadowScene : IScene
{
    readonly StarField        _stars;
    readonly ShootingStarField _shooting;
    readonly CelestialBody    _celestial;
    readonly Aurora           _aurora;
    readonly CloudLayer       _cloudsBack, _cloudsFront;
    readonly BirdFlock        _birds;
    readonly GrassLayer       _grassBack, _grassMid, _grassFront;
    readonly FlowerField      _flowers;
    readonly ButterflyField   _butterflies;
    readonly AppSettings      _s;
    readonly Palette          _p;
    readonly string           _tod;
    readonly double           _w, _h;
    readonly double           _horizonY;
    readonly double           _groundY;

    public MeadowScene(AppSettings s, double w, double h)
    {
        _s = s; _w = w; _h = h;
        _p = Palettes.All.GetValueOrDefault(s.Flavor, Palettes.All["mocha"]);
        _tod = s.EffectiveTimeOfDay;
        _horizonY = h * 0.45;
        _groundY  = h * 0.75;

        int starCount = s.StarDensity switch { "sparse" => 50, "dense" => 300, _ => 160 };
        _stars    = new StarField(starCount, w, h, _p);
        _shooting = new ShootingStarField(2, w, h, _p);
        _celestial = new CelestialBody(w * 0.75, h * 0.18, _p, _tod);
        _aurora   = new Aurora(w, h, _p);

        _cloudsBack  = new CloudLayer(w, h, 4, _p.Overlay0, 60, 111);
        _cloudsFront = new CloudLayer(w, h, 3, _p.Surface2, 90, 222);
        _birds       = new BirdFlock(5, w, h, _p.Crust, 333);

        var grassColors = new[] { _p.Green, _p.Teal, Palettes.Lerp(_p.Green, _p.Yellow, 0.3) };
        _grassBack  = new GrassLayer(w, h, _horizonY + 10, 80, grassColors, h * 0.08, h * 0.15, 444);
        _grassMid   = new GrassLayer(w, h, _groundY - 20, 100, grassColors, h * 0.12, h * 0.22, 555);
        _grassFront = new GrassLayer(w, h, _groundY + 30, 120, grassColors, h * 0.15, h * 0.28, 666);

        var flowerColors = new[] { _p.Pink, _p.Mauve, _p.Peach, _p.Rosewater, _p.Yellow };
        _flowers = new FlowerField(w, _groundY, 40, flowerColors, 777);

        _butterflies = new ButterflyField(12, w, h, _horizonY, _groundY + 50, _p);
    }

    public void Update(double dt)
    {
        _stars.Update(dt);
        if (_s.ShowShooting) _shooting.Update(dt);
        _celestial.Update(dt);
        if (_s.ShowAurora) _aurora.Update(dt);
        _cloudsBack.Update(dt);
        _cloudsFront.Update(dt);
        _birds.Update(dt);
        _grassBack.Update(dt);
        _grassMid.Update(dt);
        _grassFront.Update(dt);
        _flowers.Update(dt);
        _butterflies.Update(dt);
    }

    public void Draw(DrawingContext dc, double w, double h, double ppd)
    {
        // 1. Sky
        Background.DrawSky(dc, w, h, _p, _tod);

        // 2. Aurora (night/dusk only)
        if (_s.ShowAurora && _tod is "night" or "dusk") _aurora.Draw(dc);

        // 3. Stars
        double starOp = _tod switch { "day" or "morning" => 0.0, "dusk" => 0.2, _ => 1.0 };
        _stars.Draw(dc, starOp);
        if (_s.ShowShooting && _tod == "night") _shooting.Draw(dc);

        // 4. Celestial body
        _celestial.Draw(dc);

        // 5. Background clouds
        if (_s.ShowClouds && _tod is "day" or "morning" or "dusk") _cloudsBack.Draw(dc);

        // 6. Birds
        if (_s.ShowBirds && _tod is "day" or "morning") _birds.Draw(dc);

        // 7. Distant grass
        _grassBack.Draw(dc);

        // 8. Mid-ground grass
        _grassMid.Draw(dc);

        // 9. Flowers
        if (_s.ShowFlowers) _flowers.Draw(dc);

        // 10. Butterflies
        if (_s.ShowButterflies && _tod is "day" or "morning" or "dusk") _butterflies.Draw(dc);

        // 11. Foreground grass
        _grassFront.Draw(dc);

        // 12. Foreground clouds
        if (_s.ShowClouds && _tod is "day" or "morning" or "dusk") _cloudsFront.Draw(dc);

        // 13. Clock
        if (_s.ShowClock)
            ClockOverlay.Draw(dc, w, h, _p, ppd, _s.ClockPos, _s.ClockFormat);
    }
}
