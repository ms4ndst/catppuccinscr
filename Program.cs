using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace CatppuccinCoast;

static class Program
{
    const string SettingsMutex = "CatppuccinCoast_Settings";

    [STAThread]
    static void Main(string[] args)
    {
        string mode = args.Length > 0 ? args[0].ToLowerInvariant() : "/s";

        // Kill previous screensaver instances, but never kill a /c settings window.
        // A named mutex protects the settings process from being killed.
        var self = Process.GetCurrentProcess();
        foreach (var p in Process.GetProcessesByName(self.ProcessName))
        {
            if (p.Id == self.Id) continue;
            // Skip if the other process holds the settings mutex
            bool createdNew;
            using var probe = new Mutex(false, SettingsMutex, out createdNew);
            if (!createdNew) { /* settings window is open — leave it alone */ continue; }
            try { p.Kill(); p.WaitForExit(500); } catch { }
        }

        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };

        if (mode.StartsWith("/c"))
        {
            // Hold a named mutex so other instances won't kill us.
            using var mtx = new Mutex(true, SettingsMutex);
            try
            {
                app.ShutdownMode = ShutdownMode.OnMainWindowClose;
                var w = new SettingsWindow();
                w.Topmost = true; // ensure it appears above the screensaver picker
                app.MainWindow = w;
                w.Show();
                w.Activate();
                app.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Catppuccin Coast — Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else if (mode.StartsWith("/p") && args.Length > 1 && long.TryParse(args[1], out long hwndVal))
        {
            var previewHwnd = new IntPtr(hwndVal);
            NativeMethods.GetClientRect(previewHwnd, out var rect);
            int pw = rect.Right  > 0 ? rect.Right  : 320;
            int ph = rect.Bottom > 0 ? rect.Bottom : 240;

            var settings = AppSettings.Load();
            var scene = SceneFactory.Create(settings, pw, ph);
            var host  = new SceneHost(scene, pw, ph);

            var src = new HwndSource(new HwndSourceParameters("CatppuccinCoastPreview")
            {
                ParentWindow = previewHwnd,
                WindowStyle  = 0x50000000,
                Width = pw, Height = ph
            }) { RootVisual = host };

            app.Run();
            src.Dispose();
        }
        else
        {
            var w = new ScreensaverWindow();
            app.MainWindow = w;
            w.Show();
            app.Run();
        }
    }
}
