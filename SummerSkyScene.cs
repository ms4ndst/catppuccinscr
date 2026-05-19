using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CatppuccinCoast;

// ---------------------------------------------------------------------------
// Sun rays — radial beams from celestial body
// ---------------------------------------------------------------------------

sealed class SunRays
{
    readonly double _cx, _cy;
    readonly Palette _p;
    double _t;

    public SunRays(double cx, double cy, Palette p)
    {
        _cx = cx; _cy = cy; _p = p;
    }

    public void Update(double dt) => _t += dt;

    public void Draw(DrawingContext dc, double w, double h)
    {
        int rayCount = 12;
        for (int i = 0; i < rayCount; i++)
        {
            double angle = (i * Math.Tau / rayCount) + _t * 0.1;
            double len = Math.Min(w, h) * 0.8;
            double x1 = _cx + Math.Cos(angle) * 80;
            double y1 = _cy + Math.Sin(angle) * 80;
            double x2 = _cx + Math.Cos(angle) * len;
            double y2 = _cy + Math.Sin(angle) * len;

            var brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(1, 1);
            brush.GradientStops.Add(new GradientStop(Palettes.WithAlpha(_p.Yellow, 40), 0));
            brush.GradientStops.Add(new GradientStop(Palettes.WithAlpha(_p.Yellow, 0), 1));
            
            var geom = new StreamGeometry();
            using (var ctx = geom.Open())
            {
                double perpAngle = angle + Math.PI / 2;
                double width = 15 + 10 * Math.Sin(_t * 0.5 + i);
                double dx = Math.Cos(perpAngle) * width;
                double dy = Math.Sin(perpAngle) * width;

                ctx.BeginFigure(new Point(x1 - dx, y1 - dy), true, true);
                ctx.LineTo(new Point(x1 + dx, y1 + dy), false, false);
                ctx.LineTo(new Point(x2, y2), false, false);
            }
            geom.Freeze();
            dc.DrawGeometry(brush, null, geom);
        }
    }
}

// ---------------------------------------------------------------------------
// Hot air balloon — floating decorative element
// ---------------------------------------------------------------------------

sealed class BalloonField
{
    sealed class Balloon
    {
        public double X, Y, Size, Speed, Phase, BobPhase;
        public Color Clr1, Clr2;
    }

    readonly List<Balloon> _balloons = [];
    readonly double _w, _h;
    double _t;

    public BalloonField(int count, double w, double h, Palette p, int seed)
    {
        _w = w; _h = h;
        var colors = new[] { p.Pink, p.Mauve, p.Red, p.Peach, p.Yellow, p.Blue, p.Sapphire };
        var rng = new Random(seed);
        
        for (int i = 0; i < count; i++)
        {
            var c1 = colors[rng.Next(colors.Length)];
            var c2 = colors[rng.Next(colors.Length)];
            _balloons.Add(new Balloon
            {
                X = rng.NextDouble() * w,
                Y = h * 0.2 + rng.NextDouble() * h * 0.3,
                Size = 40 + rng.NextDouble() * 60,
                Speed = 5 + rng.NextDouble() * 10,
                Phase = rng.NextDouble() * Math.Tau,
                BobPhase = rng.NextDouble() * Math.Tau,
                Clr1 = c1,
                Clr2 = c2
            });
        }
    }

    public void Update(double dt)
    {
        _t += dt;
        foreach (var b in _balloons)
        {
            b.X += b.Speed * dt;
            b.Y += 5 * Math.Sin(_t * 0.4 + b.BobPhase) * dt;
            if (b.X > _w + b.Size) { b.X = -b.Size; b.Y = _h * 0.2 + Util.Rand(0, _h * 0.3); }
        }
    }

