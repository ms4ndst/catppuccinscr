using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CatppuccinCoast;

// ---------------------------------------------------------------------------
// Window rain — streaks sliding down the glass pane
// ---------------------------------------------------------------------------

sealed class WindowRain
{
    sealed class Streak
    {
        public double X, Y, Speed, Len;
        public byte Alpha;
    }

    readonly List<Streak> _streaks = [];
    readonly double _left, _top, _right, _bottom;
    readonly Color _color;

    public WindowRain(int count, double left, double top, double right, double bottom, Color color)
    {
        _left = left; _top = top; _right = right; _bottom = bottom; _color = color;
        for (int i = 0; i < count; i++) _streaks.Add(Spawn(true));
    }

    Streak Spawn(bool init = false) => new()
    {
        X = Util.Rand(_left + 4, _right - 4),
        Y = init ? Util.Rand(_top, _bottom) : Util.Rand(_top - 20, _top),
        Speed = Util.Rand(60, 180),
        Len = Util.Rand(8, 28),
        Alpha = (byte)Util.RandInt(25, 70)
    };

    public void Update(double dt)
    {
        for (int i = 0; i < _streaks.Count; i++)
        {
            var s = _streaks[i];
            s.Y += s.Speed * dt;
            if (s.Y > _bottom + 10) _streaks[i] = Spawn();
        }
    }

    public void Draw(DrawingContext dc)
    {
        foreach (var s in _streaks)
        {
            if (s.Y < _top) continue;
            dc.DrawLine(
                new Pen(new SolidColorBrush(Palettes.WithAlpha(_color, s.Alpha)), 1),
                new Point(s.X, s.Y),
                new Point(s.X - 1, Math.Min(s.Y + s.Len, _bottom)));
        }
    }
}

// ---------------------------------------------------------------------------
// Steam particles — rising wisps from the mug
// ---------------------------------------------------------------------------

sealed class SteamEffect
{
    sealed class Wisp { public double X, Y, Vx, Vy, Life, MaxLife, Size; }
    readonly List<Wisp> _wisps = [];
    readonly double _originX, _originY;
    readonly Color _color;

    public SteamEffect(double originX, double originY, int count, Color color)
    {
        _originX = originX; _originY = originY; _color = color;
        for (int i = 0; i < count; i++) _wisps.Add(Spawn(true));
    }

    Wisp Spawn(bool init = false)
    {
        double ml = Util.Rand(2.0, 4.5);
        return new()
        {
            X = _originX + Util.Rand(-6, 6), Y = init ? _originY - Util.Rand(0, 60) : _originY,
            Vx = Util.Rand(-5, 5), Vy = -Util.Rand(12, 28),
            Life = init ? Util.Rand(0, ml) : ml, MaxLife = ml,
            Size = Util.Rand(4, 10)
        };
    }

    public void Update(double dt)
    {
        for (int i = 0; i < _wisps.Count; i++)
        {
            var w = _wisps[i]; w.Life -= dt;
            if (w.Life <= 0) { _wisps[i] = Spawn(); continue; }
            w.X += w.Vx * dt; w.Y += w.Vy * dt;
            w.Vx += Util.Rand(-3, 3) * dt; // gentle drift
            w.Size += dt * 2; // expand as it rises
        }
    }

    public void Draw(DrawingContext dc)
    {
        foreach (var w in _wisps)
        {
            double t = w.Life / w.MaxLife;
            byte a = (byte)Util.Clamp(t * 40, 0, 40); // very subtle
            dc.DrawEllipse(new RadialGradientBrush(
                Palettes.WithAlpha(_color, a), Palettes.WithAlpha(_color, 0)),
                null, new Point(w.X, w.Y), w.Size, w.Size);
        }
    }
}

// ---------------------------------------------------------------------------
// String lights — twinkling dots across the top
// ---------------------------------------------------------------------------

