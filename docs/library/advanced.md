# 🛠️ Advanced Scenarios

## Custom Path Prefix

Host YARPad under a different URL path:

```json
{
  "YARPad": {
    "PathPrefix": "/proxy-admin"
  }
}
```

---

## Programmatic Configuration

You can also configure YARPad in code:

```csharp
builder.Services.AddYARPad(builder.Configuration, options =>
{
    options.PathPrefix = "/yarpad";
    options.ConnectionString = "DataSource=yarpad.db";
    options.MultiUserEnabled = false;
    options.IsLanOnlyAccessDisabled = false;
});
```

---

## Troubleshooting

### Cannot Access YARPad UI

**Problem:** `404 Not Found` when navigating to `/yarpad`

**Solution:**

1. Verify `app.MapYARPad()` is called in `Program.cs`
2. Check that `PathPrefix` in configuration matches your URL
3. Ensure YARP endpoints are mapped: `app.MapReverseProxy()`

### LAN-Only Access Blocking Legitimate Users

**Problem:** `403 Forbidden` when accessing from allowed network

**Solution:**

1. Check if you're behind a proxy — configure `TrustedProxies`/`TrustedNetworks`
2. Verify `ForwardLimit` is set correctly
3. Check server logs for the detected IP address
4. Temporarily set `IsLanOnlyAccessDisabled = true` for testing (not recommended for production)

### Registration Page Not Working

**Problem:** "Registration is disabled" message

**Solution:**

- First user can always register
- Subsequent users need `MultiUserEnabled = true` in configuration
- Check `appsettings.json` or environment variables

### YARP Configuration Not Applying

**Problem:** Route/cluster changes don't take effect

**Solution:**

1. Ensure profile is **activated** (not just saved)
2. Check validation errors in the UI
3. Review logs for `YarpConfigProvider` messages
4. Verify YARP is registered: `builder.Services.AddReverseProxy()`

---

## FAQ

### Can I use a database other than SQLite?

YARPad currently supports SQLite only.

### How do I migrate from JSON configuration files to YARPad?

YARPad does not currently support importing existing YARP JSON configurations. You'll need to manually recreate your routes, clusters, and policies using the YARPad UI. On the other hand, YARPad configuration can be exported and imported.

### Can I use YARPad in production?

Yes! YARPad is designed for production use with proper security configuration:

- Keep LAN-only access enabled
- Use HTTPS
- Enable authentication and 2FA
- Regular backups of the database

### Does YARPad support clustering/high availability?

YARPad currently supports single-instance deployments only.

### What happens if I delete the SQLite database?

The database will be recreated on next startup, but all configuration, users, and profiles will be lost. **Always back up your database regularly or export it.**

### How do I update YARPad to a newer version?

1. Review release notes for breaking changes
2. Backup your yarpad database or export configuration
3. Update the NuGet package: `dotnet add package CodingCell.YARPad`
4. Restart the application

---

**Back to:** [Documentation Index](../README.md)
