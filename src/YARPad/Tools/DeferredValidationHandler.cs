using FluentValidation;

namespace CodingCell.YARPad;

internal class DeferredValidationHandler<T>
{
    private readonly MudValidator<T> _validator;
    private bool _isDeferred = true;
    private readonly bool _validateWholeObject;

    public DeferredValidationHandler(MudValidator<T> validator, bool validateWholeObject = false)
    {
        _validator = validator;
        _validateWholeObject = validateWholeObject;
    }

    public async Task ExecuteValidationAsync(Func<Task> validationFunc)
    {
        try
        {
            _isDeferred = false;
            await validationFunc();
        }
        finally
        {
            _isDeferred = true;
        }
    }

    public async Task<IEnumerable<string>> ValidateAsync(object model, string propertyName)
    {
        if (_isDeferred)
            return Array.Empty<string>();

        if (_validateWholeObject)
        {
            var context = ValidationContext<T>.CreateWithOptions((T)model, x => x.IncludeProperties(propertyName));

            foreach (var kvp in _validator.ContextData)
                context.RootContextData[kvp.Key] = kvp.Value;

            var result = await _validator.ValidateAsync(context);
            if (result.IsValid)
                return Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(propertyName))
                return result.Errors.Select(e => e.ErrorMessage);

            return result.Errors
                .Where(e => string.Equals(e.PropertyName, propertyName, System.StringComparison.Ordinal))
                .Select(e => e.ErrorMessage);
        }

        return await _validator.ValidateValue(model, propertyName);
    }
}
