using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace GentleDrift.ScreenSaver;

internal sealed class ConfigurationWindow : Window
{
    private readonly WebView2 _webView = new();

    public ConfigurationWindow()
    {
        Title = "Gentle Drift Settings";
        Width = 1120;
        Height = 760;
        MinWidth = 680;
        MinHeight = 480;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = System.Windows.Media.Brushes.Black;
        Content = _webView;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await ScreenSaverWebView.LoadAsync(_webView, screenSaverMode: false);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            MessageBox.Show(
                "Gentle Drift needs the Microsoft Edge WebView2 Runtime. Install it, then open this screen saver again.",
                "Gentle Drift",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Close();
        }
    }
}

internal sealed class ScreenSaverWindow : Window
{
    private readonly WebView2 _webView = new();
    private readonly bool _isPreview;
    private readonly IntPtr _previewParent;
    private readonly DispatcherTimer _cursorTimer;
    private readonly Stopwatch _inputGracePeriod = new();
    private NativeMethods.POINT _startingCursor;

    public ScreenSaverWindow(Rectangle bounds)
    {
        _isPreview = false;
        _cursorTimer = CreateCursorTimer();
        ConfigureWindow();
        Left = bounds.Left;
        Top = bounds.Top;
        Width = bounds.Width;
        Height = bounds.Height;
    }

    public ScreenSaverWindow(IntPtr previewParent)
    {
        _isPreview = true;
        _previewParent = previewParent;
        _cursorTimer = CreateCursorTimer();
        ConfigureWindow();
        Topmost = false;
        ShowInTaskbar = false;
        Width = 320;
        Height = 240;
        SourceInitialized += AttachToPreviewWindow;
    }

    private void ConfigureWindow()
    {
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        Background = System.Windows.Media.Brushes.Black;
        Content = _webView;
        Loaded += OnLoaded;
    }

    private DispatcherTimer CreateCursorTimer()
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(80) };
        timer.Tick += (_, _) => ExitWhenInputMoves();
        return timer;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await ScreenSaverWebView.LoadAsync(_webView, screenSaverMode: !_isPreview);
            _webView.CoreWebView2.AcceleratorKeyPressed += (_, _) => ExitScreenSaver();
        }
        catch (WebView2RuntimeNotFoundException)
        {
            ExitScreenSaver();
            return;
        }

        if (_isPreview)
        {
            return;
        }

        _startingCursor = NativeMethods.GetCursorPosition();
        _inputGracePeriod.Start();
        _cursorTimer.Start();
        PreviewKeyDown += (_, _) => ExitScreenSaver();
    }

    private void ExitWhenInputMoves()
    {
        if (_inputGracePeriod.Elapsed < TimeSpan.FromMilliseconds(500))
        {
            return;
        }

        var currentCursor = NativeMethods.GetCursorPosition();
        if (Math.Abs(currentCursor.X - _startingCursor.X) > 5 || Math.Abs(currentCursor.Y - _startingCursor.Y) > 5)
        {
            ExitScreenSaver();
        }
    }

    private void ExitScreenSaver()
    {
        if (_isPreview)
        {
            return;
        }

        _cursorTimer.Stop();
        Application.Current.Shutdown();
    }

    private void AttachToPreviewWindow(object? sender, EventArgs e)
    {
        if (_previewParent == IntPtr.Zero)
        {
            return;
        }

        var windowHandle = new WindowInteropHelper(this).Handle;
        var currentStyle = NativeMethods.GetWindowLongPtr(windowHandle, NativeMethods.GwlStyle).ToInt64();
        NativeMethods.SetWindowLongPtr(
            windowHandle,
            NativeMethods.GwlStyle,
            new IntPtr((currentStyle & ~NativeMethods.WsPopup) | NativeMethods.WsChild));
        NativeMethods.SetParent(windowHandle, _previewParent);

        if (NativeMethods.GetClientRect(_previewParent, out var rect))
        {
            NativeMethods.SetWindowPos(
                windowHandle,
                IntPtr.Zero,
                0,
                0,
                rect.Right - rect.Left,
                rect.Bottom - rect.Top,
                NativeMethods.SwpNoZOrder | NativeMethods.SwpShowWindow);
        }
    }
}

internal static class ScreenSaverWebView
{
    public static async Task LoadAsync(WebView2 webView, bool screenSaverMode)
    {
        var userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GentleDrift",
            "WebView2");
        var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
        await webView.EnsureCoreWebView2Async(environment);

        var settings = webView.CoreWebView2.Settings;
        settings.AreDefaultContextMenusEnabled = false;
        settings.AreDevToolsEnabled = false;
        settings.AreBrowserAcceleratorKeysEnabled = false;

        var pagePath = Path.Combine(AppContext.BaseDirectory, "Web", "index.html");
        var pageUri = new Uri(pagePath).AbsoluteUri;
        webView.CoreWebView2.Navigate(screenSaverMode ? $"{pageUri}?screensaver=1" : pageUri);
    }
}

internal static class NativeMethods
{
    internal const int GwlStyle = -16;
    internal const long WsChild = 0x40000000L;
    internal const long WsPopup = unchecked((long)0x80000000);
    internal const uint SwpNoZOrder = 0x0004;
    internal const uint SwpShowWindow = 0x0040;

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT point);

    [DllImport("user32.dll")]
    internal static extern IntPtr SetParent(IntPtr childWindow, IntPtr newParent);

    [DllImport("user32.dll")]
    internal static extern bool GetClientRect(IntPtr window, out RECT rect);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern IntPtr SetWindowLong32(IntPtr window, int index, IntPtr value);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr window, int index, IntPtr value);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern IntPtr GetWindowLong32(IntPtr window, int index);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr window, int index);

    [DllImport("user32.dll")]
    internal static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

    internal static POINT GetCursorPosition()
    {
        return GetCursorPos(out var point) ? point : default;
    }

    internal static IntPtr SetWindowLongPtr(IntPtr window, int index, IntPtr value)
    {
        return IntPtr.Size == 8 ? SetWindowLongPtr64(window, index, value) : SetWindowLong32(window, index, value);
    }

    internal static IntPtr GetWindowLongPtr(IntPtr window, int index)
    {
        return IntPtr.Size == 8 ? GetWindowLongPtr64(window, index) : GetWindowLong32(window, index);
    }
}
