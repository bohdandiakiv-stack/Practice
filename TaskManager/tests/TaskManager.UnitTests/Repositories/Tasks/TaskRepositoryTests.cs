using Couchbase;
using Couchbase.KeyValue;
using Couchbase.Query;
using FluentAssertions;
using Moq;
using TaskManager.Domain.Models.Tasks;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.UnitTests.Repositories.Tasks
{
    public class TaskRepositoryTests
    {
        private readonly Mock<ICouchbaseCollection> _collectionMock;
        private readonly Mock<ICluster> _clusterMock;

        private readonly TaskRepository _sut;

        public TaskRepositoryTests()
        {
            _collectionMock = new Mock<ICouchbaseCollection>(MockBehavior.Strict);
            _clusterMock = new Mock<ICluster>(MockBehavior.Strict);

            _sut = new TaskRepository(
                _collectionMock.Object,
                _clusterMock.Object);
        }

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WhenExists_ReturnsTask()
        {
            // Arrange
            var task = new TaskItem
            {
                Id = "1",
                Title = "Title",
                Description = "Desc"
            };

            var getResultMock = new Mock<IGetResult>();
            getResultMock
                .Setup(r => r.ContentAs<TaskItem>())
                .Returns(task);

            _collectionMock
                .Setup(c => c.GetAsync(
                    task.Id,
                    It.IsAny<GetOptions>()))
                .ReturnsAsync(getResultMock.Object);

            // Act
            var result = await _sut.GetByIdAsync(task.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(task.Id);

            _collectionMock.Verify(
                c => c.GetAsync(task.Id, It.IsAny<GetOptions>()),
                Times.Once);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_InsertsDocument_AndReturnsTask()
        {
            // Arrange
            var task = new TaskItem
            {
                Id = "1",
                Title = "New",
                Description = "Desc"
            };

            _collectionMock
                .Setup(c => c.InsertAsync(
                    task.Id,
                    task,
                    It.IsAny<InsertOptions>()))
                .ReturnsAsync(Mock.Of<IMutationResult>());

            // Act
            var result = await _sut.CreateAsync(task);

            // Assert
            result.Should().BeSameAs(task);

            _collectionMock.Verify(
                c => c.InsertAsync(task.Id, task, It.IsAny<InsertOptions>()),
                Times.Once);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ReplacesDocument_AndReturnsTask()
        {
            // Arrange
            var task = new TaskItem
            {
                Id = "1",
                Title = "Updated",
                Description = "Desc"
            };

            _collectionMock
                .Setup(c => c.ReplaceAsync(
                    task.Id,
                    task,
                    It.IsAny<ReplaceOptions>()))
                .ReturnsAsync(Mock.Of<IMutationResult>());

            // Act
            var result = await _sut.UpdateAsync(task);

            // Assert
            result.Should().BeSameAs(task);

            _collectionMock.Verify(
                c => c.ReplaceAsync(task.Id, task, It.IsAny<ReplaceOptions>()),
                Times.Once);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_RemovesDocument_AndReturnsTrue()
        {
            // Arrange
            var id = "1";

            _collectionMock
                .Setup(c => c.RemoveAsync(
                    id,
                    It.IsAny<RemoveOptions>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteAsync(id);

            // Assert
            result.Should().BeTrue();

            _collectionMock.Verify(
                c => c.RemoveAsync(id, It.IsAny<RemoveOptions>()),
                Times.Once);
        }

        #endregion

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ReturnsItems_FromQuery()
        {
            // Arrange
            var tasks = new[]
            {
            new TaskItem { Id = "1", Title = "T1" },
            new TaskItem { Id = "2", Title = "T2" }
        };

            var rows = CreateAsyncEnumerable(tasks);

            var queryResultMock = new Mock<IQueryResult<TaskItem>>();
            queryResultMock
                .Setup(r => r.Rows)
                .Returns(rows);

            _clusterMock
                .Setup(c => c.QueryAsync<TaskItem>(
                    It.IsAny<string>(),
                    It.IsAny<QueryOptions>()))
                .ReturnsAsync(queryResultMock.Object);

            // Act
            var result = (await _sut.GetAllAsync()).ToList();

            // Assert
            result.Should().HaveCount(2);
            result.Select(t => t.Id)
                  .Should().BeEquivalentTo("1", "2");

            _clusterMock.Verify(
                c => c.QueryAsync<TaskItem>(
                    It.IsAny<string>(),
                    It.IsAny<QueryOptions>()),
                Times.Once);
        }

        #endregion

        private static IAsyncEnumerable<TaskItem> CreateAsyncEnumerable(
            IEnumerable<TaskItem> items)
        {
            return Iterate(items);

            static async IAsyncEnumerable<TaskItem> Iterate(
                IEnumerable<TaskItem> source)
            {
                foreach (var item in source)
                {
                    yield return item;
                    await Task.Yield();
                }
            }
        }
    }
}
