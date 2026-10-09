using FluentValidation;

namespace CodingCell.MudValidation;

internal static class ValidationContextFactory
{
    public static ValidationContext<T> Create<T>(T instance, string? propertyName, IReadOnlyDictionary<string, object?> contextData)
    {
        var context = string.IsNullOrWhiteSpace(propertyName)
            ? new ValidationContext<T>(instance)
            : ValidationContext<T>.CreateWithOptions(instance, x => x.IncludeProperties(propertyName));

        foreach (var kvp in contextData)
            context.RootContextData[kvp.Key] = kvp.Value;

        return context;
    }
}