sealed class StringLights
{
    record struct Bulb(double X, double Y, Color Clr, double Phase, double Speed);
    readonly Bulb[] _bulbs;
    double _t;

    public StringLights(double x1, double y1, double x2, double y2, int count, Palette p)
    {
        var colors = new[] { p.Yellow, p.Peach, p.Rosewater, p.Flamingo, p.Pink };
        _bulbs = new Bulb[count];
        for (int i = 0; i < count; i++)
        {
            double frac = (double)i / (count - 1);
            double x = x1 + (x2 - x1) * frac;
            // Catenary sag
            double sag = 18 * Math.Sin(frac * Math.PI);
            double y = y1 + (y2 - y1) * frac + sag;
            _bulbs[i] = new(x, y, Util.Pick(colors), Util.Rand(0, Math.Tau), Util.Rand(0.6, 1.8));
        }
    }

    public void Update(double dt) => _t += dt;

    public void Draw(DrawingContext dc)
    {
        // Wire
        if (_bulbs.Length > 1)
        {
            var pen = new Pen(new SolidColorBrush(Palettes.WithAlpha(_bulbs[0].Clr, 30)), 1);
            for (int i = 0; i < _bulbs.Length - 1; i++)
                dc.DrawLine(pen, new Point(_bulbs[i].X, _bulbs[i].Y),
                    new Point(_bulbs[i + 1].X, _bulbs[i + 1].Y));
        }
        // Bulbs
        foreach (var b in _bulbs)
        {
            double pulse = 0.5 + 0.5 * Math.Sin(_t * b.Speed + b.Phase);
            byte a = (byte)Util.Clamp(pulse * 220, 40, 220);
            byte ga = (byte)Util.Clamp(pulse * 50, 0, 50);
            dc.DrawEllipse(new RadialGradientBrush(
                Palettes.WithAlpha(b.Clr, ga), Palettes.WithAlpha(b.Clr, 0)),
                null, new Point(b.X, b.Y), 10, 10);
            dc.DrawEllipse(new SolidColorBrush(Palettes.WithAlpha(b.Clr, a)),
                null, new Point(b.X, b.Y), 2.5, 2.5);
        }
    }
}

// ---------------------------------------------------------------------------
// Desk silhouette — static geometric shapes drawn once
// ---------------------------------------------------------------------------

