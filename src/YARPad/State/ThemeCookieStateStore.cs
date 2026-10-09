using Microsoft.AspNetCore.Http;

namespace CodingCell.YARPad;

/// <summary>
/// Theme state seeded from the cookie that wwwroot/js/yarpad.js writes (see themeCookieName there).
/// </summary>
public class ThemeCookieStateStore : CookieStateStore<ThemeState>
{
    public const string COOKIE_NAME = "yarpad_themestate";

    public ThemeCookieStateStore(ThemeState initialState, IHttpContextAccessor httpContextAccessor)
        : base(initialState, httpContextAccessor, COOKIE_NAME)
    {
    }
}
