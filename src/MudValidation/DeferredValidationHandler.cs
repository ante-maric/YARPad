using FluentValidation;
using FluentValidation.Results;

namespace CodingCell.MudValidation;

public sealed class DeferredValidationHandler<T> : IDisposable
{
    private readonly MudValidator<T> _validator;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly CancellationTokenSource _disposeCts = new();
    // Captured once: a token stays usable after the source is cancelled, so disposal cannot race a late caller.
    private readonly CancellationToken _disposeToken;

    // Property names fields asked for during the current validation: the errors of those properties
    // are shown by their field, every other error ends up in UnclaimedErrors.
    private readonly HashSet<string> _claimedProperties = new(StringComparer.Ordinal);
    private readonly Lock _claimedPropertiesLock = new();

    // The model is validated once and all fields share the result; null while validation is deferred.
    private Task<ValidationResult>? _currentValidation;
    private bool _disposed;

    private IReadOnlyDictionary<string, object?> _contextData = new Dictionary<string, object?>();

    public DeferredValidationHandler(MudValidator<T> validator)
    {
        _validator = validator;
        _disposeToken = _disposeCts.Token;
    }

    public Func<object, string, Task<IEnumerable<string>>> Validation => ValidateFieldAsync;

    public bool IsValidating { get; private set; }

    /// <summary>
    /// Errors of the last validation no field has shown, e.g. errors of root-level rules or of
    /// properties without a (correctly) bound field. They make the model invalid like any other error.
    /// </summary>
    public IReadOnlyList<string> UnclaimedErrors { get; private set; } = [];

    public void SetContextData(IReadOnlyDictionary<string, object?> contextData) => _contextData = contextData;

    public Task<bool> ValidateAsync(T model, Func<Task> formValidateAsync, Func<bool> isFormValid) =>
        TryValidateAndExecuteAsync(model, formValidateAsync, isFormValid, () => Task.CompletedTask);

    public async Task<bool> TryValidateAndExecuteAsync(T model, Func<Task> formValidateAsync, Func<bool> isFormValid, Func<Task> onValidAsync)
    {
        await _gate.WaitAsync();
        try
        {
            // A caller queued behind a validation that closed the dialog finds the handler disposed.
            if (_disposed)
                return false;

            IsValidating = true;

            if (!await ValidateModelAsync(model, formValidateAsync, isFormValid))
                return false;

            await onValidAsync();
            return true;
        }
        finally
        {
            IsValidating = false;
            _gate.Release();
        }
    }

    private async Task<bool> ValidateModelAsync(T model, Func<Task> formValidateAsync, Func<bool> isFormValid)
    {
        lock (_claimedPropertiesLock)
            _claimedProperties.Clear();

        UnclaimedErrors = [];

        // Never scoped to a property: a property include does not select rules behind a root-level rule
        // (e.g. inheritance validators), and silently validates nothing when the property name is off.
        var context = ValidationContextFactory.Create(model, propertyName: null, _contextData);
        var validation = _validator.ValidateAsync(context, _disposeToken);

        _currentValidation = validation;
        try
        {
            // Every field of the form(s) asks for its own errors through ValidateFieldAsync.
            await formValidateAsync();

            var result = await validation;

            UnclaimedErrors = result.Errors
                .Where(e => !IsClaimed(e.PropertyName))
                .Select(e => e.ErrorMessage)
                .ToList();

            return isFormValid() && UnclaimedErrors.Count == 0;
        }
        catch (OperationCanceledException) when (_disposeToken.IsCancellationRequested)
        {
            return false;
        }
        finally
        {
            _currentValidation = null;
        }
    }

    // The model MudForm passes in is ignored: the fields of every form bound to this handler
    // show the errors of the model given to the running validation.
    private async Task<IEnumerable<string>> ValidateFieldAsync(object model, string propertyName)
    {
        var validation = _currentValidation;
        if (validation == null)
            return Array.Empty<string>();

        // A field bound to the model itself owns no property, so it leaves all errors unclaimed.
        if (string.IsNullOrWhiteSpace(propertyName))
            return Array.Empty<string>();

        lock (_claimedPropertiesLock)
            _claimedProperties.Add(propertyName);

        try
        {
            var result = await validation;

            return result.Errors
                .Where(e => IsPropertyOrChild(e.PropertyName, propertyName))
                .Select(e => e.ErrorMessage)
                .ToList();
        }
        catch (OperationCanceledException) when (_disposeToken.IsCancellationRequested)
        {
            return Array.Empty<string>();
        }
    }

    private bool IsClaimed(string? errorPropertyName)
    {
        if (string.IsNullOrEmpty(errorPropertyName))
            return false;

        lock (_claimedPropertiesLock)
            return _claimedProperties.Any(x => IsPropertyOrChild(errorPropertyName, x));
    }

    // "Items" owns "Items", "Items[0]" and "Items[0].Name", but not "ItemsCount".
    private static bool IsPropertyOrChild(string? errorPropertyName, string propertyName)
    {
        if (errorPropertyName == null || !errorPropertyName.StartsWith(propertyName, StringComparison.Ordinal))
            return false;

        return errorPropertyName.Length == propertyName.Length || errorPropertyName[propertyName.Length] is '.' or '[';
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        // A validation still in flight ends without errors. The source and the gate are not disposed:
        // neither holds an OS handle here, and a caller still waiting on the gate must wake up to see _disposed.
        _disposeCts.Cancel();
    }
}