static class DeskDrawer
{
    public static void Draw(DrawingContext dc, double w, double h, Palette p,
                            out double mugX, out double mugTopY,
                            out double monitorCx, out double monitorCy,
                            out double monitorW, out double monitorH)
    {
        var dark = new SolidColorBrush(Palettes.WithAlpha(p.Crust, 230));
        dark.Freeze();
        var mid = new SolidColorBrush(Palettes.WithAlpha(p.Mantle, 220));
        mid.Freeze();

        double deskY = h * 0.68;
        double deskH = h * 0.04;

        // Desk surface
        dc.DrawRectangle(mid, null, new Rect(w * 0.08, deskY, w * 0.84, deskH));
        // Desk legs
        dc.DrawRectangle(dark, null, new Rect(w * 0.12, deskY + deskH, w * 0.03, h - deskY - deskH));
        dc.DrawRectangle(dark, null, new Rect(w * 0.85, deskY + deskH, w * 0.03, h - deskY - deskH));

        // Monitor
        monitorW = w * 0.28;
        monitorH = h * 0.22;
        double mx = w * 0.38;
        double my = deskY - monitorH - h * 0.02;
        monitorCx = mx + monitorW / 2;
        monitorCy = my + monitorH / 2;
        dc.DrawRoundedRectangle(dark, null, new Rect(mx, my, monitorW, monitorH), 4, 4);
        // Monitor stand
        dc.DrawRectangle(dark, null, new Rect(monitorCx - w * 0.015, my + monitorH, w * 0.03, deskY - my - monitorH));
        dc.DrawRectangle(dark, null, new Rect(monitorCx - w * 0.04, deskY - h * 0.008, w * 0.08, h * 0.008));

        // Mug — right of monitor
        mugX = w * 0.73;
        double mugW = w * 0.032, mugH = h * 0.05;
        mugTopY = deskY - mugH;
        dc.DrawRoundedRectangle(dark, null, new Rect(mugX - mugW / 2, mugTopY, mugW, mugH), 2, 2);
        // Mug handle
        dc.DrawEllipse(null, new Pen(dark, 2), new Point(mugX + mugW / 2 + 4, mugTopY + mugH * 0.4), 5, 6);

        // Plant — left side
        double px = w * 0.20, potW = w * 0.04, potH = h * 0.05;
        double potY = deskY - potH;
        dc.DrawRoundedRectangle(mid, null, new Rect(px - potW / 2, potY, potW, potH), 2, 2);
        // Leaves
        var leaf = new SolidColorBrush(Palettes.WithAlpha(p.Green, 180));
        for (int i = 0; i < 5; i++)
        {
            double angle = -Math.PI / 2 + (i - 2) * 0.35;
            double len = h * 0.04 + (i % 2) * h * 0.015;
            double lx = px + Math.Cos(angle) * len, ly = potY + Math.Sin(angle) * len;
            dc.DrawLine(new Pen(leaf, 2), new Point(px, potY), new Point(lx, ly));
            dc.DrawEllipse(leaf, null, new Point(lx, ly), 5, 3);
        }

        // Keyboard
        double kbW = w * 0.16, kbH = h * 0.018;
        double kbX = monitorCx - kbW / 2, kbY = deskY - kbH - 2;
        dc.DrawRoundedRectangle(dark, null, new Rect(kbX, kbY, kbW, kbH), 2, 2);
    }
}

// ---------------------------------------------------------------------------
// Cat silhouette on desk — simple geometric shape with glowing eyes
// ---------------------------------------------------------------------------

sealed class DeskCat
{
    readonly double _x, _baseY;
    readonly Palette _p;
    double _t;

    public DeskCat(double x, double baseY, Palette p)
    { _x = x; _baseY = baseY; _p = p; }

    public void Update(double dt) => _t += dt;

    public void Draw(DrawingContext dc)
    {
        var body = new SolidColorBrush(Palettes.WithAlpha(_p.Crust, 230));
        double bw = 22, bh = 26;
        double by = _baseY - bh;

        // Body
        dc.DrawEllipse(body, null, new Point(_x, by + bh * 0.4), bw / 2, bh / 2);
        // Head
        dc.DrawEllipse(body, null, new Point(_x, by - 2), 10, 9);
        // Ears
        var ear = new StreamGeometry();
        using (var ctx = ear.Open())
        {
            ctx.BeginFigure(new Point(_x - 9, by - 4), true, true);
            ctx.LineTo(new Point(_x - 5, by - 16), false, false);
            ctx.LineTo(new Point(_x - 2, by - 5), false, false);
        }
        ear.Freeze();
        dc.DrawGeometry(body, null, ear);
        var ear2 = new StreamGeometry();
        using (var ctx = ear2.Open())
        {
            ctx.BeginFigure(new Point(_x + 9, by - 4), true, true);
            ctx.LineTo(new Point(_x + 5, by - 16), false, false);
            ctx.LineTo(new Point(_x + 2, by - 5), false, false);
        }
        ear2.Freeze();
        dc.DrawGeometry(body, null, ear2);
        // Tail
        dc.DrawLine(new Pen(body, 3),
            new Point(_x + bw / 2 - 2, by + bh * 0.3),
            new Point(_x + bw / 2 + 14, by + bh * 0.1 + 4 * Math.Sin(_t * 0.8)));

        // Eyes — glowing
        double blink = Math.Sin(_t * 0.3);
        if (blink > -0.92) // Occasional blink
        {
            byte ea = (byte)(140 + 60 * Math.Sin(_t * 1.2));
            dc.DrawEllipse(new SolidColorBrush(Palettes.WithAlpha(_p.Mauve, ea)),
                null, new Point(_x - 4, by - 2), 1.8, 1.8);
            dc.DrawEllipse(new SolidColorBrush(Palettes.WithAlpha(_p.Mauve, ea)),
                null, new Point(_x + 4, by - 2), 1.8, 1.8);
            // Eye glow
            byte ga = (byte)(ea / 4);
            dc.DrawEllipse(new RadialGradientBrush(
                Palettes.WithAlpha(_p.Mauve, ga), Palettes.WithAlpha(_p.Mauve, 0)),
                null, new Point(_x, by - 2), 8, 6);
        }
    }
}

