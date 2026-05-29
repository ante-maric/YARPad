using FluentValidation;

namespace CodingCell.YARPad;

public class MudValidator<T> : AbstractValidator<T>
{
    public Dictionary<string, object?> ContextData { get; } = new();

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var context = ValidationContext<T>.CreateWithOptions((T)model, x => x.IncludeProperties(propertyName));

        foreach (var kvp in ContextData)
            context.RootContextData[kvp.Key] = kvp.Value;

        var result = await ValidateAsync(context);

        return result.IsValid
            ? Array.Empty<string>()
            : result.Errors.Select(e => e.ErrorMessage);
    };

    protected static bool HaveUniqueIDs<TItem>(List<TItem> items, Func<TItem, string> idFunc)
    {
        var names = items.Select(idFunc);
        return names.Distinct().Count() == items.Count;
    }
}
