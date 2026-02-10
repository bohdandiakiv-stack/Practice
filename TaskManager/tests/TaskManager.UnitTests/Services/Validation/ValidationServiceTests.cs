using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Dtos.Tasks;
using TaskManager.Application.Validation.Facades.Interfaces;
using TaskManager.UnitTests.Services.Common;

namespace TaskManager.UnitTests.Services.Validation;

public class ValidationServiceTests
{
    private readonly IValidationService _validationService;

    public ValidationServiceTests()
    {
        _validationService = TestServiceProviderFactory
            .Create()
            .GetRequiredService<IValidationService>();
    }

    [Fact]
    public async Task ValidateAsync_ValidDto_DoesNotThrow()
    {
        // Arrange
        var dto = new CreateTaskDto("Title", "Description");

        // Act
        Func<Task> act = () => _validationService.ValidateAsync(dto);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateAsync_InvalidDto_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateTaskDto(string.Empty, "Description");

        // Act
        Func<Task> act = () => _validationService.ValidateAsync(dto);

        // Assert
        using var scope = new AssertionScope();

        var exception = await act.Should().ThrowAsync<ValidationException>();

        exception.Which.Errors
            .Should()
            .Contain(e => e.PropertyName == nameof(CreateTaskDto.Title));
    }

    [Fact]
    public async Task ValidateAsync_ValidatorNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new UnknownDto();

        // Act
        Func<Task> act = () => _validationService.ValidateAsync(dto);

        // Assert
        using var scope = new AssertionScope();

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();

        exception.Which.Message
            .Should()
            .Contain(nameof(UnknownDto));
    }

    [Fact]
    public async Task ValidateAsync_NullInstance_ThrowsInvalidOperationException()
    {
        // Act
        Func<Task> act = () =>
            _validationService.ValidateAsync<CreateTaskDto>(null!);

        // Assert
        using var scope = new AssertionScope();

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();

        exception.Which.Message
            .Should()
            .Contain("null model");
    }

    private record UnknownDto;
}
