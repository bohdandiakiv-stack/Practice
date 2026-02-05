namespace TaskManager.Application.Validation.Facades.Interfaces;

public interface IValidationService
{
    Task ValidateAsync<T>(T instance, CancellationToken cancellationToken = default);
}
