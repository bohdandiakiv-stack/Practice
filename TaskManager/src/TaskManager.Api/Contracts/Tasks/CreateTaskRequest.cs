namespace TaskManager.Api.Contracts.Tasks;

public record CreateTaskRequest(
    string Title,
    string Description
    );
