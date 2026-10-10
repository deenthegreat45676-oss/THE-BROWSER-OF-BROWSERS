# THE-BROWSER-OF-BROWSERS

A small Windows desktop browser built with .NET 10 WinForms and Microsoft
WebView2. WebView2 provides the Chromium-based rendering and networking engine;
this project is a browser shell, not a new browser engine like Chromium or Gecko.

## Build and publish

Install the .NET 10 SDK, then run on Windows:

```powershell
dotnet publish .\BrowserOfBrowsers.csproj -c Release -r win-x64 --self-contained true
```

The publish directory contains the executable and its .NET dependencies. The
Microsoft Edge WebView2 Evergreen Runtime must also be installed on the target
machine. It is a separate browser runtime and is not bundled into this app.

The browser currently includes address/search navigation, back, forward, and
reload controls. Network access follows the machine's normal network policy;
there is no proxy or network-restriction bypass feature.