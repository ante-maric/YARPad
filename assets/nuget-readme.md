[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

# YARPad

**🚀 A full-featured admin console for [YARP (Yet Another Reverse Proxy)](https://github.com/dotnet/yarp) — manage routes, clusters, transforms, and policies visually at runtime.**

Add `YARPad` to your ASP.NET Core + YARP application and get a browser-based admin UI to edit YARP configuration without code or restarts.

---

## ✨ Key Features

- **📊 Visual Configuration Editors** — Routes, clusters, destinations, transforms, metadata, and policies through the browser
- **ⓘ Contextual Help Everywhere** — Inline explanations for every non-trivial field (what it does, valid values, behavioral impact)
- **🔄 Configuration Profiles** — Create, clone, export, import, and activate profiles safely; easy rollbacks
- **✅ Built-in Validation** — Catch configuration errors before they go live
- **🔐 Secure by Default** — ASP.NET Core Identity with passkeys, 2FA, and LAN-only access guard
- **⚡ Live Updates** — Push new configurations to YARP without restarting
- **🎨 Themeable UI** — MudBlazor-based interface with persisted user preferences

---

## 📋 Requirements

- **.NET 10** / ASP.NET Core 10
- An ASP.NET Core app using **`Yarp.ReverseProxy`**

---

## ⚡ Quick Start (3 Steps)

### 1. Install the package

```bash
dotnet add package CodingCell.YARPad
```

### 2. Register in `Program.cs`

```csharp
using CodingCell.YARPad;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddYARPad(builder.Configuration);
builder.Services.AddReverseProxy();

var app = builder.Build();

app.MapYARPad();
app.MapReverseProxy();

app.Run();
```

### 3. Add minimal config to `appsettings.json`

```json
{
  "YARPad": {
    "PathPrefix": "/yarpad",
    "ConnectionString": "DataSource=yarpad.db"
  }
}
```

### Done! 🎉

- Browse to `https://localhost:<port>/yarpad`
- Register your first admin account
- Start managing your proxy configuration

---

## 🔗 Links

- **GitHub:** https://github.com/ante-maric/YARPad
- **Full Documentation:** See docs/ for getting started, security, and advanced scenarios
- **YARP Project:** https://github.com/dotnet/yarp
