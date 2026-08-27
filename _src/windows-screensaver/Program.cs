using FormsScreen = System.Windows.Forms.Screen;
using WpfApplication = System.Windows.Application;

namespace GentleDrift.ScreenSaver;

internal enum ScreenSaverLaunchMode
{
    Configure,
    ScreenSaver,
    Preview,
}

internal readonly record struct ScreenSaverLaunch(ScreenSaverLaunchMode Mode, IntPtr PreviewParent)
{
    public static ScreenSaverLaunch Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return new(ScreenSaverLaunchMode.Configure, IntPtr.Zero);
        }

        var command = args[0].Trim();
        var option = command.Length >= 2 ? command[..2] : command;

        if (option.Equals("/s", StringComparison.OrdinalIgnoreCase))
        {
            return new(ScreenSaverLaunchMode.ScreenSaver, IntPtr.Zero);
        }

        if (option.Equals("/c", StringComparison.OrdinalIgnoreCase))
        {
            return new(ScreenSaverLaunchMode.Configure, IntPtr.Zero);
        }

        if (option.Equals("/p", StringComparison.OrdinalIgnoreCase))
        {
            var handleText = command.Length > 2 ? command[2..].Trim().TrimStart(':') : args.ElementAtOrDefault(1);
            return long.TryParse(handleText, out var handle) && handle != 0
                ? new(ScreenSaverLaunchMode.Preview, new IntPtr(handle))
                : new(ScreenSaverLaunchMode.Configure, IntPtr.Zero);
        }

        return new(ScreenSaverLaunchMode.Configure, IntPtr.Zero);
    }
}

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var app = new WpfApplication { ShutdownMode = System.Windows.ShutdownMode.OnLastWindowClose };
        var launch = ScreenSaverLaunch.Parse(args);

        if (launch.Mode == ScreenSaverLaunchMode.Configure)
        {
            app.Run(new ConfigurationWindow());
            return;
        }

        if (launch.Mode == ScreenSaverLaunchMode.Preview)
        {
            app.Run(new ScreenSaverWindow(launch.PreviewParent));
            return;
        }

        foreach (var screen in FormsScreen.AllScreens)
        {
            new ScreenSaverWindow(screen.Bounds).Show();
        }

        app.Run();
    }
}
