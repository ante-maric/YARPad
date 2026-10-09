using FluentValidation;

namespace CodingCell.MudValidation;

public class MudValidator<T> : AbstractValidator<T>
{
    public async Task<IEnumerable<string>> ValidateFieldAsync(
        object model,
        string propertyName,
        IReadOnlyDictionary<string, object?> contextData,
        CancellationToken cancellationToken = default)
    {
        var context = ValidationContextFactory.Create((T)model, propertyName, contextData);

        var result = await ValidateAsync(context, cancellationToken);

        return result.IsValid
            ? Array.Empty<string>()
            : result.Errors.Select(e => e.ErrorMessage);
    }

    protected static bool HaveUniqueIDs<TItem>(List<TItem> items, Func<TItem, string> idFunc)
    {
        var names = items.Select(idFunc);
        return names.Distinct().Count() == items.Count;
    }
}
