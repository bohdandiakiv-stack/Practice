using Couchbase;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;
using Couchbase.Query;
using TaskManager.Domain.Models.Tasks;
using TaskManager.Domain.Repositories;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ICouchbaseCollection _collection;
    private readonly ICluster _cluster;

    public TaskRepository(
        ICouchbaseCollection collection,
        ICluster cluster)
    {
        _collection = collection;
        _cluster = cluster;
    }

    public async Task<TaskItem?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _collection.GetAsync(
                id,
                new GetOptions().CancellationToken(cancellationToken));

            return result.ContentAs<TaskItem>();
        }
        catch (DocumentNotFoundException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        const string query = """
            SELECT t.*
            FROM `Tasks`.`1l`.`tasks` t
            """;

        var result = await _cluster.QueryAsync<TaskItem>(
            query,
            new QueryOptions().CancellationToken(cancellationToken));

        var items = new List<TaskItem>();

        await foreach (var item in result.Rows.WithCancellation(cancellationToken))
        {
            items.Add(item);
        }

        return items;
    }

    public async Task<TaskItem> CreateAsync(
        TaskItem task,
        CancellationToken cancellationToken = default)
    {
        await _collection.InsertAsync(
            task.Id,
            task,
            new InsertOptions().CancellationToken(cancellationToken));

        return task;
    }

    public async Task<TaskItem?> UpdateAsync(
        TaskItem task,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _collection.ReplaceAsync(
                task.Id,
                task,
                new ReplaceOptions().CancellationToken(cancellationToken));

            return task;
        }
        catch (DocumentNotFoundException)
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _collection.RemoveAsync(
                id,
                new RemoveOptions().CancellationToken(cancellationToken));

            return true;
        }
        catch (DocumentNotFoundException)
        {
            return false;
        }
    }
}