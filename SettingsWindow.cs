using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CatppuccinCoast;

public sealed class SettingsWindow : Window
{
    static readonly Palette Ui = Palettes.All["mocha"];
    static SolidColorBrush Br(Color c) => new(c);
    static SolidColorBrush Br(Color c, byte a) => new(Palettes.WithAlpha(c, a));

    readonly AppSettings _s = AppSettings.Load();

    string _flavor;
    string _tod, _clockPos, _clockFmt, _starDensity, _waveLayers, _catSize;
    double _waveSpeed;
    bool   _clock, _aurora, _shooting, _foam, _bio, _lighthouse, _rain;

    public SettingsWindow()
    {
        _flavor      = _s.Flavor;
        _tod         = _s.TimeOfDay;
        _clockPos    = _s.ClockPos;
        _clockFmt    = _s.ClockFormat;
        _starDensity = _s.StarDensity;
        _waveLayers  = _s.WaveLayers;
        _catSize     = _s.CatSize;
        _waveSpeed   = _s.WaveSpeed;
        _clock       = _s.ShowClock;
        _aurora      = _s.ShowAurora;
        _shooting    = _s.ShowShooting;
        _foam        = _s.ShowFoam;
        _bio         = _s.ShowBio;
        _lighthouse  = _s.ShowLighthouse;
        _rain        = _s.ShowRain;

        Title         = "Catppuccin Coast — Settings";
        Width         = 600;
        Height        = 700;
        ResizeMode    = ResizeMode.NoResize;
        Background    = Br(Ui.Base);
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Content = new ScrollViewer
        {
            Content = BuildUi(),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
    }

    // -----------------------------------------------------------------------
    // Layout
    // -----------------------------------------------------------------------

    UIElement BuildUi()
    {
        var root = new StackPanel { Background = Br(Ui.Base) };

        root.Children.Add(BuildHeader());
        root.Children.Add(Divider());

        root.Children.Add(Section("FLAVOR",      BuildFlavorRow()));
        root.Children.Add(Divider());
        root.Children.Add(Section("TIME OF DAY", ChoiceRow(
            ["Night", "Dusk", "Day"], _tod,
            ["night", "dusk", "day"], v => _tod = v)));
        root.Children.Add(Divider());
        root.Children.Add(Section("FEATURES",    BuildToggles()));
        root.Children.Add(Divider());
        root.Children.Add(Section("WAVE SPEED",  ChoiceRow(
            ["Calm", "Normal", "Stormy"],
            _waveSpeed switch { 0.5 => "calm", 1.8 => "stormy", _ => "normal" },
            ["calm", "normal", "stormy"],
            v => _waveSpeed = v switch { "calm" => 0.5, "stormy" => 1.8, _ => 1.0 })));
        root.Children.Add(Divider());
        root.Children.Add(Section("WAVE LAYERS", ChoiceRow(
            ["Few (3)", "Normal (5)", "Many (7)"], _waveLayers,
            ["few", "normal", "many"], v => _waveLayers = v)));
        root.Children.Add(Divider());
        root.Children.Add(Section("STAR DENSITY", ChoiceRow(
            ["Sparse", "Normal", "Dense"], _starDensity,
            ["sparse", "normal", "dense"], v => _starDensity = v)));
        root.Children.Add(Divider());
        root.Children.Add(Section("CAT SIZE", ChoiceRow(
            ["Small", "Medium", "Large"], _catSize,
            ["small", "medium", "large"], v => _catSize = v)));
        root.Children.Add(Divider());
        root.Children.Add(Section("CLOCK", BuildClockSection()));
        root.Children.Add(Divider());
        root.Children.Add(BuildSaveButton());

        return root;
    }

    // -----------------------------------------------------------------------
    // Header
    // -----------------------------------------------------------------------

    Border BuildHeader()
    {
        var cat = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/assets/catppuccin_cat.png")),
            Width = 72, Height = 72, Margin = new Thickness(16, 12, 16, 12)
        };
        var title = TB("Catppuccin Coast", Ui.Lavender, 22, FontWeights.SemiBold);
        title.Margin = new Thickness(0, 14, 0, 2);
        var sub = TB("Screensaver Settings", Ui.Subtext0, 13);

        var texts = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        texts.Children.Add(title); texts.Children.Add(sub);

        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(cat); row.Children.Add(texts);
        return new Border { Background = Br(Ui.Surface0), Child = row };
    }

