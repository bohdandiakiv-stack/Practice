using FluentAssertions;
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

    [Fact]
    public async Task POST_CreateTask_ShouldReturn201AndTask()
    {
        var dto = new CreateTaskDto(
            Title: "API Test Task",
            Description: "Created by test");

        var response = await _client.PostAsJsonAsync("/api/tasks", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        task.Should().NotBeNull();
        task!.Title.Should().Be(dto.Title);

        _createdTaskIds.Add(task.Id);
    }

    [Fact]
    public async Task GET_AllTasks_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/tasks");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PUT_UpdateTask_WithExistingId_ShouldReturn200()
    {
        var task = await CreateTaskAsync();

        var updateDto = new UpdateTaskDto(
            Title: "Updated title",
            Description: "Updated description");

        var response = await _client.PutAsJsonAsync(
            $"/api/tasks/{task.Id}",
            updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DELETE_Task_WithExistingId_ShouldReturn204()
    {
        var task = await CreateTaskAsync();

        var response = await _client.DeleteAsync($"/api/tasks/{task.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        _createdTaskIds.Remove(task.Id);
    }

    [Fact]
    public async Task GET_Task_WithExistingId_ShouldReturnTask()
    {
        // Arrange
        var task = await CreateTaskAsync();

        // Act
        var response = await _client.GetAsync($"/api/tasks/{task.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TaskDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
        result.Title.Should().Be(task.Title);
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

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert (read again)
        var getResponse = await _client.GetAsync($"/api/tasks/{task.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await getResponse.Content.ReadFromJsonAsync<TaskDto>();
        updated!.Title.Should().Be(updateDto.Title);
        updated.Description.Should().Be(updateDto.Description);
    }

    private async Task<TaskDto> CreateTaskAsync()
    {
        var dto = new CreateTaskDto(
            Title: "Helper Task",
            Description: "Created for API test");

        var response = await _client.PostAsJsonAsync("/api/tasks", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        task.Should().NotBeNull();

        _createdTaskIds.Add(task!.Id);

        return task!;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var id in _createdTaskIds.Distinct())
        {
            try
            {
                await _client.DeleteAsync($"/api/tasks/{id}");
            }
            catch
            {
            }
        }

        _createdTaskIds.Clear();
    }
}