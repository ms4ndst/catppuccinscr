using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CatppuccinCoast;

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

static class Util
{
    public static double Clamp(double v, double lo, double hi) => Math.Max(lo, Math.Min(hi, v));
    static readonly Random Rng = new();
    public static double Rand(double lo, double hi) => lo + Rng.NextDouble() * (hi - lo);
    public static T Pick<T>(T[] arr) => arr[Rng.Next(arr.Length)];
    public static int RandInt(int lo, int hi) => Rng.Next(lo, hi);
}

// ---------------------------------------------------------------------------
// Wave
// ---------------------------------------------------------------------------

sealed class Wave
{
    readonly double _yBase, _speed, _amp, _freq, _phase;
    readonly SolidColorBrush _brush;
    double _t;

    public Wave(double yBase, Color color, byte alpha, double speed, double amp, double freq)
    {
        _yBase = yBase; _speed = speed; _amp = amp; _freq = freq;
        _phase = Util.Rand(0, Math.Tau);
        _brush = new SolidColorBrush(Palettes.WithAlpha(color, alpha));
        _brush.Freeze();
    }

    public void Update(double dt, double speedMult) => _t += dt * speedMult;

    public double GetY(double x) =>
        _yBase
        + _amp * Math.Sin(_freq * x + _t * _speed + _phase)
        + _amp * 0.38 * Math.Sin(_freq * 2.1 * x - _t * _speed * 0.72 + _phase * 1.4);

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
// StarField
// ---------------------------------------------------------------------------

sealed class StarField
{
    record struct Star(double X, double Y, double R, Color Base, double Phase, double Speed);
    readonly Star[] _stars;
    double _t;

    public StarField(int count, double w, double h, Palette p)
    {
        var colors = new[] { p.Lavender, p.Blue, p.Sky, p.Text, p.Subtext0 };
        _stars = new Star[count];
        for (int i = 0; i < count; i++)
            _stars[i] = new(Util.Rand(0, w), Util.Rand(0, h * 0.56),
                Util.Pick(new[] { 1.0, 1.0, 1.0, 2.0 }),
                Util.Pick(colors), Util.Rand(0, Math.Tau), Util.Rand(0.4, 1.3));
    }

    public void Update(double dt) => _t += dt;

    public void Draw(DrawingContext dc, double opacity = 1.0)
    {
        if (opacity <= 0.02) return;
        foreach (var s in _stars)
        {
            double b = 0.5 + 0.5 * Math.Sin(_t * s.Speed + s.Phase);
            byte a = (byte)Util.Clamp(b * 255 * opacity, 0, 255);
            dc.DrawEllipse(new SolidColorBrush(Palettes.WithAlpha(s.Base, a)),
                null, new Point(s.X, s.Y), s.R, s.R);
        }
    }
}

// ---------------------------------------------------------------------------
// Shooting stars
// ---------------------------------------------------------------------------

sealed class ShootingStarField
{
    sealed class Shooter
    {
        double _x, _y, _vx, _vy, _length, _life;
        bool _active;
        double _timer;
        readonly double _w, _h;
        readonly Palette _p;

        public Shooter(double w, double h, Palette p)
        { _w = w; _h = h; _p = p; _timer = Util.Rand(3, 10); }

        public void Update(double dt)
        {
            if (!_active) { _timer -= dt; if (_timer <= 0) Activate(); return; }
            _x += _vx * dt; _y += _vy * dt; _life -= dt * 1.9;
            if (_life <= 0 || _x < -200 || _y > _h * 0.55) { _active = false; _timer = Util.Rand(3, 10); }
        }

        void Activate()
        {
            double angle = Util.Rand(Math.PI * 205 / 180, Math.PI * 335 / 180);
            double speed = Util.Rand(350, 620);
            _x = Util.Rand(_w * 0.1, _w * 0.9); _y = Util.Rand(5, _h * 0.28);
            _vx = Math.Cos(angle) * speed; _vy = Math.Sin(angle) * speed;
            _length = Util.Rand(70, 150); _life = 1.0; _active = true;
        }

