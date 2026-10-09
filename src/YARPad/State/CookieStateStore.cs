using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace CodingCell.YARPad;

/// <summary>
/// State store whose initial state is read from a request cookie, falling back to the given initial state
/// when the cookie is missing or unreadable. Writing the cookie is up to the caller (e.g. client-side script).
/// </summary>
public class CookieStateStore<TState> : StateStore<TState> 
    where TState : class
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CookieStateStore(TState initialState, IHttpContextAccessor httpContextAccessor, string cookieName)
        : base(ReadFromCookie(httpContextAccessor, cookieName, initialState))
    {
    }

    private static TState ReadFromCookie(IHttpContextAccessor httpContextAccessor, string cookieName, TState fallback)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cookieName);

        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext is null)
                return fallback;

            if (!httpContext.Request.Cookies.TryGetValue(cookieName, out var raw) || string.IsNullOrWhiteSpace(raw))
                return fallback;

            var decoded = Uri.UnescapeDataString(raw);
            var parsed = JsonSerializer.Deserialize<TState>(decoded, _serializerOptions);

            return parsed ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }
}
