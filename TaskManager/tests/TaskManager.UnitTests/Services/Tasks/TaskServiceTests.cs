using FluentAssertions;
using Moq;
using System.ComponentModel.DataAnnotations;
using TaskManager.Application.Dtos.Tasks;
using TaskManager.Application.Services.Implementations;
using TaskManager.Application.Validation.Facades.Interfaces;
using TaskManager.Domain.Models.Tasks;
using TaskManager.Domain.Repositories;
using TaskManager.UnitTests.Services.Common;

namespace TaskManager.UnitTests.Services.Tasks;

public class TaskServiceTests : BaseTestFixture
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IValidationService> _validationServiceMock;

    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>(MockBehavior.Strict);
        _validationServiceMock = new Mock<IValidationService>(MockBehavior.Strict);

        _sut = new TaskService(
            _taskRepositoryMock.Object,
            Mapper,
            _validationServiceMock.Object);
    }

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        // Arrange
        var tasks = new[]
        {
            new TaskItem { Id = "1", Title = "T1", Description = "D1" },
            new TaskItem { Id = "2", Title = "T2", Description = "D2" }
        };

        _taskRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Select(x => x.Id)
              .Should().BeEquivalentTo("1", "2");

        _taskRepositoryMock.Verify(
            r => r.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_TaskExists_ReturnsDto()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = "1",
            Title = "Title",
            Description = "Description"
        };

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        var result = await _sut.GetByIdAsync("1");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("1");
        result.Title.Should().Be("Title");

        _taskRepositoryMock.Verify(
            r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_TaskNotFound_ReturnsNull()
    {
        // Arrange
        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync("404", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _sut.GetByIdAsync("404");

        // Assert
        result.Should().BeNull();

        _taskRepositoryMock.Verify(
            r => r.GetByIdAsync("404", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ValidDto_ValidatesThenCreatesAndReturnsDto()
    {
        // Arrange
        var dto = new CreateTaskDto("New title", "New desc");

        var sequence = new MockSequence();

        _validationServiceMock
            .InSequence(sequence)
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _taskRepositoryMock
            .InSequence(sequence)
            .Setup(r => r.CreateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem task, CancellationToken _) =>
            {
                task.Id = "1";
                return task;
            });

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Id.Should().Be("1");
        result.Title.Should().Be(dto.Title);
        result.Description.Should().Be(dto.Description);

        _validationServiceMock.Verify(
            v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()),
            Times.Once);

        _taskRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidationFails_ThrowsAndDoesNotCallRepository()
    {
        // Arrange
        var dto = new CreateTaskDto("", "");

        _validationServiceMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("validation error"));

        // Act
        var act = () => _sut.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        _taskRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_TaskExists_UpdatesAndReturnsDto()
    {
        // Arrange
        var dto = new UpdateTaskDto("Updated", "Updated desc");

        var existing = new TaskItem
        {
            Id = "1",
            Title = "Old",
            Description = "Old"
        };

        _validationServiceMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _taskRepositoryMock
            .Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _sut.UpdateAsync("1", dto);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be(dto.Title);
        result.Description.Should().Be(dto.Description);

        _taskRepositoryMock.Verify(
            r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()),
            Times.Once);

        _taskRepositoryMock.Verify(
            r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_TaskNotFound_ReturnsNull_AndDoesNotUpdate()
    {
        // Arrange
        var dto = new UpdateTaskDto("Title", "Desc");

        _validationServiceMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _sut.UpdateAsync("1", dto);

        // Assert
        result.Should().BeNull();

        _taskRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        // Arrange
        _taskRepositoryMock
            .Setup(r => r.DeleteAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync("1");

        // Assert
        result.Should().BeTrue();

        _taskRepositoryMock.Verify(
            r => r.DeleteAsync("1", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}