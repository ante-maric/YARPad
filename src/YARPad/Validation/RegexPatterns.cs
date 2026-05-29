namespace CodingCell.YARPad;

public static class RegexPatterns
{
    public static string DOMAIN = @"^(?=.{1,255}$)([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$";
    public static string HEADER_COOKIE_NAME = "^[!#$%&'*+\\-.^_`|~0-9A-Za-z]+$";
    public static string HOST = "^(?:(?:[a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,}|(?:\\d{1,3}\\.){3}\\d{1,3})(?::\\d{1,5})?$";
}