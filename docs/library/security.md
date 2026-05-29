# 🔐 Security

## Security Best Practices

**⚠️ YARPad is a powerful admin tool — treat it like you would a database admin panel.**

✅ **DO:**

- ✅ Keep `IsLanOnlyAccessDisabled = false` (LAN-only access enabled)
- ✅ Use `MultiUserEnabled = false` for single-admin scenarios
- ✅ Enable HTTPS in production (never run YARPad over HTTP in production)
- ✅ Configure `TrustedProxies`/`TrustedNetworks` correctly when behind another a reverse proxy
- ✅ Use strong passwords and enable 2FA for all admin accounts
- ✅ Regularly back up the SQLite database (`yarpad.db`) or export your configuration
- ✅ Monitor access logs for unauthorized attempts
- ✅ Use passkeys (WebAuthn) for enhanced authentication security

❌ **DON'T:**

- ❌ Expose YARPad directly to the public internet
- ❌ Set `IsLanOnlyAccessDisabled = true` unless you have additional security layers
- ❌ Use default/weak passwords
- ❌ Run without HTTPS in production
- ❌ Grant unnecessary users access when `MultiUserEnabled = true`

---

## LAN-Only Access

By default, YARPad only responds to requests from:

- **Loopback** addresses (`127.0.0.1`, `::1`)
- **Private network ranges** (RFC 1918: `10.x.x.x`, `172.16-31.x.x`, `192.168.x.x`)

This is enforced by `LanOnlyAccessMiddleware` which runs before authentication.

---

## `LanOnly` Authorization Policy

YARPad includes a built-in `LanOnly` authorization policy that you can apply to proxied routes from the UI.

---

## Hosting Behind a Reverse Proxy

When YARPad runs behind another proxy (e.g., nginx, Azure App Gateway), configure trusted proxies so the LAN-only middleware evaluates the correct client IP:

```json
{
  "YARPad": {
    "LanAccess": {
      "TrustedProxies": ["10.0.0.100"],
      "TrustedNetworks": ["10.0.0.0/24"],
      "ForwardLimit": 1
    }
  }
}
```

YARPad automatically configures `ForwardedHeadersMiddleware` to respect `X-Forwarded-For`, `X-Forwarded-Proto`, and `X-Forwarded-Host` headers from trusted proxies.

---

**Next:** [Advanced Scenarios](advanced.md)
