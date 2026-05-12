using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CatppuccinCoast;

public sealed class SceneHost : FrameworkElement
{
    readonly DrawingVisual _visual = new();
    readonly CoastScene    _scene;
    readonly double        _initW, _initH;
    double                 _pixelsPerDip = 1.0;
    DateTime               _lastFrame    = DateTime.UtcNow;

    public SceneHost(CoastScene scene, double w, double h)
    {
        _scene = scene; _initW = w; _initH = h;
        AddVisualChild(_visual);
        Loaded += (_, _) =>
        {
            _pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            CompositionTarget.Rendering += OnRender;
        };
    }

    public void StopRendering() => CompositionTarget.Rendering -= OnRender;

    protected override int VisualChildrenCount => 1;
    protected override Visual GetVisualChild(int index) => _visual;

    void OnRender(object? s, EventArgs e)
    {
        var now = DateTime.UtcNow;
        double dt = Math.Min((now - _lastFrame).TotalSeconds, 0.05);
        _lastFrame = now;

        double w = ActualWidth  > 1 ? ActualWidth  : _initW;
        double h = ActualHeight > 1 ? ActualHeight : _initH;

        _scene.Update(dt);
        using var dc = _visual.RenderOpen();
        _scene.Draw(dc, w, h, _pixelsPerDip);
    }
}

public sealed class ScreensaverWindow : Window
{
    SceneHost? _host;
    Point      _lastMouse;
    int        _mouseMoves;
    bool       _quitting;

    public ScreensaverWindow()
    {
        WindowStyle = WindowStyle.None;
        WindowState = WindowState.Maximized;
        ResizeMode  = ResizeMode.NoResize;
        Topmost     = true;
        Cursor      = Cursors.None;
        Background  = Brushes.Black;

        Loaded    += OnLoaded;
        KeyDown   += (_, _) => Quit();
        MouseDown += (_, _) => Quit();
        MouseMove += OnMouseMove;
    }

    void OnLoaded(object s, RoutedEventArgs e)
    {
        _lastMouse = Mouse.GetPosition(this);
        var scene  = new CoastScene(AppSettings.Load(), ActualWidth, ActualHeight);
        _host      = new SceneHost(scene, ActualWidth, ActualHeight);
        Content    = _host;
    }

    void OnMouseMove(object s, MouseEventArgs e)
    {
        var pos = e.GetPosition(this);
        if (Math.Abs(pos.X - _lastMouse.X) + Math.Abs(pos.Y - _lastMouse.Y) > 6)
            if (++_mouseMoves > 3) Quit();
        _lastMouse = pos;
    }

    void Quit()
    {
        if (_quitting) return;
        _quitting = true;
        _host?.StopRendering();   // detach render callback before anything else
        Environment.Exit(0);      // guaranteed process termination — no stragglers
    }
}
