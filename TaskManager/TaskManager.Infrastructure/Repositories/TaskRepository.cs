using Couchbase;
using Couchbase.KeyValue;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{

    private readonly IBucket _bucket;
    private readonly ICouchbaseCollection _collection;

    public TaskRepository(IBucket bucket, IScope scope, ICouchbaseCollection collection)
    {
        _bucket = bucket;
        _collection = collection;
    }

    public async Task<TaskItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await _collection.GetAsync(id);
        return result.ContentAs<TaskItem>();
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cluster = _bucket.Cluster;
        var query = "SELECT t.* FROM `TaskManager` t";

        var result = await cluster.QueryAsync<TaskItem>(query);
        var items = new List<TaskItem>();
        await foreach (var item in result.Rows.WithCancellation(cancellationToken))
        {
            items.Add(item);
        }
        return items;
    }

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await _collection.InsertAsync(task.Id, task);
        return task;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceAsync(task.Id, task);
        return task;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _collection.RemoveAsync(id);
        return true;
    }
}