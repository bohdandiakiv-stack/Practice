namespace TaskManager.Api.Contracts.Tasks;

public record UpdateTaskRequest(
    string Title,
    string Description
    );
