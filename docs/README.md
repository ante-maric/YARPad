<div align="center">
  <h1>YARPad</h1>
  <p><strong>A full-featured admin console for <a href="https://github.com/dotnet/yarp">YARP (Yet Another Reverse Proxy)</a>.</strong></p>
  
  [![NuGet](https://img.shields.io/nuget/v/CodingCell.YARPad.svg)](https://www.nuget.org/packages/CodingCell.YARPad/)
  ![Blazor Server](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor&logoColor=white)
  [![License](https://img.shields.io/badge/license-Apache--2.0-green.svg)](LICENSE)
</div>

## ✨ What is YARPad?

> **🎯 A full-featured admin console for [YARP (Yet Another Reverse Proxy)](https://github.com/dotnet/yarp) — manage your reverse proxy configuration visually at runtime.**

YARPad adds a Blazor Server management UI to an ASP.NET Core app so you can edit routes, clusters, transforms, metadata, and policies through the browser, validate changes before activation, and keep your live YARP configuration under control.

One of the defining qualities of YARPad is its **pervasive, built-in contextual help**. YARP has a rich configuration surface — load-balancing policies, session-affinity modes, active/passive health checks, transform types, forwarding headers, and much more — and not everyone has all of it memorised. Throughout the UI, fields and options marked with **ⓘ** show inline explanations of what the setting does, what values are valid, and how it affects YARP's behaviour.

This repository contains two main products:

| Product                                             | Description                                                                                                    |
| --------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| [**📦 YARPad Library**](library/getting-started.md) | A Blazor Server NuGet package that adds a management UI to any ASP.NET Core YARP application                   |
| [**🐳 YARPad Proxy**](proxy/overview.md)            | A turnkey Docker appliance that bundles YARP + YARPad into a ready-to-run container with Let's Encrypt support |

---

## 🖼️ Screenshots

![Routes list showing named routes with HTTP methods, hosts, and cluster mappings](../assets/screenshots/screenshot-1.jpg)

[View all screenshots](screenshots.md)

---

## ✨ Features

- **📊 Visual Editors** — Manage routes, clusters, destinations, transforms, metadata, and policies through the browser
- **ⓘ Contextual Help Everywhere** — Inline help tooltips on every non-trivial field
- **🔄 Configuration Profiles** — Create, clone, export, import, and activate profiles for safer change management
- **✅ Built-in Validation** — FluentValidation and YARP validation catch bad configuration before it goes live
- **🔐 Secure by Default** — ASP.NET Core Identity with login/register, passkeys, 2FA, and recovery codes
- **🌐 LAN-Only Access Guard** — Restrict access to local networks by default
- **🎨 Themeable UI** — MudBlazor-based interface with persisted user theme preferences
- **⚡ Live Configuration Activation** — Push configurations to YARP without restarting the app
- **🧩 Extensible Host Integration** — Bring your own transforms, policies, and host-specific configuration

---

## 📚 YARPad Library

Add `YARPad` to your existing ASP.NET Core + YARP application and get a browser-based admin console for routes, clusters, transforms, and policies.

- [🧭 Getting Started](library/getting-started.md) — Requirements, installation, quick start
- [⚙️ Configuration Reference](library/configuration.md) — YARPadOptions, LAN access, Identity settings
- [💼 Working with YARPad](library/working-with-yarpad.md) — Profiles, routes, clusters, transforms, policies
- [🔐 Security](library/security.md) — LAN-only access, best practices, reverse proxy hosting
- [🛠️ Advanced Scenarios](library/advanced.md) — Custom paths, programmatic config, troubleshooting, FAQ

## 🐳 YARPad Proxy

Run YARP as a standalone Docker container with browser-based management — no custom code required.

- [🧭 Overview](proxy/overview.md) — What it is, features, architecture
- [🐳 Docker Deployment](proxy/docker-deployment.md) — docker-compose, environment variables, volumes, Let's Encrypt

---

## 🗺️ Roadmap

Current direction for the core package:

- get feedback from community
- continued YARP compatibility improvements

---

## 🤝 Contributing

Contributions are currently disabled while the project stabilizes.

---

## 📝 License

This project is licensed under the Apache License 2.0. See the [LICENSE](../LICENSE) file.

---

**Made with ❤️ for the YARP community**
