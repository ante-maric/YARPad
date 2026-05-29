using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace CodingCell.YARPad;

public class CookieStateStore<TState> : StateStore<TState> where TState : class
{
    private static readonly string CookieName = $"yarpad_{typeof(TState).Name}".ToLowerInvariant();
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CookieStateStore(TState initialState, IHttpContextAccessor httpContextAccessor)
        : base(initialState)
    {
        var fromCookie = ReadFromCookie(httpContextAccessor, initialState);
        _subject.OnNext(fromCookie);
    }

    private TState ReadFromCookie(IHttpContextAccessor httpContextAccessor, TState fallback)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext is null)
                return fallback;

            if (!httpContext.Request.Cookies.TryGetValue(CookieName, out var raw) || string.IsNullOrWhiteSpace(raw))
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
