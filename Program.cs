using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace BrowserOfBrowsers;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new BrowserWindow());
    }
}

internal sealed class BrowserWindow : Form
{
    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };
    private readonly TextBox _addressBar = new() { Dock = DockStyle.Fill };
    private readonly Button _backButton = new() { Text = "Back", AutoSize = true };
    private readonly Button _forwardButton = new() { Text = "Forward", AutoSize = true };
    private readonly Button _reloadButton = new() { Text = "Reload", AutoSize = true };
    private readonly Button _goButton = new() { Text = "Go", AutoSize = true };

    public BrowserWindow()
    {
        Text = "Browser of Browsers";
        Width = 1200;
        Height = 800;
        MinimumSize = new Size(640, 400);

        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 42,
            ColumnCount = 5,
            Padding = new Padding(6),
        };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        toolbar.Controls.Add(_backButton, 0, 0);
        toolbar.Controls.Add(_forwardButton, 1, 0);
        toolbar.Controls.Add(_reloadButton, 2, 0);
        toolbar.Controls.Add(_addressBar, 3, 0);
        toolbar.Controls.Add(_goButton, 4, 0);

        Controls.Add(_webView);
        Controls.Add(toolbar);

        _backButton.Click += (_, _) =>
        {
            if (_webView.CoreWebView2?.CanGoBack == true)
                _webView.CoreWebView2.GoBack();
        };
        _forwardButton.Click += (_, _) =>
        {
            if (_webView.CoreWebView2?.CanGoForward == true)
                _webView.CoreWebView2.GoForward();
        };
        _reloadButton.Click += (_, _) => _webView.CoreWebView2?.Reload();
        _goButton.Click += (_, _) => NavigateFromAddressBar();
        _addressBar.KeyDown += (_, args) =>
        {
            if (args.KeyCode == Keys.Enter)
            {
                NavigateFromAddressBar();
                args.SuppressKeyPress = true;
            }
        };

        Shown += InitializeBrowserAsync;
    }

    private async void InitializeBrowserAsync(object? sender, EventArgs args)
    {
        try
        {
            await _webView.EnsureCoreWebView2Async();
            _webView.CoreWebView2.SourceChanged += (_, _) =>
                _addressBar.Text = _webView.CoreWebView2.Source;
            _webView.CoreWebView2.NavigationCompleted += (_, _) => UpdateNavigationButtons();
            _webView.CoreWebView2.NewWindowRequested += (_, request) =>
            {
                request.Handled = true;
                _webView.CoreWebView2.Navigate(request.Uri);
            };
            _webView.CoreWebView2.Navigate("https://www.example.com");
        }
        catch (WebView2RuntimeNotFoundException)
        {
            MessageBox.Show(
                this,
                "Microsoft Edge WebView2 Runtime is required. Install the Evergreen WebView2 Runtime, then restart this browser.",
                "WebView2 Runtime Missing",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Browser startup failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void NavigateFromAddressBar()
    {
        var target = ResolveAddress(_addressBar.Text);
        if (_webView.CoreWebView2 is not null)
            _webView.CoreWebView2.Navigate(target);
    }

    private static string ResolveAddress(string input)
    {
        var value = input.Trim();
        if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUri) &&
            (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
            return absoluteUri.ToString();

        if (!value.Contains(' ') && Uri.TryCreate($"https://{value}", UriKind.Absolute, out var hostUri))
            return hostUri.ToString();

        return $"https://duckduckgo.com/?q={Uri.EscapeDataString(value)}";
    }

    private void UpdateNavigationButtons()
    {
        _backButton.Enabled = _webView.CoreWebView2.CanGoBack;
        _forwardButton.Enabled = _webView.CoreWebView2.CanGoForward;
    }
}