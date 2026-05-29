# 🐳 YARPad Proxy

## What is YARPad Proxy?

**YARPad Proxy** is a turnkey Docker appliance that bundles [YARP (Yet Another Reverse Proxy)](https://github.com/dotnet/yarp) with the [YARPad](../library/getting-started.md) browser-based management UI into a single, ready-to-run container. It is designed for teams that need a reverse proxy with visual configuration management but don't want to build and maintain a custom ASP.NET Core host application.

Where the **YARPad library** is a NuGet package you integrate into your own app, **YARPad Proxy** is the app — you pull the Docker image, configure it with environment variables, and run it.

---

## Key Features

| Feature                       | Description                                                                                                        |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| **Zero-code deployment**      | No custom ASP.NET Core project needed — just `docker run` or `docker compose up`                                   |
| **Full YARPad UI**            | All visual editing, profiles, validation, and Identity features from the YARPad library                            |
| **Let's Encrypt support**     | Automatic TLS certificate issuance and renewal via the bundled [go-acme/lego](https://github.com/go-acme/lego) CLI |
| **Persistent storage**        | SQLite database and Let's Encrypt certificates stored on a Docker volume                                           |
| **Non-root container**        | Runs as a dedicated non-root user (UID 1654) for improved security                                                 |
| **Structured logging**        | Built-in [Serilog](https://serilog.net/) with console sink, configurable via environment variables                 |
| **Production-ready defaults** | LAN-only access enabled, single-user mode, HTTPS-ready                                                             |

---

## Architecture

```
┌──────────────────────────────────────────┐
│            YARPad Proxy Container        │
│                                          │
│  ┌──────────────┐   ┌─────────────────┐  │
│  │  Kestrel     │   │    YARPad UI    │  │
│  │  :8080 HTTP  │   │    /yarpad      │  │
│  │  :8081 HTTPS │   │                 │  │
│  └─────┬────┘───└───└─────┬───────────┘  │
│        │                  │              │
│  ┌─────▼──────────────────▼───────────┐  │
│  │         YARP Reverse Proxy         │  │
│  │   routes · clusters · transforms   │  │
│  └─────────────────┬──────────────────┘  │
│                    │                     │
│  ┌─────────────────▼──────────────────┐  │
│  │        SQLite (yarpad.db)          │  │
│  │   config · profiles · identity     │  │
│  └────────────────────────────────────┘  │
│                                          │
│  ┌────────────────────────────────────┐  │
│  │    lego (optional)                 │  │
│  │    auto TLS via Let's Encrypt      │  │
│  └────────────────────────────────────┘  │
└──────────────────────────────────────────┘
         │
         ▼  /data volume
   ┌───────────────┐
   │  yarpad.db    │
   │  LetsEncrypt/ │
   └───────────────┘
```

### Ports

The following table shows the default ports. Both can be customized via Kestrel endpoint configuration — see [Listening Addresses and Ports](docker-deployment.md#listening-addresses-and-ports) for details.

| Port   | Protocol | Purpose                                                                         |
| ------ | -------- | ------------------------------------------------------------------------------- |
| `8080` | HTTP     | Main traffic; also serves ACME HTTP-01 challenges when Let's Encrypt is enabled |
| `8081` | HTTPS    | TLS-terminated traffic                                                          |

### Data Volume

All persistent state is stored under `/data` inside the container:

- `yarpad.db` — SQLite database (YARP configuration, profiles, Identity users)
- `LetsEncrypt/` — Certificate data (when Let's Encrypt is enabled)

---

## When to Use YARPad Proxy vs. the Library

| Scenario                                                                       | Recommended                                     |
| ------------------------------------------------------------------------------ | ----------------------------------------------- |
| You already have an ASP.NET Core app with YARP and want to add a management UI | [YARPad Library](../library/getting-started.md) |
| You need a standalone reverse proxy with browser-based management              | **YARPad Proxy**                                |
| You want to run YARP in Docker/Kubernetes with zero custom code                | **YARPad Proxy**                                |
| You need custom middleware, transforms, or DI in your host app                 | [YARPad Library](../library/getting-started.md) |

---

**Next:** [Docker Deployment](docker-deployment.md)