// ---------------------------------------------------------------------------
// Lofi scene orchestrator
// ---------------------------------------------------------------------------

public sealed class LofiScene : IScene
{
    readonly WindowRain     _rain;
    readonly SteamEffect    _steam;
    readonly StringLights   _lights;
    readonly DeskCat        _cat;
    readonly StarField      _stars;
    readonly AppSettings    _s;
    readonly Palette        _p;
    readonly double         _w, _h;
    // Desk geometry (recomputed each frame by DeskDrawer)
    double _mugX, _mugTopY, _monCx, _monCy, _monW, _monH;
    double _t;

    // Window bounds (the "outside" view)
    readonly double _winL, _winT, _winR, _winB;

    public LofiScene(AppSettings s, double w, double h)
    {
        _s = s; _w = w; _h = h;
        _p = Palettes.All.GetValueOrDefault(s.Flavor, Palettes.All["mocha"]);

        // Window occupies center-ish area
        _winL = w * 0.22; _winT = h * 0.06;
        _winR = w * 0.78; _winB = h * 0.55;

        _rain  = new WindowRain(120, _winL, _winT, _winR, _winB, _p.Overlay0);
        _stars = new StarField(80, _winR - _winL, _winB - _winT, _p);
        _steam = new SteamEffect(w * 0.73, h * 0.63, 15, _p.Overlay0);
        _lights = new StringLights(_winL - w * 0.04, _winT - h * 0.02,
                                   _winR + w * 0.04, _winT + h * 0.01, 18, _p);
        _cat = new DeskCat(w * 0.30, h * 0.68, _p);
    }

    public void Update(double dt)
    {
        _t += dt;
        _rain.Update(dt);
        _stars.Update(dt);
        _steam.Update(dt);
        _lights.Update(dt);
        _cat.Update(dt);
    }

    public void Draw(DrawingContext dc, double w, double h, double ppd)
    {
        // 1. Room background (dark wall)
        dc.DrawRectangle(new SolidColorBrush(_p.Crust), null, new Rect(0, 0, w, h));

        // 2. Wall behind window (slightly lighter)
        dc.DrawRectangle(new SolidColorBrush(Palettes.WithAlpha(_p.Mantle, 255)),
            null, new Rect(0, 0, w, h));

        // 3. Window — night sky view
        DrawWindowView(dc);

        // 4. Rain on glass
        _rain.Draw(dc);

        // 5. Window frame
        DrawWindowFrame(dc);

        // 6. String lights (across top of window)
        _lights.Draw(dc);

        // 7. Desk + objects
        DeskDrawer.Draw(dc, w, h, _p,
            out _mugX, out _mugTopY,
            out _monCx, out _monCy, out _monW, out _monH);

        // 8. Monitor glow
        DrawMonitorGlow(dc);

        // 9. Steam
        _steam.Draw(dc);

        // 10. Cat
        _cat.Draw(dc);

        // 11. Ambient room glow (warm overlay from string lights)
        DrawAmbientGlow(dc);

        // 12. Clock
        if (_s.ShowClock)
            ClockOverlay.Draw(dc, w, h, _p, ppd, _s.ClockPos, _s.ClockFormat);
    }

