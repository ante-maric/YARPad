namespace CodingCell.ReactiveStore;

internal static class ReducerScope
{
    [ThreadStatic]
    private static int _depth;

    public static bool IsActive => _depth > 0;

    public static Scope Enter()
    {
        _depth++;
        return default;
    }

    public readonly struct Scope : IDisposable
    {
        public void Dispose() => _depth--;
    }
}
