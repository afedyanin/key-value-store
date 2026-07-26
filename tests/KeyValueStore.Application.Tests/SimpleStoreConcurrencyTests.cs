namespace KeyValueStore.Application.Tests;

public class SimpleStoreConcurrencyTests
{
    [Fact]
    public async Task Concurrent_SetAndGet_ShouldMaintainDataIntegrity()
    {
        using var store = new SimpleStore();
        const int numThreads = 10;
        const int opsPerThread = 100;
        var tasks = new List<Task>();

        // Записываем данные из нескольких потоков
        for (var i = 0; i < numThreads; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                for (var j = 0; j < opsPerThread; j++)
                {
                    var key = $"key-{index}-{j}";
                    var profile = new UserProfile
                    {
                        Id = index * opsPerThread + j,
                        Username = $"user-{index}-{j}",
                        CreatedAt = DateTime.UtcNow
                    };
                    store.Set(key, profile);
                    await Task.Yield();
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Проверяем статистику перед чтением данных
        var (setCount, getCount, deleteCount) = store.GetStatistics();
        Assert.Equal(numThreads * opsPerThread, setCount);
        Assert.Equal(0L, getCount);
        Assert.Equal(0L, deleteCount);

        // Проверяем, что все данные корректно сохранены
        for (var i = 0; i < numThreads; i++)
        {
            for (var j = 0; j < opsPerThread; j++)
            {
                var key = $"key-{i}-{j}";
                var profile = store.Get(key);
                Assert.NotNull(profile);
                Assert.Equal(i * opsPerThread + j, profile.Id);
                Assert.Equal($"user-{i}-{j}", profile.Username);
            }
        }
    }

    [Fact]
    public async Task Concurrent_GetAfterSet_ShouldReturnCorrectValues()
    {
        using var store = new SimpleStore();
        const int numKeys = 100;

        // Записываем данные
        for (var j = 0; j < numKeys; j++)
        {
            store.Set($"data-{j}", new UserProfile
            {
                Id = j,
                Username = $"user-{j}",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Проверяем статистику после записи
        var (setCount, getCount, deleteCount) = store.GetStatistics();
        Assert.Equal(numKeys, setCount);
        Assert.Equal(0L, getCount);
        Assert.Equal(0L, deleteCount);

        // Читаем данные из нескольких потоков
        var readCount = 0L;

        var readTasks = new List<Task>();
        for (var i = 0; i < 5; i++)
        {
            readTasks.Add(Task.Run(async () =>
            {
                for (var j = 0; j < numKeys; j++)
                {
                    var key = $"data-{j}";
                    var profile = store.Get(key);
                    Assert.NotNull(profile);
                    Assert.Equal(j, profile.Id);
                    Assert.Equal($"user-{j}", profile.Username);

                    Interlocked.Increment(ref readCount);

                    await Task.Yield();
                }
            }));
        }

        await Task.WhenAll(readTasks);

        // Проверяем статистику после чтения
        (setCount, getCount, deleteCount) = store.GetStatistics();
        Assert.Equal(numKeys, setCount);
        Assert.Equal(5 * numKeys, getCount);
        Assert.Equal(0L, deleteCount);

        // Проверяем, что все чтения выполнены
        Assert.Equal(5 * numKeys, readCount);
    }

    [Fact]
    public async Task Concurrent_SetAndDelete_ShouldMaintainCorrectStatistics()
    {
        using var store = new SimpleStore();
        const int numSets = 100;
        const int numDeletes = 50;
        var setTasks = new List<Task>();

        // Записываем данные из нескольких потоков
        for (var i = 0; i < numSets; i++)
        {
            var index = i;
            setTasks.Add(Task.Run(async () =>
            {
                store.Set($"item-{index}", new UserProfile
                {
                    Id = index,
                    Username = $"user-{index}",
                    CreatedAt = DateTime.UtcNow
                });
                await Task.Yield();
            }));
        }

        await Task.WhenAll(setTasks);

        // Проверяем статистику после записи
        var (setCount, getCount, deleteCount) = store.GetStatistics();
        Assert.Equal(numSets, setCount);
        Assert.Equal(0L, getCount);
        Assert.Equal(0L, deleteCount);

        var deleteTasks = new List<Task>();

        // Удаляем половину из нескольких потоков
        for (var i = 0; i < numDeletes; i++)
        {
            var index = i;
            deleteTasks.Add(Task.Run(async () =>
            {
                store.Delete($"item-{index}");
                await Task.Yield();
            }));
        }

        await Task.WhenAll(deleteTasks);

        // Проверяем статистику после удаления
        (setCount, getCount, deleteCount) = store.GetStatistics();
        Assert.Equal(numSets, setCount);
        Assert.Equal(0L, getCount);
        Assert.Equal(numDeletes, deleteCount);

        // Проверяем, что удаленные элементы действительно удалены
        for (var i = 0; i < numDeletes; i++)
        {
            var profile = store.Get($"item-{i}");
            Assert.Null(profile);
        }

        // Проверяем, что оставшиеся элементы на месте
        for (var i = numDeletes; i < numSets; i++)
        {
            var profile = store.Get($"item-{i}");
            Assert.NotNull(profile);
            Assert.Equal(i, profile.Id);
        }
    }
}
