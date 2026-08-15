using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace EmailLock;

/// <summary>
/// A borderless window whose entire surface is a WebView2. Both app screens are HTML,
/// so the visual design lives in ui/*.html rather than in hand-placed WinForms controls.
/// </summary>
public class WebWindow : Form
{
    readonly WebView2 _web = new() { Dock = DockStyle.Fill };
    readonly string _page;
    readonly object _data;

    public event Action<JsonElement>? MessageReceived;

    public WebWindow(string page, object data)
    {
        _page = page;
        _data = data;

        FormBorderStyle = FormBorderStyle.None;
        BackColor = Color.FromArgb(0x0a, 0x09, 0x08);
        KeyPreview = true;
        Controls.Add(_web);
    }

    /// <summary>Boots WebView2 and loads the page. Await before Show() so nothing flashes white.</summary>
    public async Task PrepareAsync()
    {
        var dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EmailLock", "WebView2");
        Directory.CreateDirectory(dataDir);

        var env = await CoreWebView2Environment.CreateAsync(null, dataDir);
        await _web.EnsureCoreWebView2Async(env);

        var s = _web.CoreWebView2.Settings;
        s.AreDefaultContextMenusEnabled = false;
        s.AreDevToolsEnabled = false;
        s.IsStatusBarEnabled = false;
        s.AreBrowserAcceleratorKeysEnabled = false;
        s.IsZoomControlEnabled = false;
        s.IsSwipeNavigationEnabled = false;
        _web.DefaultBackgroundColor = Color.FromArgb(0x0a, 0x09, 0x08);

        // window.APP has to exist before the page's own script runs.
        await _web.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
            "window.APP = " + JsonSerializer.Serialize(_data, JsonOpts) + ";");

        _web.CoreWebView2.WebMessageReceived += (_, e) =>
            MessageReceived?.Invoke(JsonDocument.Parse(e.WebMessageAsJson).RootElement);

        _web.CoreWebView2.NavigateToString(Read(_page).Replace("/*CSS*/", Read("app.css")));
    }

    public static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Task Call(string js) => _web.CoreWebView2 is null ? Task.CompletedTask : _web.ExecuteScriptAsync(js);

    static string Read(string name)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream($"EmailLock.ui.{name}")
            ?? throw new FileNotFoundException($"embedded ui resource missing: {name}");
        return new StreamReader(stream).ReadToEnd();
    }

    // --- window shaping ---------------------------------------------------

    public void FillPrimaryScreen()
    {
        StartPosition = FormStartPosition.Manual;
        Bounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1280, 800);
        TopMost = true;
        ShowInTaskbar = false;
    }

    public void Panel(int width, int height)
    {
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(width, height);
        ShowInTaskbar = true;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        // Windows 11 rounded corners -- a borderless form is a sharp rectangle without this.
        var round = DWMWCP_ROUND;
        DwmSetWindowAttribute(Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref round, sizeof(int));
    }

    /// <summary>Lets an HTML element act as the titlebar.</summary>
    public void BeginDrag()
    {
        ReleaseCapture();
        SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
    }

    const int DWMWA_WINDOW_CORNER_PREFERENCE = 33, DWMWCP_ROUND = 2;
    const int WM_NCLBUTTONDOWN = 0x00A1, HTCAPTION = 2;

    [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr h, int attr, ref int val, int size);
    [DllImport("user32.dll")] static extern bool ReleaseCapture();
    [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr h, int msg, int wp, int lp);
}
