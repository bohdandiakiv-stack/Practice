using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Validation.Facades.Interfaces;

namespace TaskManager.Application.Validation.Facades;

public class ValidationService : IValidationService
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ValidateAsync<T>(T instance, CancellationToken cancellationToken = default)
    {
        var validator = _serviceProvider.GetService<IValidator<T>>();

        if (validator == null)
        {
            throw new InvalidOperationException($"No validator found for type {typeof(T).Name}.");
        }

        var validationResult = await validator.ValidateAsync(instance, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }

    public void ValidateNotNull<T>(T instance, string errorMessage, params object[] args)
    {
        if (instance is null)
        {
            throw new ValidationException(string.Format(errorMessage, args));
        }
    }
}