    public void Draw(DrawingContext dc)
    {
        foreach (var b in _balloons)
        {
            // Balloon envelope (striped)
            var brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(0, 1);
            brush.GradientStops.Add(new GradientStop(Palettes.WithAlpha(b.Clr1, 200), 0));
            brush.GradientStops.Add(new GradientStop(Palettes.WithAlpha(b.Clr2, 200), 0.5));
            brush.GradientStops.Add(new GradientStop(Palettes.WithAlpha(b.Clr1, 200), 1));

            dc.DrawEllipse(brush, null, new Point(b.X, b.Y), b.Size * 0.5, b.Size * 0.65);

            // Basket
            var basketBrush = new SolidColorBrush(Palettes.WithAlpha(Color.FromRgb(0x40, 0xa0, 0x2b), 180));
            double basketY = b.Y + b.Size * 0.65 + 15;
            dc.DrawRectangle(basketBrush, null, new Rect(b.X - b.Size * 0.15, basketY, b.Size * 0.3, b.Size * 0.2));

            // Ropes
            var ropePen = new Pen(new SolidColorBrush(Palettes.WithAlpha(Color.FromRgb(0x58, 0x5b, 0x70), 150)), 1);
            dc.DrawLine(ropePen, new Point(b.X - b.Size * 0.3, b.Y + b.Size * 0.4), new Point(b.X - b.Size * 0.15, basketY));
            dc.DrawLine(ropePen, new Point(b.X + b.Size * 0.3, b.Y + b.Size * 0.4), new Point(b.X + b.Size * 0.15, basketY));
        }
    }
}

// ---------------------------------------------------------------------------
// Kites — flying in the breeze
// ---------------------------------------------------------------------------

sealed class KiteField
{
    sealed class Kite
    {
        public double X, Y, Size, Speed, SwayPhase, TailPhase;
        public Color Clr;
    }

    readonly List<Kite> _kites = [];
    readonly double _w, _h;
    double _t;

    public KiteField(int count, double w, double h, Palette p, int seed)
    {
        _w = w; _h = h;
        var colors = new[] { p.Red, p.Pink, p.Mauve, p.Blue, p.Teal, p.Green, p.Yellow };
        var rng = new Random(seed);

        for (int i = 0; i < count; i++)
        {
            _kites.Add(new Kite
            {
                X = rng.NextDouble() * w,
                Y = h * 0.15 + rng.NextDouble() * h * 0.25,
                Size = 20 + rng.NextDouble() * 30,
                Speed = 15 + rng.NextDouble() * 20,
                SwayPhase = rng.NextDouble() * Math.Tau,
                TailPhase = rng.NextDouble() * Math.Tau,
                Clr = colors[rng.Next(colors.Length)]
            });
        }
    }

    public void Update(double dt)
    {
        _t += dt;
        foreach (var k in _kites)
        {
            k.X += k.Speed * dt;
            k.Y += 8 * Math.Sin(_t * 0.6 + k.SwayPhase) * dt;
            if (k.X > _w + 100) { k.X = -100; k.Y = _h * 0.15 + Util.Rand(0, _h * 0.25); }
        }
    }

    public void Draw(DrawingContext dc)
    {
        foreach (var k in _kites)
        {
            double sway = 15 * Math.Sin(_t * 1.2 + k.SwayPhase);
            
            // Kite diamond shape
            var geom = new StreamGeometry();
            using (var ctx = geom.Open())
            {
                ctx.BeginFigure(new Point(k.X, k.Y - k.Size * 0.5), true, true);
                ctx.LineTo(new Point(k.X + k.Size * 0.5, k.Y), false, false);
                ctx.LineTo(new Point(k.X, k.Y + k.Size * 0.5), false, false);
                ctx.LineTo(new Point(k.X - k.Size * 0.5, k.Y), false, false);
            }
            geom.Freeze();
            dc.DrawGeometry(new SolidColorBrush(Palettes.WithAlpha(k.Clr, 220)), 
                new Pen(new SolidColorBrush(Palettes.WithAlpha(k.Clr, 255)), 2), geom);

            // Tail (wavy ribbon)
            var tailPen = new Pen(new SolidColorBrush(Palettes.WithAlpha(k.Clr, 180)), 3);
            int segments = 8;
            Point prev = new Point(k.X, k.Y + k.Size * 0.5);
            for (int i = 1; i <= segments; i++)
            {
                double ty = k.Y + k.Size * 0.5 + i * 12;
                double tx = k.X + sway + 10 * Math.Sin(_t * 2 + k.TailPhase + i * 0.5);
                Point cur = new Point(tx, ty);
                dc.DrawLine(tailPen, prev, cur);
                prev = cur;
            }
        }
    }
}

// ---------------------------------------------------------------------------
// Fluffy cumulus clouds — larger, more defined than generic clouds
// ---------------------------------------------------------------------------

sealed class CumulusField
{
    sealed class Cloud
    {
        public double X, Y, W, H, Speed;
        public List<(double dx, double dy, double rw, double rh)> Puffs = [];
    }

    readonly List<Cloud> _clouds = [];
    readonly double _w;
    readonly Color _color;

