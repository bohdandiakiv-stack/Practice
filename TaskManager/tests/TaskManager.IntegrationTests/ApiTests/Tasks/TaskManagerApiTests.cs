using FluentAssertions;
using FluentAssertions.Execution;
using System.Net;
using System.Net.Http.Json;
using TaskManager.Application.Dtos.Tasks;

namespace TaskManager.IntegrationTests.ApiTests.Tasks;

public class TaskManagerApiTests :
    IClassFixture<ApiTestFixture>,
    IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly List<string> _createdTaskIds = new();

    public TaskManagerApiTests(ApiTestFixture fixture)
    {
        _client = fixture.Client;
    }

    #region Create

    [Fact]
    public async Task POST_CreateTask_ShouldReturn201AndTask()
    {
        // Arrange
        var dto = new CreateTaskDto(
            Title: "API Test Task",
            Description: "Created by test");

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", dto);

        // Assert
        using var scope = new AssertionScope();

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Content.Headers.ContentType!.MediaType
            .Should().Be("application/json");

        var task = await response.Content.ReadFromJsonAsync<TaskDto>();

        task.Should().NotBeNull();
        task!.Id.Should().NotBeNullOrWhiteSpace();
        task.Title.Should().Be(dto.Title);
        task.Description.Should().Be(dto.Description);

        _createdTaskIds.Add(task.Id);
    }

    [Fact]
    public async Task POST_CreateTask_InvalidDto_ShouldReturn400()
    {
        // Arrange
        var dto = new CreateTaskDto("", "");

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GetAll

    [Fact]
    public async Task GET_AllTasks_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GET_Task_WithExistingId_ShouldReturnTask()
    {
        // Arrange
        var task = await CreateTaskAsync();

        // Act
        var response = await _client.GetAsync($"/api/tasks/{task.Id}");

        // Assert
        using var scope = new AssertionScope();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TaskDto>();

        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
        result.Title.Should().Be(task.Title);
        result.Description.Should().Be(task.Description);
    }

    [Fact]
    public async Task GET_Task_WithUnknownId_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/unknown-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Update

    [Fact]
    public async Task PUT_UpdateTask_WithExistingId_ShouldReturn200()
    {
        // Arrange
        var task = await CreateTaskAsync();

        var updateDto = new UpdateTaskDto(
            Title: "Updated title",
            Description: "Updated description");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/tasks/{task.Id}",
            updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PUT_UpdateTask_ShouldPersistChanges()
    {
        // Arrange
        var task = await CreateTaskAsync();

        var updateDto = new UpdateTaskDto(
            Title: "Persisted title",
            Description: "Persisted description");

        // Act
        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/tasks/{task.Id}",
            updateDto);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert (read again)
        var getResponse = await _client.GetAsync($"/api/tasks/{task.Id}");

        using var scope = new AssertionScope();

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await getResponse.Content.ReadFromJsonAsync<TaskDto>();

        updated.Should().NotBeNull();
        updated!.Title.Should().Be(updateDto.Title);
        updated.Description.Should().Be(updateDto.Description);
    }

    [Fact]
    public async Task PUT_UpdateTask_WithUnknownId_ShouldReturn404()
    {
        // Arrange
        var updateDto = new UpdateTaskDto("Title", "Desc");

        // Act
        var response = await _client.PutAsJsonAsync(
            "/api/tasks/unknown-id",
            updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DELETE_Task_WithExistingId_ShouldReturn204()
    {
        // Arrange
        var task = await CreateTaskAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{task.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        _createdTaskIds.Remove(task.Id);
    }

    [Fact]
    public async Task DELETE_Task_WithUnknownId_ShouldReturn404()
    {
        // Act
        var response = await _client.DeleteAsync("/api/tasks/unknown-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Helpers

    private async Task<TaskDto> CreateTaskAsync()
    {
        // Arrange
        var dto = new CreateTaskDto(
            Title: "Helper Task",
            Description: "Created for API test");

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var task = await response.Content.ReadFromJsonAsync<TaskDto>();

        task.Should().NotBeNull();
        task!.Id.Should().NotBeNullOrWhiteSpace();

        _createdTaskIds.Add(task.Id);

        return task;
    }

    #endregion

    #region Lifecycle

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var id in _createdTaskIds.Distinct())
        {
            await _client.DeleteAsync($"/api/tasks/{id}");
        }

        _createdTaskIds.Clear();
    }

    #endregion
}