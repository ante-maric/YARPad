# 🧭 Getting Started

## 📋 Requirements

- **.NET 10** / ASP.NET Core 10
- A host application that references **`Yarp.ReverseProxy`**

> **Note:** YARPad is designed to augment an existing YARP-enabled application. If you're starting fresh, make sure your project includes the YARP reverse proxy package.

---

## 📦 Installation

Add the `CodingCell.YARPad` package to your ASP.NET Core project:

```bash
dotnet add package CodingCell.YARPad
```

---

## 🚀 Quick Start

### 1. Register services

In `Program.cs`, register YARP and YARPad:

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

### 2. Configure options

Add configuration to `appsettings.json`:

```json
{
  "YARPad": {
    "PathPrefix": "/yarpad",
    "ConnectionString": "DataSource=yarpad.db",
    "InstanceID": null,
    "MultiUserEnabled": false,
    "IsLanOnlyAccessDisabled": false,
    "LanAccess": {
      "TrustedProxies": [],
      "TrustedNetworks": [],
      "AdditionalAllowedRanges": [],
      "IncludeDefaultPrivateRanges": true,
      "AllowLoopback": true,
      "ForwardLimit": 1
    }
  }
}
```

### 3. Run and access

1. Start the application with `dotnet run`
2. Browse to `https://localhost:<port>/yarpad`
3. On first run, database migrations execute automatically
4. **Register the first admin account** (subsequent registrations are blocked by default)
5. Start managing your YARP configuration visually

---

## First-Time Setup

On first startup, YARPad will:

1. ✅ Create the SQLite database if it does not exist
2. ✅ Apply Entity Framework Core migrations
3. ✅ Initialize tables for Identity and YARPad configuration data

After that:

1. Navigate to `/yarpad/Account/Register`
2. Create the first admin account
3. Optionally set up passkeys, 2FA, and recovery codes
4. Create a configuration profile and activate it

> **⚠️ Important:** By default, only the first user can register. Set `MultiUserEnabled = true` if you want more than one admin account, but make sure you set it to false once you create all accounts.

---

**Next:** [Configuration Reference](configuration.md)