    public CumulusField(double w, double h, int count, Color color, byte alpha, int seed)
    {
        _w = w; _color = Palettes.WithAlpha(color, alpha);
        var rng = new Random(seed);

        for (int i = 0; i < count; i++)
        {
            var cloud = new Cloud
            {
                X = rng.NextDouble() * w,
                Y = h * 0.1 + rng.NextDouble() * h * 0.15,
                W = 100 + rng.NextDouble() * 150,
                H = 40 + rng.NextDouble() * 60,
                Speed = 2 + rng.NextDouble() * 5
            };

            // Generate multiple puffs per cloud
            int puffCount = 5 + rng.Next(4);
            for (int j = 0; j < puffCount; j++)
            {
                cloud.Puffs.Add((
                    rng.NextDouble() * cloud.W - cloud.W * 0.5,
                    rng.NextDouble() * cloud.H * 0.5 - cloud.H * 0.25,
                    20 + rng.NextDouble() * 40,
                    20 + rng.NextDouble() * 35
                ));
            }
            _clouds.Add(cloud);
        }
    }

    public void Update(double dt)
    {
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
            var brush = new SolidColorBrush(_color);
            foreach (var (dx, dy, rw, rh) in c.Puffs)
            {
                dc.DrawEllipse(brush, null, new Point(c.X + dx, c.Y + dy), rw, rh);
            }
        }
    }
}

// ---------------------------------------------------------------------------
// SummerSky scene orchestrator
// ---------------------------------------------------------------------------

public sealed class SummerSkyScene : IScene
{
    readonly CelestialBody    _sun;
    readonly SunRays          _sunRays;
    readonly CumulusField     _clouds;
    readonly BalloonField     _balloons;
    readonly KiteField        _kites;
    readonly BirdFlock        _birds;
    readonly AppSettings      _s;
    readonly Palette          _p;
    readonly double           _w, _h;

    public SummerSkyScene(AppSettings s, double w, double h)
    {
        _s = s; _w = w; _h = h;
        _p = Palettes.All.GetValueOrDefault(s.Flavor, Palettes.All["mocha"]);

        double sunX = w * 0.75;
        double sunY = h * 0.2;

        _sun     = new CelestialBody(sunX, sunY, _p, "day");
        _sunRays = new SunRays(sunX, sunY, _p);
        _clouds  = new CumulusField(w, h, 6, _p.Text, 140, 888);
        _balloons = new BalloonField(3, w, h, _p, 999);
        _kites   = new KiteField(4, w, h, _p, 1010);
        _birds   = new BirdFlock(8, w, h, _p.Crust, 1111);
    }

    public void Update(double dt)
    {
        _sun.Update(dt);
        _sunRays.Update(dt);
        _clouds.Update(dt);
        if (_s.ShowBalloons) _balloons.Update(dt);
        if (_s.ShowKites) _kites.Update(dt);
        if (_s.ShowBirds) _birds.Update(dt);
    }

    public void Draw(DrawingContext dc, double w, double h, double ppd)
    {
        // 1. Bright summer sky
        var skyBrush = new LinearGradientBrush();
        skyBrush.StartPoint = new Point(0, 0);
        skyBrush.EndPoint = new Point(0, 1);
        skyBrush.GradientStops.Add(new GradientStop(Palettes.Lerp(_p.Sky, _p.Sapphire, 0.3), 0));
        skyBrush.GradientStops.Add(new GradientStop(Palettes.Lerp(_p.Sapphire, _p.Teal, 0.2), 0.5));
        skyBrush.GradientStops.Add(new GradientStop(Palettes.Lerp(_p.Sky, _p.Blue, 0.4), 1));
        dc.DrawRectangle(skyBrush, null, new Rect(0, 0, w, h));

        // 2. Sun rays
        if (_s.ShowSunRays) _sunRays.Draw(dc, w, h);

        // 3. Sun
        _sun.Draw(dc);

        // 4. Clouds
        if (_s.ShowClouds) _clouds.Draw(dc);

        // 5. Hot air balloons
        if (_s.ShowBalloons) _balloons.Draw(dc);

        // 6. Birds
        if (_s.ShowBirds) _birds.Draw(dc);

        // 7. Kites
        if (_s.ShowKites) _kites.Draw(dc);

        // 8. Clock
        if (_s.ShowClock)
            ClockOverlay.Draw(dc, w, h, _p, ppd, _s.ClockPos, _s.ClockFormat);
    }
}
