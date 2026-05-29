# ⚙️ Configuration Reference

## `YARPadOptions`

| Option                    | Type               | Default                | Description                                                                                                                                                               |
| ------------------------- | ------------------ | ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `PathPrefix`              | `string`           | `/yarpad`              | URL path where the YARPad UI is hosted                                                                                                                                    |
| `ConnectionString`        | `string`           | `DataSource=yarpad.db` | SQLite connection string for the management database                                                                                                                      |
| `InstanceID`              | `string`           | Machine name           | Identifier used for logging and self-notification suppression                                                                                                             |
| `MultiUserEnabled`        | `bool`             | `false`                | **Single-user mode by default.** Only the first user can register. Set to `true` to allow multiple admin accounts                                                         |
| `IsLanOnlyAccessDisabled` | `bool`             | `false`                | **LAN-only access by default.** YARPad responds only to requests from local network addresses. Set to `true` to disable this restriction (not recommended for production) |
| `LanAccess`               | `LanAccessOptions` | See below              | Trusted proxies, networks, and allowed ranges for LAN-only protection                                                                                                     |

---

## `LanAccessOptions`

| Option                        | Type       | Default | Description                                                                                                                                                                      |
| ----------------------------- | ---------- | ------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `TrustedProxies`              | `string[]` | `[]`    | Trusted reverse proxy IP addresses. X-Forwarded-For headers are only trusted from these IPs. e.g. `["10.0.0.1", "192.168.1.1"]`                                                  |
| `TrustedNetworks`             | `string[]` | `[]`    | Trusted reverse proxy networks in CIDR notation. Use instead of (or in addition to) `TrustedProxies` when multiple proxy IPs may be used. e.g. `["10.0.0.0/8", "172.16.0.0/12"]` |
| `AdditionalAllowedRanges`     | `string[]` | `[]`    | Extra CIDR ranges allowed to access YARPad beyond the default private ranges. Supports IPv4 and IPv6. e.g. `["203.0.113.0/24", "2a02:1234::/48"]`                                |
| `IncludeDefaultPrivateRanges` | `bool`     | `true`  | Includes standard private IPv4 and local IPv6 ranges                                                                                                                             |
| `AllowLoopback`               | `bool`     | `true`  | Allows loopback access                                                                                                                                                           |
| `ForwardLimit`                | `int?`     | `1`     | Maximum number of forwarded proxy hops to trust                                                                                                                                  |

---

## Identity Configuration

YARPad uses ASP.NET Core Identity for authentication and user management. Customize Identity settings in `appsettings.json`:

```json
{
  "Identity": {
    "SignIn": {
      "RequireConfirmedAccount": true,
      "RequireConfirmedEmail": false,
      "RequireConfirmedPhoneNumber": false
    },
    "Password": {
      "RequiredLength": 6,
      "RequireDigit": true,
      "RequireLowercase": true,
      "RequireUppercase": true,
      "RequireNonAlphanumeric": false,
      "RequiredUniqueChars": 1
    },
    "Lockout": {
      "DefaultLockoutTimeSpanMinutes": 15,
      "MaxFailedAccessAttempts": 5,
      "AllowedForNewUsers": true
    },
    "User": {
      "RequireUniqueEmail": true,
      "AllowedUserNameCharacters": "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"
    }
  }
}
```

**Key Identity Features:**

- **Password Requirements** — Configure complexity rules for user passwords
- **Account Lockout** — Protect against brute-force attacks with automatic account lockout
- **Email Confirmation** — Optionally require email verification before allowing sign-in
- **Two-Factor Authentication (2FA)** — TOTP authenticator apps supported
- **Passkeys (WebAuthn/FIDO2)** — Passwordless authentication with hardware/platform authenticators
- **Recovery Codes** — Backup codes for 2FA recovery

For complete documentation on ASP.NET Core Identity configuration options, see the [Microsoft Identity documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration).

---

**Next:** [Working with YARPad](working-with-yarpad.md)
