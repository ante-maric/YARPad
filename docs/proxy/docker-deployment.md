# 🐳 Docker Deployment

## Quick Start with Docker Compose

Images and available version tags are published in the [YARPad Proxy GitHub Container Registry](https://github.com/ante-maric/YARPad/pkgs/container/yarpad-proxy).

Create a `docker-compose.yml`:

```yaml
services:
  yarpad-init:
    image: busybox:1.36
    # Ensures the persistent volume is owned by the non-root user (UID/GID 1654)
    # that the yarpad container runs as, with restricted permissions.
    command: sh -c "chown -R 1654:1654 /data && chmod 750 /data"
    volumes:
      - yarpad_data:/data
    restart: "no"

  yarpad:
    image: ghcr.io/ante-maric/yarpad-proxy:0.16.0
    restart: unless-stopped
    depends_on:
      yarpad-init:
        condition: service_completed_successfully
    volumes:
      - yarpad_data:/data
    ports:
      - 80:8080
      # Uncomment when enabling Let's Encrypt:
      #- 443:8081
    environment:
      - YARPad__ConnectionString=DataSource=/data/yarpad.db

volumes:
  yarpad_data:
```

Start with:

```bash
docker compose up -d
```

Then browse to `http://<host>/yarpad` and register your first admin account.

---

## Init Container

The `yarpad-init` service is a one-shot container that sets correct ownership on the `/data` volume before the main container starts. YARPad Proxy runs as a non-root user (UID/GID **1654**), so the volume must be writable by that user:

```yaml
yarpad-init:
  image: busybox:1.36
  command: sh -c "chown -R 1654:1654 /data && chmod 750 /data"
  volumes:
    - yarpad_data:/data
  restart: "no"
```

---

## Environment Variables

> **ℹ️ ASP.NET Core uses `__` (double underscore) as the hierarchy separator for environment-variable-based configuration.** Every dot or colon in a configuration key becomes `__` when expressed as an environment variable. For example, the key `YARPad:LanAccess:ForwardLimit` becomes `YARPad__LanAccess__ForwardLimit`, and `Kestrel:Endpoints:Https:Url` becomes `Kestrel__Endpoints__Https__Url`. This convention applies to all configuration sections — `YARPad__`, `YARPadProxy__`, `Kestrel__`, `Serilog__`, and others.

All YARPad configuration can be set through environment variables using the `YARPad__` prefix (double underscore for nested keys).

### Database

| Variable                   | Default             | Description                                                                                |
| -------------------------- | ------------------- | ------------------------------------------------------------------------------------------ |
| `YARPad__ConnectionString` | `DataSource=app.db` | SQLite connection string. The file path must reside inside the persistent volume (`/data`) |

### General

| Variable             | Default            | Description                                   |
| -------------------- | ------------------ | --------------------------------------------- |
| `YARPad__PathPrefix` | `/yarpad`          | Base path under which the YARPad UI is served |
| `YARPad__InstanceID` | Container hostname | Human-readable identifier shown in the UI     |

### Multi-User Mode

| Variable                   | Default | Description                                                                                                |
| -------------------------- | ------- | ---------------------------------------------------------------------------------------------------------- |
| `YARPad__MultiUserEnabled` | `false` | Set to `true` to allow multiple user registrations. When `false`, registration closes after the first user |

### LAN-Only Access

| Variable                                         | Default | Description                                                                                                                                                                                  |
| ------------------------------------------------ | ------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `YARPad__IsLanOnlyAccessDisabled`                | `false` | Set to `true` to disable LAN-only restriction (not recommended for production)                                                                                                               |
| `YARPad__LanAccess__IncludeDefaultPrivateRanges` | `true`  | Include default private IPv4 ranges (10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16) and IPv6 ULA/link-local                                                                                      |
| `YARPad__LanAccess__AllowLoopback`               | `true`  | Allow loopback addresses (127.0.0.1, ::1)                                                                                                                                                    |
| `YARPad__LanAccess__AdditionalAllowedRanges__0`  | —       | Extra CIDR ranges allowed to access YARPad beyond the default private ranges. Supports IPv4 and IPv6. Use `__0`, `__1`, etc. for multiple entries. e.g. `203.0.113.0/24` or `2a02:1234::/48` |
| `YARPad__LanAccess__TrustedProxies__0`           | —       | Trusted proxy IP addresses — X-Forwarded-For headers are only trusted from these IPs. Use `__0`, `__1`, etc. for multiple entries. e.g. `10.0.0.1`                                           |
| `YARPad__LanAccess__TrustedNetworks__0`          | —       | Trusted proxy networks in CIDR notation. Use instead of (or in addition to) `TrustedProxies__N` for IP ranges. Use `__0`, `__1`, etc. for multiple entries. e.g. `10.0.0.0/8`                |
| `YARPad__LanAccess__ForwardLimit`                | `1`     | Maximum X-Forwarded-For hops to trust                                                                                                                                                        |

### Logging

| Variable                         | Default       | Description                                                                               |
| -------------------------------- | ------------- | ----------------------------------------------------------------------------------------- |
| `Serilog__MinimumLevel__Default` | `Information` | Minimum log level. Values: `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal` |

---

## Listening Addresses and Ports

YARPad Proxy uses [Kestrel](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints), the ASP.NET Core web server. Which addresses and ports the container listens on depends on whether [Let's Encrypt](#lets-encrypt-automatic-https) is enabled and whether Kestrel endpoint configuration is provided.

### Without Let's Encrypt

| Kestrel endpoint configuration                | Listening addresses                                                                                                                       |
| --------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Not provided                                  | `http://+:8080` — the [.NET 8+ container default](https://learn.microsoft.com/en-us/dotnet/core/compatibility/containers/8.0/aspnet-port) |
| Provided via `Kestrel__Endpoints__*` env vars | As configured                                                                                                                             |

When no endpoint configuration is supplied, ASP.NET Core's .NET 8+ base image sets `ASPNETCORE_HTTP_PORTS=8080`, so the app listens on all interfaces on port 8080. You can override this with standard ASP.NET Core environment variables:

```yaml
# Override using the simple ports variable
environment:
  - ASPNETCORE_HTTP_PORTS=9000

# Or override using the full URL variable
environment:
  - ASPNETCORE_URLS=http://+:9000

# Or configure named Kestrel endpoints directly
environment:
  - Kestrel__Endpoints__Http__Url=http://+:9000
```

### With Let's Encrypt

When `YARPadProxy__IsLetsEncryptEnabled=true`, the application reads Kestrel endpoint configuration and attaches the lego-backed certificate store to the Kestrel endpoint named **`Https`** (case-insensitive).

| Kestrel endpoint configuration                | Listening addresses                                                                                                                                             |
| --------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Not provided                                  | `http://+:8080` and `https://+:8081` — the [.NET 8+ container defaults](https://learn.microsoft.com/en-us/dotnet/core/compatibility/containers/8.0/aspnet-port) |
| Provided via `Kestrel__Endpoints__*` env vars | As configured; the certificate store is applied to the endpoint named `Https`                                                                                   |

When configuring custom endpoints with Let's Encrypt, the HTTPS endpoint **must be named `Https`** for the certificate store to be attached automatically:

```yaml
environment:
  - YARPadProxy__IsLetsEncryptEnabled=true
  - Kestrel__Endpoints__Http__Url=http://+:8080
  - Kestrel__Endpoints__Https__Url=https://+:8081
  # ... other Let's Encrypt settings
```

> **⚠️ Important:** If you give the HTTPS endpoint a different name, the certificate store will not be attached and TLS will not work.

---

## Using Your Own TLS Certificate (Without Let's Encrypt)

If you already have a TLS certificate (e.g. issued by your own CA, Certbot/ACME, or a commercial provider), you can configure Kestrel to use it directly — no Let's Encrypt needed.

### Mounting the certificate

Mount the certificate file(s) into the container with a read-only bind mount:

```yaml
yarpad:
  # ...
  volumes:
    - yarpad_data:/data
    - /path/to/certs:/certs:ro # bind-mount your cert directory
  ports:
    - 80:8080
    - 443:8081
```

### Configuring the certificate via environment variables

Kestrel reads certificate configuration from the `Kestrel:Endpoints:<name>:Certificate` section. The endpoint must have an HTTPS URL; the certificate details tell it which file to use.

**PFX / PKCS#12 certificate:**

```yaml
environment:
  - Kestrel__Endpoints__Http__Url=http://+:8080
  - Kestrel__Endpoints__Https__Url=https://+:8081
  - Kestrel__Endpoints__Https__Certificate__Path=/certs/mycert.pfx
  - Kestrel__Endpoints__Https__Certificate__Password=your-pfx-password
```

**PEM certificate + separate key file:**

```yaml
environment:
  - Kestrel__Endpoints__Http__Url=http://+:8080
  - Kestrel__Endpoints__Https__Url=https://+:8081
  - Kestrel__Endpoints__Https__Certificate__Path=/certs/cert.pem
  - Kestrel__Endpoints__Https__Certificate__KeyPath=/certs/cert.key
  # Certificate__Password is only needed if the key is encrypted
```

The endpoint name (`Http`, `Https`) is arbitrary when not using Let's Encrypt; only the URL scheme and port matter.

> **ℹ️ Note:** Ensure the certificate file is readable by the container's user (UID **1654**). Using `:ro` on the bind mount is recommended.

---

## Let's Encrypt (Automatic HTTPS)

YARPad Proxy includes built-in support for automatic TLS certificate issuance and renewal via [Let's Encrypt](https://letsencrypt.org/), powered by the bundled [go-acme/lego](https://github.com/go-acme/lego) CLI.

When enabled, Kestrel listens on:

- **Port 8080** — plain HTTP (used for HTTP-01 ACME challenge responses)
- **Port 8081** — HTTPS with the automatically issued certificate

Certificates are checked once per day. If a certificate is missing it is issued immediately; if it is within 30 days of expiry it is renewed.

### Top-Level Let's Encrypt Options

All Let's Encrypt settings live under `YARPadProxy`. Use `__` as the key separator when setting them via environment variables.

| Variable                                     | Default       | Description                                                                                  |
| -------------------------------------------- | ------------- | -------------------------------------------------------------------------------------------- |
| `YARPadProxy__IsLetsEncryptEnabled`          | `false`       | Enable automatic certificate issuance and renewal                                            |
| `YARPadProxy__RootDataPath`                  | Content root  | Base directory for all persistent data. Use `/data` in Docker                                |
| `YARPadProxy__LetsEncrypt__DataPath`         | `LetsEncrypt` | Subdirectory under `RootDataPath` where lego stores certificates                             |
| `YARPadProxy__LetsEncrypt__UseStagingServer` | `false`       | Use the Let's Encrypt staging CA (recommended during testing to avoid rate limits)           |
| `YARPadProxy__LetsEncrypt__StagingServerUrl` | —             | Override the staging CA URL (leave unset to use the standard Let's Encrypt staging endpoint) |

### Per-Certificate Options

Certificates are configured as named entries under `YARPadProxy__LetsEncrypt__Certificates`. Replace `<name>` with a short identifier for the certificate (e.g. `proxy`). Multiple certificates can be configured by adding more named entries.

| Variable                                                                      | Default | Description                                                                                                                                                                                                      |
| ----------------------------------------------------------------------------- | ------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `YARPadProxy__LetsEncrypt__Certificates__<name>__Email`                       | —       | Contact email registered with the CA                                                                                                                                                                             |
| `YARPadProxy__LetsEncrypt__Certificates__<name>__Domains__0`                  | —       | Domains to include in the certificate. Use `__0`, `__1`, etc. for multiple domains                                                                                                                               |
| `YARPadProxy__LetsEncrypt__Certificates__<name>__AcmeChallengeType`           | `Dns01` | Challenge type: `Dns01` or `Http01`                                                                                                                                                                              |
| `YARPadProxy__LetsEncrypt__Certificates__<name>__DnsProvider`                 | —       | lego DNS provider name (e.g. `cloudflare`, `route53`). Required for `Dns01` challenges. See [lego DNS providers](https://go-acme.github.io/lego/dns/) for the full list                                          |
| `YARPadProxy__LetsEncrypt__Certificates__<name>__EnvironmentVariables__<KEY>` | —       | Provider-specific credentials passed as environment variables to lego (e.g. `CF_DNS_API_TOKEN` for Cloudflare). See the relevant [lego DNS provider docs](https://go-acme.github.io/lego/dns/) for required keys |

### Example: DNS-01 Challenge (Cloudflare)

DNS-01 is the recommended challenge type — it works behind firewalls and supports wildcard certificates.

```yaml
services:
  yarpad-init:
    image: busybox:1.36
    command: sh -c "chown -R 1654:1654 /data && chmod 750 /data"
    volumes:
      - yarpad_data:/data
    restart: "no"

  yarpad:
    image: ghcr.io/ante-maric/yarpad-proxy:0.16.0
    restart: unless-stopped
    depends_on:
      yarpad-init:
        condition: service_completed_successfully
    volumes:
      - yarpad_data:/data
    ports:
      - 80:8080
      - 443:8081
    environment:
      - YARPad__ConnectionString=DataSource=/data/yarpad.db
      - YARPadProxy__IsLetsEncryptEnabled=true
      - YARPadProxy__RootDataPath=/data
      - YARPadProxy__LetsEncrypt__Certificates__proxy__Email=admin@example.com
      - YARPadProxy__LetsEncrypt__Certificates__proxy__Domains__0=proxy.example.com
      - YARPadProxy__LetsEncrypt__Certificates__proxy__AcmeChallengeType=Dns01
      - YARPadProxy__LetsEncrypt__Certificates__proxy__DnsProvider=cloudflare
      - YARPadProxy__LetsEncrypt__Certificates__proxy__EnvironmentVariables__CF_DNS_API_TOKEN=your-cloudflare-api-token

volumes:
  yarpad_data:
```

### Example: HTTP-01 Challenge

HTTP-01 challenges are served at `/.well-known/acme-challenge/` on port 8080. Port 80 must be publicly reachable from the internet.

```yaml
environment:
  - YARPad__ConnectionString=DataSource=/data/yarpad.db
  - YARPadProxy__IsLetsEncryptEnabled=true
  - YARPadProxy__RootDataPath=/data
  - YARPadProxy__LetsEncrypt__Certificates__proxy__Email=admin@example.com
  - YARPadProxy__LetsEncrypt__Certificates__proxy__Domains__0=proxy.example.com
  - YARPadProxy__LetsEncrypt__Certificates__proxy__AcmeChallengeType=Http01
```

> **⚠️ Important:** The HTTP-01 challenge requires that port 80 is publicly reachable from the internet on your domain before certificates can be issued.

### Staging Server

Set `YARPadProxy__LetsEncrypt__UseStagingServer=true` to use the Let's Encrypt staging CA during development and testing. This avoids hitting production rate limits. Staging certificates are not trusted by browsers — switch back to `false` for production.

---

## Hosting Behind Another Reverse Proxy

When YARPad Proxy sits behind another reverse proxy (e.g., nginx, Traefik, Azure App Gateway), configure trusted proxies so the LAN-only middleware evaluates the correct client IP:

```yaml
environment:
  - YARPad__LanAccess__TrustedProxies__0=10.0.0.1
  - YARPad__LanAccess__TrustedNetworks__0=10.0.0.0/8
  - YARPad__LanAccess__ForwardLimit=1
```

---

## Volumes and Persistence

The `/data` volume stores all persistent state:

| Path                 | Contents                                                       |
| -------------------- | -------------------------------------------------------------- |
| `/data/yarpad.db`    | SQLite database (YARP configuration, profiles, Identity users) |
| `/data/LetsEncrypt/` | Certificate data (when Let's Encrypt is enabled)               |

**Always back up the `/data` volume regularly.** Losing `yarpad.db` means losing all configuration, profiles, and user accounts.

---

## First-Time Setup

1. Start the container with `docker compose up -d`
2. Browse to `http://<host>/yarpad`
3. Register the first admin account at `/yarpad/Account/Register`
4. Optionally set up passkeys, 2FA, and recovery codes
5. Create a configuration profile, add routes and clusters, and activate it

> **⚠️ Important:** By default, only the first user can register. Set `YARPad__MultiUserEnabled=true` if you need multiple admin accounts, then set it back to `false` once all accounts are created.

---

**Back to:** [YARPad Proxy Overview](overview.md) · [Documentation Index](../README.md)
