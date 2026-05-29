# 💼 Working with YARPad

> **ⓘ Contextual help throughout the UI**
>
> One of the biggest pain points with YARP is knowing what every setting actually does. YARPad addresses this directly: wherever it makes sense, field labels are prefixed with **ⓘ** and clicking or hovering them shows an inline explanation — what the option controls, what values are accepted, and how the choice affects proxy behaviour. This is true for routes, clusters, destinations, transforms, health check policies, session affinity, load balancing, HTTP client settings, and more. You should rarely need to leave the UI to look something up.

---

## Configuration Profiles

Profiles let you maintain multiple YARP configurations and switch between them safely:

- **Create** — Start a new configuration from scratch
- **Clone** — Duplicate an existing profile for testing changes
- **Import** — Load configuration from JSON (YARP format)
- **Export** — Download profile as JSON for backup or sharing
- **Activate** — Apply the profile to the running YARP instance

---

## Routes

Configure how YARP matches and forwards requests. YARPad provides visual editors for all YARP route configuration options:

- **Match Criteria** — Path patterns, hosts, HTTP methods, headers, query parameters
- **Order** — Priority when multiple routes match
- **Cluster Assignment** — Which backend cluster handles the request
- **Policies** — Authorization, timeout, CORS, rate limiting, output caching
- **Transforms** — Modify requests/responses (headers, path, query, etc.)
- **Metadata** — Custom key-value pairs for routing logic

For detailed information about YARP routing concepts, see the [YARP documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/getting-started?view=aspnetcore-10.0).

---

## Clusters

Define backend destination groups and behavior. YARPad provides visual editors for all YARP cluster configuration options:

- **Destinations** — Backend server URLs with optional health metadata
- **Load Balancing** — Round-robin, least requests, random, power-of-two, and more
- **Session Affinity** — Sticky sessions configuration
- **Health Checks** — Active and passive health check policies
- **HTTP Client** — Request configuration, timeouts, SSL/TLS settings
- **Metadata** — Custom cluster-level settings

For detailed information about YARP cluster concepts, see the [YARP documentation](https://microsoft.github.io/reverse-proxy/articles/config-files.html).

---

## Transforms

Modify requests and responses in the proxy pipeline. YARPad provides visual editors for all built-in YARP transforms and supports custom transforms.

### Built-in Transforms

- Path manipulation (prefix, rewrite, pattern)
- Header manipulation (request, response, trailers)
- Query string modifications
- HTTP method changes
- Forwarded headers
- Client certificate forwarding

### Custom Transforms

- Define custom transform types with parameters in YARPad
- Register your `ITransformProvider` implementation in the host app
- Configure instances through the YARPad UI

For detailed information about YARP transforms, see the [YARP Transforms documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/extensibility-transforms?view=aspnetcore-10.0).

---

## Policies

Author and manage ASP.NET Core policies that can be applied to routes and clusters:

- **Authorization** — ASP.NET Core authorization policies
- **CORS** — CORS policies
- **Rate Limiting** — ASP.NET Core rate limiting policies
- **Output Caching** — Output cache policies
- **Timeouts** — Timeout policies
- **Load Balancing** — Custom YARP load balancing policies
- **Session Affinity** — Custom YARP session affinity policies
- **Health Checks** — Custom YARP health check policies

For YARP-specific policy information, see the [YARP documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/load-balancing?view=aspnetcore-10.0).

---

**Next:** [Security](security.md)