        public void Draw(DrawingContext dc)
        {
            if (!_active) return;
            double spd = Math.Sqrt(_vx * _vx + _vy * _vy);
            double tx = _x - _vx / spd * _length, ty = _y - _vy / spd * _length;
            byte a = (byte)Util.Clamp(_life * 210, 0, 210);
            dc.DrawLine(new Pen(new SolidColorBrush(Palettes.WithAlpha(_p.Text, a)), 1),
                new Point(tx, ty), new Point(_x, _y));
            dc.DrawEllipse(new SolidColorBrush(Palettes.WithAlpha(_p.Text, a)),
                null, new Point(_x, _y), 1.5, 1.5);
        }
    }

    readonly Shooter[] _shooters;
    public ShootingStarField(int count, double w, double h, Palette p) =>
        _shooters = Enumerable.Range(0, count).Select(_ => new Shooter(w, h, p)).ToArray();

    public void Update(double dt) { foreach (var s in _shooters) s.Update(dt); }
    public void Draw(DrawingContext dc) { foreach (var s in _shooters) s.Draw(dc); }
}

// ---------------------------------------------------------------------------
// Moon / Sun
// ---------------------------------------------------------------------------

sealed class CelestialBody
{
    readonly double _x, _y;
    readonly Palette _p;
    readonly string _timeOfDay;
    double _t;

    public CelestialBody(double x, double y, Palette p, string timeOfDay)
    { _x = x; _y = y; _p = p; _timeOfDay = timeOfDay; }

    public void Update(double dt) => _t += dt * 0.14;

    public void Draw(DrawingContext dc)
    {
        if (_timeOfDay == "day") DrawSun(dc);
        else DrawMoon(dc);
    }

    void DrawMoon(DrawingContext dc)
    {
        double pulse = 0.85 + 0.15 * Math.Sin(_t);
        byte baseA = _timeOfDay == "dusk" ? (byte)100 : (byte)255;
        foreach (var (r, a) in new[] { (72, 12), (58, 20), (48, 33), (40, 52) })
        {
            byte alpha = (byte)Util.Clamp(a * pulse * baseA / 255.0, 0, 255);
            dc.DrawEllipse(new RadialGradientBrush(
                Palettes.WithAlpha(_p.Lavender, alpha), Palettes.WithAlpha(_p.Lavender, 0)),
                null, new Point(_x, _y), r, r);
        }
        byte bodyA = _timeOfDay == "dusk" ? (byte)140 : (byte)255;
        dc.DrawEllipse(new SolidColorBrush(Palettes.WithAlpha(_p.Yellow, bodyA)), null, new Point(_x, _y), 28, 28);
        dc.DrawEllipse(new SolidColorBrush(Palettes.WithAlpha(_p.Mantle, (byte)(195 * baseA / 255))),
            null, new Point(_x + 6, _y), 26, 26);
        foreach (var (cx, cy, cr) in new[] { (_x - 8, _y + 5, 4.0), (_x + 6, _y - 8, 3.0) })
            dc.DrawEllipse(new SolidColorBrush(Palettes.WithAlpha(
                Palettes.Lerp(_p.Yellow, _p.Mantle, 0.38), bodyA)),
                null, new Point(cx, cy), cr, cr);
    }

    void DrawSun(DrawingContext dc)
    {
        double pulse = 0.88 + 0.12 * Math.Sin(_t * 0.4);
        // Rays
        for (int i = 0; i < 8; i++)
        {
            double angle = i * Math.PI / 4 + _t * 0.05;
            double len = 50 + 10 * Math.Sin(_t * 0.3 + i);
            double rx = _x + Math.Cos(angle) * (34 + len);
            double ry = _y + Math.Sin(angle) * (34 + len);
            byte ra = (byte)(40 + 20 * pulse);
            dc.DrawLine(new Pen(new SolidColorBrush(Palettes.WithAlpha(_p.Yellow, ra)), 2),
                new Point(_x + Math.Cos(angle) * 32, _y + Math.Sin(angle) * 32),
                new Point(rx, ry));
        }
        // Outer glow
        foreach (var (r, a) in new[] { (60, 20), (48, 35), (40, 60) })
            dc.DrawEllipse(new RadialGradientBrush(
                Palettes.WithAlpha(_p.Yellow, (byte)(a * pulse)), Palettes.WithAlpha(_p.Yellow, 0)),
                null, new Point(_x, _y), r, r);
        // Body
        dc.DrawEllipse(new SolidColorBrush(_p.Yellow), null, new Point(_x, _y), 28, 28);
        dc.DrawEllipse(new SolidColorBrush(Palettes.WithAlpha(_p.Peach, 80)), null, new Point(_x, _y), 28, 28);
    }
}

// ---------------------------------------------------------------------------
// Aurora
// ---------------------------------------------------------------------------

sealed class Aurora
{
    record struct Band(double Y, Color Clr, double Phase, double Speed, double Amp, double Freq, byte Alpha);
    readonly Band[] _bands;
    readonly double _w, _h;
    double _t;

