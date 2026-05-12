using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace CatppuccinCoast;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // Kill any previous screensaver instance that didn't exit cleanly.
        var self = Process.GetCurrentProcess();
        foreach (var p in Process.GetProcessesByName(self.ProcessName))
        {
            if (p.Id == self.Id) continue;
            try { p.Kill(); p.WaitForExit(500); } catch { }
        }

        var app  = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        string mode = args.Length > 0 ? args[0].ToLowerInvariant() : "/s";

        if (mode == "/c")
        {
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;
            var w = new SettingsWindow();
            app.MainWindow = w;
            w.Show();
            app.Run();
        }
        else if (mode == "/p" && args.Length > 1 && long.TryParse(args[1], out long hwndVal))
        {
            var previewHwnd = new IntPtr(hwndVal);
            NativeMethods.GetClientRect(previewHwnd, out var rect);
            int pw = rect.Right  > 0 ? rect.Right  : 320;
            int ph = rect.Bottom > 0 ? rect.Bottom : 240;

            var scene = new CoastScene(AppSettings.Load(), pw, ph);
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