    // -----------------------------------------------------------------------
    // Flavor cards
    // -----------------------------------------------------------------------

    UIElement BuildFlavorRow()
    {
        var grid = new UniformGrid { Columns = 4 };
        var cards = new Border[4];
        var keys  = new[] { "latte", "frappe", "macchiato", "mocha" };

        for (int i = 0; i < keys.Length; i++)
        {
            var fp   = Palettes.All[keys[i]];
            var key  = keys[i];
            var dots = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(6, 8, 6, 4) };
            foreach (var c in new[] { fp.Blue, fp.Teal, fp.Mauve, fp.Peach, fp.Green })
                dots.Children.Add(new Border { Width=13, Height=13, CornerRadius=new(6.5),
                    Background=Br(c), Margin=new Thickness(2,0,2,0) });

            var name = TB(fp.Name, fp.Text, 13);
            name.Margin = new Thickness(6, 2, 6, 8);

            var inner = new StackPanel();
            inner.Children.Add(dots); inner.Children.Add(name);

            var card = new Border { Background = Br(fp.Base), CornerRadius = new(8),
                Margin = new Thickness(4), Cursor = Cursors.Hand, Child = inner };
            cards[i] = card;

            void Refresh()
            {
                bool active = _flavor == key;
                card.BorderThickness = new Thickness(active ? 2 : 1);
                card.BorderBrush = Br(active ? fp.Lavender : fp.Surface1);
            }
            Refresh();
            card.MouseDown += (_, _) =>
            {
                _flavor = key;
                foreach (var c in cards) (c.Tag as Action)?.Invoke();
            };
            card.Tag = (Action)Refresh;
            grid.Children.Add(card);
        }
        return grid;
    }

    // -----------------------------------------------------------------------
    // Feature toggles
    // -----------------------------------------------------------------------

    UIElement BuildToggles()
    {
        var wrap = new WrapPanel();
        Toggle(wrap, "Show clock",         _clock,       v => _clock      = v);
        Toggle(wrap, "Aurora borealis",    _aurora,      v => _aurora     = v);
        Toggle(wrap, "Shooting stars",     _shooting,    v => _shooting   = v);
        Toggle(wrap, "Seafoam particles",  _foam,        v => _foam       = v);
        Toggle(wrap, "Bioluminescence",    _bio,         v => _bio        = v);
        Toggle(wrap, "Lighthouse & beam",  _lighthouse,  v => _lighthouse = v);
        Toggle(wrap, "Rain (Stormy only)", _rain,        v => _rain       = v);
        return wrap;
    }

    void Toggle(Panel parent, string label, bool initial, Action<bool> setter)
    {
        bool state = initial;
        var dot  = new Border { Width=12, Height=12, CornerRadius=new(6), Margin=new Thickness(0,0,8,0) };
        var text = TB(label, Ui.Text, 13);
        var row  = new StackPanel { Orientation=Orientation.Horizontal, VerticalAlignment=VerticalAlignment.Center };
        row.Children.Add(dot); row.Children.Add(text);

        var b = new Border { CornerRadius=new(6), Padding=new Thickness(10,7,10,7),
            Margin=new Thickness(4), Width=200, Cursor=Cursors.Hand, Child=row };

        void Refresh()
        {
            dot.Background   = Br(state ? Ui.Teal : Ui.Overlay0);
            text.Foreground  = Br(state ? Ui.Text : Ui.Subtext0);
            b.Background     = Br(state ? Ui.Surface0 : Ui.Base);
            b.BorderThickness = new Thickness(1);
            b.BorderBrush    = Br(state ? Ui.Teal : Ui.Surface1);
        }
        Refresh();
        b.MouseDown += (_, _) => { state = !state; setter(state); Refresh(); };
        parent.Children.Add(b);
    }

    // -----------------------------------------------------------------------
    // Clock section — position + format combined
    // -----------------------------------------------------------------------

    UIElement BuildClockSection()
    {
        var stack = new StackPanel { Margin = new Thickness(0, 0, 0, 4) };

        stack.Children.Add(TB("Format", Ui.Subtext0, 11));
        stack.Children.Add(ChoiceRow(["24h", "12h (AM/PM)"], _clockFmt,
            ["24h", "12h"], v => _clockFmt = v));

        stack.Children.Add(new Border { Height = 8 });
        stack.Children.Add(TB("Position", Ui.Subtext0, 11));
        stack.Children.Add(ChoiceRow(
            ["Top-left", "Top-right", "Bottom-left", "Bottom-right"], _clockPos,
            ["top-left", "top-right", "bottom-left", "bottom-right"],
            v => _clockPos = v));

        return stack;
    }

    // -----------------------------------------------------------------------
    // Generic choice row (labels + keys + setter)
    // -----------------------------------------------------------------------

    UIElement ChoiceRow(string[] labels, string selected, string[] keys, Action<string> setter)
    {
        var panels = new Border[labels.Length];
        var panel  = new UniformGrid { Columns = labels.Length };
        string cur = selected;

        for (int i = 0; i < labels.Length; i++)
        {
            var lbl = TB(labels[i], Ui.Subtext0, 13);
            lbl.HorizontalAlignment = HorizontalAlignment.Center;
            lbl.VerticalAlignment   = VerticalAlignment.Center;

            var b = new Border { CornerRadius=new(6), Padding=new Thickness(4,7,4,7),
                Margin=new Thickness(3), Height=36, Cursor=Cursors.Hand, Child=lbl };
            panels[i] = b;

            string key = keys[i];
            void RefreshAll()
            {
                for (int j = 0; j < panels.Length; j++)
                {
                    bool active = keys[j] == cur;
                    panels[j].Background      = Br(active ? Ui.Surface1 : Ui.Surface0);
                    panels[j].BorderThickness = new Thickness(active ? 2 : 1);
                    panels[j].BorderBrush     = Br(active ? Ui.Lavender : Ui.Surface2);
                    ((TextBlock)panels[j].Child).Foreground = Br(active ? Ui.Text : Ui.Subtext0);
                }
            }
            RefreshAll();

            b.MouseDown += (_, _) => { cur = key; setter(key); RefreshAll(); };
            panel.Children.Add(b);
        }
        return panel;
    }

    // -----------------------------------------------------------------------
    // Save button
    // -----------------------------------------------------------------------

    UIElement BuildSaveButton()
    {
        var lbl = TB("Save & Close", Ui.Base, 15, FontWeights.Medium);
        lbl.HorizontalAlignment = HorizontalAlignment.Center;
        lbl.VerticalAlignment   = VerticalAlignment.Center;

        var btn = new Border { Background=Br(Ui.Lavender), CornerRadius=new(8),
            Height=42, Width=200, Cursor=Cursors.Hand,
            Margin=new Thickness(16, 14, 16, 20), Child=lbl };

        btn.MouseEnter += (_, _) => btn.Background = Br(Ui.Blue);
        btn.MouseLeave += (_, _) => btn.Background = Br(Ui.Lavender);
        btn.MouseDown  += (_, _) =>
        {
            _s.Flavor        = _flavor;
            _s.TimeOfDay     = _tod;
            _s.ClockPos      = _clockPos;
            _s.ClockFormat   = _clockFmt;
            _s.StarDensity   = _starDensity;
            _s.WaveLayers    = _waveLayers;
            _s.CatSize       = _catSize;
            _s.WaveSpeed     = _waveSpeed;
            _s.ShowClock     = _clock;
            _s.ShowAurora    = _aurora;
            _s.ShowShooting  = _shooting;
            _s.ShowFoam      = _foam;
            _s.ShowBio       = _bio;
            _s.ShowLighthouse = _lighthouse;
            _s.ShowRain      = _rain;
            _s.Save();
            Close();
        };

        var wrap = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        wrap.Children.Add(btn);
        return wrap;
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    static TextBlock TB(string text, Color color, double size, FontWeight? weight = null) => new()
    {
        Text = text, Foreground = new SolidColorBrush(color),
        FontFamily = new FontFamily("Segoe UI"), FontSize = size,
        FontWeight = weight ?? FontWeights.Normal
    };

    static Border Divider() => new()
    { Height = 1, Background = Br(Palettes.All["mocha"].Surface0), Margin = new Thickness(0, 3, 0, 3) };

    static Border Section(string title, UIElement content)
    {
        var lbl = TB(title, Ui.Subtext0, 11, FontWeights.Medium);
        lbl.Margin = new Thickness(0, 0, 0, 8);
        var inner = new StackPanel { Margin = new Thickness(16, 10, 16, 4) };
        inner.Children.Add(lbl); inner.Children.Add(content);
        return new Border { Child = inner };
    }
}
