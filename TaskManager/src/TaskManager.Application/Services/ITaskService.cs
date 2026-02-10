using TaskManager.Application.Dtos.Tasks;

namespace TaskManager.Application.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TaskDto> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<TaskDto> CreateAsync(CreateTaskDto dto, CancellationToken cancellationToken = default);
    Task<TaskDto> UpdateAsync(string id, UpdateTaskDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}