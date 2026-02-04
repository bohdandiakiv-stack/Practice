using TaskManager.Domain.Models.Tasks;

namespace TaskManager.Domain.Repositories;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task<TaskItem> UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}