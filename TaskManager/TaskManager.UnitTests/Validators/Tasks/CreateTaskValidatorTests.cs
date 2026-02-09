using FluentValidation.TestHelper;
using TaskManager.Application.Dtos.Tasks;
using TaskManager.Application.Validation.Tasks;

namespace TaskManager.UnitTests.Validators.Tasks;

public class CreateTaskValidatorTests
{
    private readonly CreateTaskValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        var dto = new CreateTaskDto("", "desc");

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Title_Exceeds_Max_Length()
    {
        var dto = new CreateTaskDto(new string('a', 101), "desc");

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Title cannot exceed 100 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_Description_Exceeds_Max_Length()
    {
        var dto = new CreateTaskDto("Title", new string('a', 501));

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Not_Have_Errors_For_Valid_Dto()
    {
        var dto = new CreateTaskDto("Valid title", "Valid description");

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
