using Couchbase;
using Couchbase.KeyValue;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ICouchbaseCollection _collection;
    private readonly ICluster _cluster;

    public TaskRepository(ICouchbaseCollection collection, ICluster cluster)
    {
        _collection = collection;
        _cluster = cluster;
    }

    public async Task<TaskItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await _collection.GetAsync(id, options => options.CancellationToken(cancellationToken));
        return result.ContentAs<TaskItem>();
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        string query = "SELECT t.* FROM `Tasks`.`1l`.`tasks` t";

        var result = await _cluster.QueryAsync<TaskItem>(query, options =>
        {
            options.CancellationToken(cancellationToken);
        });

        var items = new List<TaskItem>();
        await foreach (var item in result.Rows.WithCancellation(cancellationToken))
        {
            items.Add(item);
        }

        return items;
    }

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await _collection.InsertAsync(task.Id, task, options => options.CancellationToken(cancellationToken));
        return task;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceAsync(task.Id, task, options => options.CancellationToken(cancellationToken));
        return task;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _collection.RemoveAsync(id, options => options.CancellationToken(cancellationToken));
        return true;
    }
}