    void DrawWindowView(DrawingContext dc)
    {
        // Sky gradient inside window
        var sky = new LinearGradientBrush();
        sky.StartPoint = new Point(0, 0); sky.EndPoint = new Point(0, 1);
        sky.GradientStops.Add(new GradientStop(_p.Crust, 0.0));
        sky.GradientStops.Add(new GradientStop(_p.Base, 0.6));
        sky.GradientStops.Add(new GradientStop(Palettes.Lerp(_p.Surface0, _p.Sapphire, 0.15), 1.0));
        dc.DrawRectangle(sky, null, new Rect(_winL, _winT, _winR - _winL, _winB - _winT));

        // Stars inside window (translate to window coords)
        dc.PushTransform(new TranslateTransform(_winL, _winT));
        dc.PushClip(new RectangleGeometry(new Rect(0, 0, _winR - _winL, _winB - _winT)));
        _stars.Draw(dc, 0.7);
        dc.Pop(); dc.Pop();
    }

    void DrawWindowFrame(DrawingContext dc)
    {
        var frame = new SolidColorBrush(Palettes.WithAlpha(_p.Surface1, 200));
        double t = 5; // frame thickness
        dc.DrawRectangle(frame, null, new Rect(_winL - t, _winT - t, _winR - _winL + 2 * t, t));         // top
        dc.DrawRectangle(frame, null, new Rect(_winL - t, _winB, _winR - _winL + 2 * t, t));             // bottom
        dc.DrawRectangle(frame, null, new Rect(_winL - t, _winT, t, _winB - _winT));                      // left
        dc.DrawRectangle(frame, null, new Rect(_winR, _winT, t, _winB - _winT));                           // right
        // Cross bars
        double midX = (_winL + _winR) / 2, midY = (_winT + _winB) / 2;
        dc.DrawRectangle(frame, null, new Rect(midX - 1.5, _winT, 3, _winB - _winT));
        dc.DrawRectangle(frame, null, new Rect(_winL, midY - 1.5, _winR - _winL, 3));
    }

    void DrawMonitorGlow(DrawingContext dc)
    {
        double pulse = 0.7 + 0.3 * Math.Sin(_t * 0.15);
        // Screen content — subtle gradient
        var screen = new LinearGradientBrush();
        screen.StartPoint = new Point(0, 0); screen.EndPoint = new Point(1, 1);
        screen.GradientStops.Add(new GradientStop(
            Palettes.WithAlpha(_p.Mauve, (byte)(40 * pulse)), 0.0));
        screen.GradientStops.Add(new GradientStop(
            Palettes.WithAlpha(_p.Lavender, (byte)(55 * pulse)), 0.5));
        screen.GradientStops.Add(new GradientStop(
            Palettes.WithAlpha(_p.Blue, (byte)(35 * pulse)), 1.0));
        dc.DrawRoundedRectangle(screen, null,
            new Rect(_monCx - _monW / 2 + 3, _monCy - _monH / 2 + 3, _monW - 6, _monH - 6), 2, 2);

        // Glow cast onto desk
        byte ga = (byte)(18 * pulse);
        dc.DrawEllipse(new RadialGradientBrush(
            Palettes.WithAlpha(_p.Lavender, ga), Palettes.WithAlpha(_p.Lavender, 0)),
            null, new Point(_monCx, _monCy + _monH * 0.6), _monW * 0.8, _monH * 0.5);
    }

    void DrawAmbientGlow(DrawingContext dc)
    {
        // Warm glow from string lights onto ceiling/wall
        double pulse = 0.6 + 0.4 * Math.Sin(_t * 0.2);
        byte a = (byte)(8 * pulse);
        dc.DrawEllipse(new RadialGradientBrush(
            Palettes.WithAlpha(_p.Yellow, a), Palettes.WithAlpha(_p.Yellow, 0)),
            null, new Point(_w * 0.5, _winT), _w * 0.5, _h * 0.35);
    }
}