    public Aurora(double w, double h, Palette p)
    {
        _w = w; _h = h;
        var colors = new[] { p.Mauve, p.Teal, p.Blue, p.Green, p.Lavender };
        _bands = new Band[4];
        for (int i = 0; i < 4; i++)
            _bands[i] = new(h * Util.Rand(0.07, 0.37), Util.Pick(colors),
                Util.Rand(0, Math.Tau), Util.Rand(0.17, 0.40),
                Util.Rand(16, 38), Util.Rand(0.003, 0.007), (byte)Util.RandInt(16, 44));
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
                    top.Add(new(x, b.Y + off - 18)); bot.Add(new(x, b.Y + off + 18));
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
// Foam particles
// ---------------------------------------------------------------------------

sealed class FoamField
{
    sealed class Particle { public double X, Y, Vx, Vy, Size, Life, MaxLife; public Color Clr; }
    readonly List<Particle> _particles = [];
    readonly Wave _front;
    readonly double _w;
    readonly Color[] _colors;

    public FoamField(double w, double h, Wave front, Palette p)
    {
        _w = w; _front = front;
        _colors = [p.Sky, p.Teal, p.Text, p.Lavender, p.Sapphire];
        for (int i = 0; i < 90; i++) _particles.Add(Spawn(true));
    }

    Particle Spawn(bool init = false)
    {
        double x = Util.Rand(0, _w);
        double ml = Util.Rand(0.4, 2.0);
        return new() { X = x, Y = _front.GetY(x), Vx = Util.Rand(-10, 10),
            Vy = -Util.Rand(8, 28), Size = Util.Rand(1.2, 3.0),
            Life = init ? Util.Rand(0, ml) : ml, MaxLife = ml, Clr = Util.Pick(_colors) };
    }

    public void Update(double dt)
    {
        for (int i = 0; i < _particles.Count; i++)
        {
            var p = _particles[i]; p.Life -= dt;
            if (p.Life <= 0) { _particles[i] = Spawn(); continue; }
            p.X += p.Vx * dt; p.Y += p.Vy * dt;
        }
    }

    public void Draw(DrawingContext dc)
    {
        foreach (var p in _particles)
        {
            byte a = (byte)Util.Clamp(p.Life / p.MaxLife * 200, 0, 200);
            dc.DrawEllipse(new SolidColorBrush(Palettes.WithAlpha(p.Clr, a)),
                null, new Point(p.X, p.Y), p.Size, p.Size);
        }
    }
}

// ---------------------------------------------------------------------------
// Bioluminescence — glowing particles inside the ocean
// ---------------------------------------------------------------------------

sealed class BioGlow
{
    sealed class Particle { public double X, Y, Vx, Vy, Life, MaxLife, GlowR; public Color Clr; }
    readonly List<Particle> _particles = [];
    readonly Wave[] _waves;
    readonly double _w, _h;
    readonly Color[] _colors;

    public BioGlow(double w, double h, Wave[] waves, Palette p)
    {
        _w = w; _h = h; _waves = waves;
        _colors = [p.Teal, p.Sky, p.Green, p.Sapphire];
        for (int i = 0; i < 70; i++) _particles.Add(Spawn(true));
    }

    Particle Spawn(bool init = false)
    {
        double x  = Util.Rand(0, _w);
        var wave  = _waves[Util.RandInt(0, _waves.Length)];
        double wy = wave.GetY(x);
        double ml = Util.Rand(2.0, 5.0);
        return new() { X = x, Y = init ? Util.Rand(wy, _h) : wy + Util.Rand(5, 60),
            Vx = Util.Rand(-6, 6), Vy = Util.Rand(-4, 4),
            Life = init ? Util.Rand(0, ml) : ml, MaxLife = ml,
            GlowR = Util.Rand(5, 13), Clr = Util.Pick(_colors) };
    }

    public void Update(double dt)
    {
        for (int i = 0; i < _particles.Count; i++)
        {
            var p = _particles[i]; p.Life -= dt;
            if (p.Life <= 0) { _particles[i] = Spawn(); continue; }
            p.X += p.Vx * dt; p.Y += p.Vy * dt;
        }
    }

    public void Draw(DrawingContext dc)
    {
        foreach (var p in _particles)
        {
            double t = p.Life / p.MaxLife;
            // Pulse brightest mid-life
            double pulse = 1 - Math.Abs(t - 0.5) * 2;
            byte a = (byte)Util.Clamp(pulse * 160, 0, 160);
            dc.DrawEllipse(new RadialGradientBrush(
                Palettes.WithAlpha(p.Clr, a), Palettes.WithAlpha(p.Clr, 0)),
                null, new Point(p.X, p.Y), p.GlowR, p.GlowR);
        }
    }
}

// ---------------------------------------------------------------------------
// Moonlight reflection
// ---------------------------------------------------------------------------

sealed class MoonReflection
{
    record struct Streak(double X, double Phase, double Speed, double Width);
    readonly Streak[] _streaks;
    readonly double _w, _horizonY, _bottomY;
    readonly Palette _p;
    double _t;

    public MoonReflection(double moonX, double w, double horizonY, double bottomY, Palette p)
    {
        _w = w; _horizonY = horizonY; _bottomY = bottomY; _p = p;
        _streaks = new Streak[14];
        for (int i = 0; i < 14; i++)
            _streaks[i] = new(moonX + Util.Rand(-24, 24), Util.Rand(0, Math.Tau),
                Util.Rand(0.5, 1.5), Util.RandInt(2, 8));
    }

    public void Update(double dt) => _t += dt;

    public void Draw(DrawingContext dc)
    {
        foreach (var sk in _streaks)
        {
            byte a = (byte)Util.Clamp(20 + 18 * Math.Sin(_t * sk.Speed + sk.Phase), 5, 55);
            double xw = sk.X + 8 * Math.Sin(_t * sk.Speed * 0.7 + sk.Phase);
            for (double y = _horizonY; y < _bottomY; y += 6)
            {
                double tf = (y - _horizonY) / Math.Max(_bottomY - _horizonY, 1);
                byte a2 = (byte)(a * (1 - tf * 0.45));
                double sw = sk.Width * (1 + tf * 2.5);
                double xs = Util.Clamp(xw - sw / 2, 0, _w), xe = Util.Clamp(xw + sw / 2, 0, _w);
                if (xe > xs)
                    dc.DrawRectangle(new SolidColorBrush(Palettes.WithAlpha(_p.Yellow, a2)),
                        null, new Rect(xs, y, xe - xs, 3));
            }
        }
    }
}

// ---------------------------------------------------------------------------
// Distant lights
// ---------------------------------------------------------------------------

sealed class DistantLights
{
    record struct Light(double X, double Y, double T);
    Light[] _lights;
    readonly Palette _p;

    public DistantLights(double[] xs, double y, Palette p)
    {
        _p = p;
        _lights = xs.Select(x => new Light(x, y, Util.Rand(0, Math.Tau))).ToArray();
    }

    public void Update(double dt) =>
        _lights = _lights.Select(l => l with { T = l.T + dt * 1.1 }).ToArray();

    public void Draw(DrawingContext dc)
    {
        foreach (var l in _lights)
        {
            double b = 0.5 + 0.5 * Math.Sin(l.T);
            if (b < 0.35) continue;
            byte a = (byte)Util.Clamp(b * 180, 0, 180);
            foreach (var (r, fa) in new[] { (12, a / 5), (7, a / 3), (4, a) })
                dc.DrawEllipse(new RadialGradientBrush(
                    Palettes.WithAlpha(_p.Yellow, (byte)fa), Palettes.WithAlpha(_p.Yellow, 0)),
                    null, new Point(l.X, l.Y), r, r);
        }
    }
}

// ---------------------------------------------------------------------------
// Lighthouse — silhouette + rotating beam
// ---------------------------------------------------------------------------

sealed class Lighthouse
{
    readonly double _x, _baseY;
    readonly Palette _p;
    double _beam, _t;

    public Lighthouse(double x, double horizonY, Palette p)
    { _x = x; _baseY = horizonY; _p = p; _beam = Math.PI * 1.2; }

    public void Update(double dt) { _t += dt; _beam += dt * 0.7; }

    public void Draw(DrawingContext dc)
    {
        DrawSilhouette(dc);
        DrawBeam(dc);
        DrawLantern(dc);
    }

    void DrawSilhouette(DrawingContext dc)
    {
        var brush = new SolidColorBrush(Palettes.WithAlpha(_p.Crust, 210));
        // Tower body
        var tower = new StreamGeometry();
        using (var ctx = tower.Open())
        {
            ctx.BeginFigure(new Point(_x - 9, _baseY), true, true);
            ctx.LineTo(new Point(_x - 5, _baseY - 48), false, false);
            ctx.LineTo(new Point(_x + 5, _baseY - 48), false, false);
            ctx.LineTo(new Point(_x + 9, _baseY), false, false);
        }
        tower.Freeze();
        dc.DrawGeometry(brush, null, tower);
        // Lantern room
        dc.DrawRectangle(brush, null, new Rect(_x - 7, _baseY - 58, 14, 10));
        // Roof cap
        var roof = new StreamGeometry();
        using (var ctx = roof.Open())
        {
            ctx.BeginFigure(new Point(_x - 8, _baseY - 58), true, true);
            ctx.LineTo(new Point(_x, _baseY - 66), false, false);
            ctx.LineTo(new Point(_x + 8, _baseY - 58), false, false);
        }
        roof.Freeze();
        dc.DrawGeometry(brush, null, roof);
        // Balcony rail
        dc.DrawRectangle(brush, null, new Rect(_x - 8, _baseY - 50, 16, 2));
    }

    void DrawBeam(DrawingContext dc)
    {
        double lx = _x, ly = _baseY - 53;
        double len = 800, spread = 0.07;
        double ax = lx + Math.Cos(_beam) * len, ay = ly + Math.Sin(_beam) * len;
        double bx = lx + Math.Cos(_beam + spread) * len, by = ly + Math.Sin(_beam + spread) * len;

        byte alpha = (byte)(18 + 12 * Math.Sin(_t * 2.5));
        var geom = new StreamGeometry();
        using (var ctx = geom.Open())
        {
            ctx.BeginFigure(new Point(lx, ly), true, true);
            ctx.LineTo(new Point(ax, ay), false, false);
            ctx.LineTo(new Point(bx, by), false, false);
        }
        geom.Freeze();

        var beamBrush = new LinearGradientBrush();
        beamBrush.StartPoint = new Point(0, 0); beamBrush.EndPoint = new Point(1, 0);
        beamBrush.GradientStops.Add(new GradientStop(Palettes.WithAlpha(_p.Yellow, alpha), 0.0));
        beamBrush.GradientStops.Add(new GradientStop(Palettes.WithAlpha(_p.Yellow, 0), 1.0));
        dc.DrawGeometry(beamBrush, null, geom);
    }

    void DrawLantern(DrawingContext dc)
    {
        double b = 0.6 + 0.4 * Math.Sin(_t * 2.2);
        byte a = (byte)(b * 230);
        dc.DrawEllipse(new RadialGradientBrush(
            Palettes.WithAlpha(_p.Yellow, a), Palettes.WithAlpha(_p.Yellow, 0)),
            null, new Point(_x, _baseY - 53), 14, 14);
        dc.DrawEllipse(new SolidColorBrush(Palettes.WithAlpha(_p.Yellow, a)),
            null, new Point(_x, _baseY - 53), 3, 3);
    }
}

// ---------------------------------------------------------------------------
// Rain — diagonal streaks in the sky (stormy mode only)
// ---------------------------------------------------------------------------

sealed class RainEffect
{
    record struct Drop(double X, double Y, double Speed, double Len);
    readonly Drop[] _drops;
    readonly double _w, _h, _horizonY;
    readonly Palette _p;
    static readonly double Angle = Math.PI * 0.5 + 0.18;
    static readonly double Cos   = Math.Cos(Angle), Sin = Math.Sin(Angle);

    public RainEffect(double w, double h, double horizonY, Palette p)
    {
        _w = w; _h = h; _horizonY = horizonY; _p = p;
        _drops = new Drop[220];
        for (int i = 0; i < 220; i++) _drops[i] = SpawnDrop(true);
    }

    Drop SpawnDrop(bool init = false) => new(
        Util.Rand(-60, _w + 60),
        init ? Util.Rand(0, _horizonY + 20) : Util.Rand(-50, -5),
        Util.Rand(280, 480), Util.Rand(8, 18));

    public void Update(double dt)
    {
        for (int i = 0; i < _drops.Length; i++)
        {
            var d = _drops[i];
            double nx = d.X + Cos * d.Speed * dt, ny = d.Y + Sin * d.Speed * dt;
            _drops[i] = ny > _horizonY + 30 ? SpawnDrop() : d with { X = nx, Y = ny };
        }
    }

    public void Draw(DrawingContext dc)
    {
        var pen = new Pen(new SolidColorBrush(Palettes.WithAlpha(_p.Overlay0, 55)), 1);
        foreach (var d in _drops)
        {
            if (d.Y < 0) continue;
            dc.DrawLine(pen,
                new Point(d.X, d.Y),
                new Point(d.X + Cos * d.Len, d.Y + Sin * d.Len));
        }
    }
}

// ---------------------------------------------------------------------------
// Cat mascot
// ---------------------------------------------------------------------------

sealed class CatMascot
{
    readonly BitmapSource _img, _reflection;
    readonly double _w, _h, _px;
    readonly Wave _front;
    double _t, _x;
    readonly double _driftPhase, _bobPhase;

    public CatMascot(double w, double h, Wave front, string sizeKey)
    {
        _w = w; _h = h; _front = front;
        var src = new BitmapImage(new Uri("pack://application:,,,/assets/catppuccin_cat.png"));
        _px = h * (sizeKey switch { "small" => 0.10, "large" => 0.22, _ => 0.155 });
        _img = new TransformedBitmap(src,
            new ScaleTransform(_px / src.PixelWidth, _px / src.PixelHeight));
        _reflection = new TransformedBitmap(src,
            new ScaleTransform(_px / src.PixelWidth, -_px / src.PixelHeight, 0, src.PixelHeight / 2.0));
        _driftPhase = Util.Rand(0, Math.Tau);
        _bobPhase   = Util.Rand(0, Math.Tau);
        _x = w * 0.5;
    }

    public void Update(double dt) => _t += dt;

    public void Draw(DrawingContext dc)
    {
        _x = _w * 0.5 + _w * 0.28 * Math.Sin(_t * 0.028 + _driftPhase);
        double waveY = _front.GetY(_x);
        double bob   = 7 * Math.Sin(_t * 0.9 + _bobPhase);
        double ix    = _x - _px / 2, iy = waveY - _px + bob;
        dc.PushOpacity(0.22);
        dc.DrawImage(_reflection, new Rect(ix, waveY + bob, _px, _px));
        dc.Pop();
        dc.DrawImage(_img, new Rect(ix, iy, _px, _px));
    }
}

// ---------------------------------------------------------------------------
// Sky / sea gradients — time-of-day aware
// ---------------------------------------------------------------------------

static class Background
{
    public static void DrawSky(DrawingContext dc, double w, double horizonY, Palette p, string tod)
    {
        Color top, mid, hor;
        if (tod == "day")
        {
            top = Palettes.Lerp(p.Sky,     Colors.White, p.IsLight ? 0.4 : 0.0);
            mid = Palettes.Lerp(p.Sky,     p.Sapphire,   0.35);
            hor = Palettes.Lerp(p.Sapphire, p.Teal,      0.25);
        }
        else if (tod == "dusk")
        {
            top = Palettes.Lerp(p.Mantle, p.Mauve, 0.35);
            mid = Palettes.Lerp(p.Mauve,  p.Peach, 0.45);
            hor = Palettes.Lerp(p.Peach,  p.Yellow, 0.55);
        }
        else // night
        {
            top = p.IsLight ? Palettes.Lerp(p.Sky, Colors.White, 0.55) : p.Crust;
            mid = p.Base;
            hor = p.IsLight ? Palettes.Lerp(p.Sapphire, p.Teal, 0.3)
                            : Palettes.Lerp(p.Surface1,  p.Sapphire, 0.22);
        }
        var brush = new LinearGradientBrush();
        brush.StartPoint = new Point(0, 0); brush.EndPoint = new Point(0, 1);
        brush.GradientStops.Add(new GradientStop(top, 0.0));
        brush.GradientStops.Add(new GradientStop(mid, 0.5));
        brush.GradientStops.Add(new GradientStop(hor, 1.0));
        brush.Freeze();
        dc.DrawRectangle(brush, null, new Rect(0, 0, w, horizonY));
    }

    public static void DrawSea(DrawingContext dc, double w, double h, double horizonY, Palette p, string tod)
    {
        Color top, bot;
        if (tod == "day")
        {
            top = Palettes.Lerp(p.Sky,     p.Teal,     0.4);
            bot = Palettes.Lerp(p.Sapphire, p.Blue,    0.3);
        }
        else if (tod == "dusk")
        {
            top = Palettes.Lerp(p.Pink, p.Peach, 0.55);
            bot = Palettes.Lerp(p.Crust, p.Mantle, 0.5);
        }
        else // night
        {
            top = p.IsLight ? Palettes.Lerp(p.Sky, p.Teal, 0.35)
                            : Palettes.Lerp(p.Sapphire, p.Surface1, 0.28);
            bot = p.IsLight ? Palettes.Lerp(p.Sapphire, p.Surface0, 0.4)
                            : Palettes.Lerp(p.Base, p.Crust, 0.5);
        }
        var brush = new LinearGradientBrush();
        brush.StartPoint = new Point(0, 0); brush.EndPoint = new Point(0, 1);
        brush.GradientStops.Add(new GradientStop(top, 0.0));
        brush.GradientStops.Add(new GradientStop(bot, 1.0));
        brush.Freeze();
        dc.DrawRectangle(brush, null, new Rect(0, horizonY, w, h - horizonY));
    }
}

// ---------------------------------------------------------------------------
// Clock overlay — position + format aware
// ---------------------------------------------------------------------------

static class ClockOverlay
{
    static Typeface? _face;

    public static void Draw(DrawingContext dc, double w, double h, Palette p,
                            double ppd, string position, string format)
    {
        _face ??= new Typeface("Segoe UI");
        var now  = DateTime.Now;
        string timeStr = format == "12h"
            ? now.ToString("h:mm tt", CultureInfo.InvariantCulture)
            : now.ToString("HH:mm");
        string dateStr = $"{now:dddd, MMMM} {now.Day}";

        double ts = h * 0.072, ds = h * 0.028;
        var ft  = new FormattedText(timeStr, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, _face, ts, new SolidColorBrush(p.Text),    ppd);
        var fd  = new FormattedText(dateStr, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, _face, ds, new SolidColorBrush(p.Subtext0), ppd);
        var fts = new FormattedText(timeStr, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, _face, ts, new SolidColorBrush(Palettes.WithAlpha(p.Crust, 180)), ppd);
        var fds = new FormattedText(dateStr, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, _face, ds, new SolidColorBrush(Palettes.WithAlpha(p.Crust, 180)), ppd);

        const double pad = 36;
        double blockH  = ft.Height + 4 + fd.Height;
        bool right  = position.Contains("right");
        bool bottom = position.Contains("bottom");

        double ty = bottom ? h - blockH - pad : pad;
        double dy = ty + ft.Height + 4;

        double tx = right ? w - ft.Width - pad : pad;
        double dx = right ? w - fd.Width - pad : pad;

        dc.DrawText(fts, new Point(tx + 2, ty + 2)); dc.DrawText(fds, new Point(dx + 2, dy + 2));
        dc.DrawText(ft,  new Point(tx, ty));          dc.DrawText(fd,  new Point(dx, dy));
    }
}

// ---------------------------------------------------------------------------
// Wave factory
// ---------------------------------------------------------------------------

static class WaveFactory
{
    public static Wave[] Build(string layers, double horizonY, double h, Palette p)
    {
        bool few  = layers == "few";
        bool many = layers == "many";

        // Each entry: (yFrac, color, alpha, speed, amp, freq)
        var defs = new (double yf, Color c, byte a, double spd, double amp, double freq)[]
        {
            (0.06, p.Sapphire, 65,  0.28, 11, 0.013),
            (0.10, p.Sapphire, 80,  0.35, 14, 0.012),
            (0.16, p.Blue,     100, 0.50, 18, 0.010),
            (0.22, p.Teal,     115, 0.65, 22, 0.009),
            (0.30, p.Sapphire, 130, 0.80, 26, 0.008),
            (0.36, p.Blue,     138, 0.90, 28, 0.0075),
            (0.42, p.Sky,      148, 1.05, 32, 0.007),
        };

        int[] indices = few  ? [2, 4, 6]
                      : many ? [0, 1, 2, 3, 4, 5, 6]
                             : [1, 2, 3, 4, 6]; // normal = 5

        return indices.Select(i =>
        {
            var (yf, c, a, spd, amp, freq) = defs[i];
            return new Wave(horizonY + h * yf, c, a, spd, amp, freq);
        }).ToArray();
    }
}

// ---------------------------------------------------------------------------
// Scene orchestrator
// ---------------------------------------------------------------------------

public sealed class CoastScene
{
    readonly StarField         _stars;
    readonly ShootingStarField _shooting;
    readonly CelestialBody     _celestial;
    readonly Aurora            _aurora;
    readonly Wave[]            _waves;
    readonly Wave              _front;
    readonly FoamField         _foam;
    readonly BioGlow           _bio;
    readonly MoonReflection    _reflection;
    readonly DistantLights     _lights;
    readonly Lighthouse?       _lighthouse;
    readonly RainEffect        _rain;
    readonly CatMascot         _cat;
    readonly AppSettings       _s;
    readonly Palette           _p;
    readonly string            _tod;
    readonly double            _horizonY;

    public CoastScene(AppSettings s, double w, double h)
    {
        _s   = s;
        _p   = Palettes.All.GetValueOrDefault(s.Flavor, Palettes.All["mocha"]);
        _tod = s.TimeOfDay;
        _horizonY = h * 0.52;

        int starCount = s.StarDensity switch { "sparse" => 75, "dense" => 400, _ => 220 };
        _stars    = new StarField(starCount, w, h, _p);
        _shooting = new ShootingStarField(3, w, h, _p);
        _celestial = new CelestialBody(w * 0.18, h * 0.14, _p, _tod);
        _aurora    = new Aurora(w, h, _p);
        _waves     = WaveFactory.Build(s.WaveLayers, _horizonY, h, _p);
        _front     = _waves[^1];
        _foam      = new FoamField(w, h, _front, _p);
        _bio       = new BioGlow(w, h, _waves, _p);
        _reflection = new MoonReflection(w * 0.18, w, _horizonY, h, _p);
        _lights    = new DistantLights([w * 0.72, w * 0.81], _horizonY - 2, _p);
        _lighthouse = s.ShowLighthouse ? new Lighthouse(w * 0.87, _horizonY, _p) : null;
        _rain      = new RainEffect(w, h, _horizonY, _p);
        _cat       = new CatMascot(w, h, _front, s.CatSize);
    }

    public void Update(double dt)
    {
        double sm = _s.WaveSpeed;
        _stars.Update(dt);
        if (_s.ShowShooting) _shooting.Update(dt);
        _celestial.Update(dt);
        if (_s.ShowAurora) _aurora.Update(dt);
        foreach (var w in _waves) w.Update(dt, sm);
        _cat.Update(dt);
        if (_s.ShowFoam) _foam.Update(dt);
        if (_s.ShowBio)  _bio.Update(dt);
        _reflection.Update(dt);
        _lights.Update(dt);
        _lighthouse?.Update(dt);
        if (_s.ShowRain && _s.WaveSpeed >= 1.8) _rain.Update(dt);
    }

    public void Draw(DrawingContext dc, double w, double h, double ppd)
    {
        // 1. Background
        Background.DrawSky(dc, w, _horizonY, _p, _tod);
        Background.DrawSea(dc, w, h, _horizonY, _p, _tod);

        // 2. Rain (behind everything in sky layer)
        if (_s.ShowRain && _s.WaveSpeed >= 1.8) _rain.Draw(dc);

        // 3. Aurora (night/dusk only)
        if (_s.ShowAurora && _tod != "day") _aurora.Draw(dc);

        // 4. Stars (fade out at dusk, invisible by day)
        double starOpacity = _tod switch { "day" => 0.0, "dusk" => 0.25, _ => 1.0 };
        _stars.Draw(dc, starOpacity);
        if (_s.ShowShooting && _tod == "night") _shooting.Draw(dc);

        // 5. Celestial body
        _celestial.Draw(dc);

        // 6. Horizon details
        _lights.Draw(dc);
        _lighthouse?.Draw(dc);

        // 7. Reflection (night only, matching moon)
        if (_tod == "night") _reflection.Draw(dc);

        // 8. Waves + bioluminescence + foam
        foreach (var wave in _waves) wave.Draw(dc, w, h);
        if (_s.ShowBio && _tod != "day") _bio.Draw(dc);
        _cat.Draw(dc);
        if (_s.ShowFoam) _foam.Draw(dc);

        // 9. UI overlay
        if (_s.ShowClock)
            ClockOverlay.Draw(dc, w, h, _p, ppd, _s.ClockPos, _s.ClockFormat);
    }
}
