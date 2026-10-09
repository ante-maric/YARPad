using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CodingCell.ReactiveStore;

/// <summary>
/// Debug aid that detects changes made to a published state without going through <see cref="StateStore{TState}.Update"/>,
/// such as a <c>@bind</c> on store state.
/// </summary>
/// <remarks>
/// It keeps a JSON snapshot of the last published state and compares the store's current state against it on the next update.
/// A state that cannot be serialized turns the tripwire off for that store. Not thread-safe: the store calls it under its lock.
/// Used only while <see cref="StateStoreDiagnostics.DetectMutations"/> is on.
/// </remarks>
internal sealed class MutationTripwire<TState>
{
    private JsonNode? _snapshot;
    private bool _hasSnapshot;
    private bool _isDisabled;

    public void Snapshot(TState state)
    {
        if (TrySerialize(state, out var node))
        {
            _snapshot = node;
            _hasSnapshot = true;
        }
    }

    /// <summary>
    /// Forgets the snapshot, so a later check starts from the state it sees then. Called while detection is off.
    /// </summary>
    public void Reset()
    {
        _snapshot = null;
        _hasSnapshot = false;
    }

    /// <summary>
    /// Returns the JSON path of the first difference between <paramref name="state"/> and the last snapshot,
    /// or <see langword="null"/> when they are equal.
    /// </summary>
    /// <remarks>
    /// Without a snapshot (detection was turned on after the last update) the state is snapshotted and nothing is reported.
    /// A difference is reported once: the changed state becomes the new snapshot.
    /// </remarks>
    public string? FindChange(TState state)
    {
        if (!_hasSnapshot)
        {
            Snapshot(state);
            return null;
        }

        if (!TrySerialize(state, out var node) || JsonNode.DeepEquals(_snapshot, node))
            return null;

        var path = FindDifference(_snapshot, node, "$");
        _snapshot = node;

        return path;
    }

    private bool TrySerialize(TState state, out JsonNode? node)
    {
        node = null;

        if (_isDisabled)
            return false;

        try
        {
            node = JsonSerializer.SerializeToNode(state);
            return true;
        }
        catch (Exception ex) when (ex is NotSupportedException or JsonException or InvalidOperationException)
        {
            _isDisabled = true;
            Debug.WriteLine($"Mutation tripwire disabled for {typeof(TState)}: the state cannot be serialized. {ex.Message}");

            return false;
        }
    }

    private static string FindDifference(JsonNode? expected, JsonNode? actual, string path)
    {
        switch (expected, actual)
        {
            case (JsonObject expectedObject, JsonObject actualObject):
                foreach (var name in expectedObject.Select(x => x.Key).Union(actualObject.Select(x => x.Key)))
                {
                    if (!JsonNode.DeepEquals(expectedObject[name], actualObject[name]))
                        return FindDifference(expectedObject[name], actualObject[name], $"{path}.{name}");
                }

                break;

            case (JsonArray expectedArray, JsonArray actualArray) when expectedArray.Count == actualArray.Count:
                for (var i = 0; i < expectedArray.Count; i++)
                {
                    if (!JsonNode.DeepEquals(expectedArray[i], actualArray[i]))
                        return FindDifference(expectedArray[i], actualArray[i], $"{path}[{i}]");
                }

                break;
        }

        return path;
    }
}
