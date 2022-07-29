using System.Diagnostics;
using Xunit.Abstractions;

namespace LSMDatabase.Tests;

public class DeleteTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public DeleteTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void DeleteTest()
    {
        var dataBase = new Database(new DatabaseConfig()
            { DataDir = Path.Combine(AppContext.BaseDirectory, "TempDB") });
        var stopwatch = Stopwatch.StartNew();
        var list = dataBase.GetKeys();
        stopwatch.Stop();
        Log.Info($"查询Keys:{list.Count} 耗时:{stopwatch.ElapsedMilliseconds} 毫秒");
        list.Sort();
        var key1 = list.Last();
        stopwatch.Restart();
        var result = dataBase.Get(key1);
        stopwatch.Stop();
        Log.Info($"查询Key:{key1} 耗时:{stopwatch.ElapsedMilliseconds} 毫秒");
        _testOutputHelper.WriteLine(result.IsSuccess() ? result.Get<TestValue>().Name : $"{key1} 不存在!");

        dataBase.Delete(key1);

        list = dataBase.GetKeys();
        _testOutputHelper.WriteLine($"剩下数据个数:{list.Count}");

        stopwatch.Restart();
        result = dataBase.Get(key1);
        stopwatch.Stop();
        Log.Info($"查询Key:{key1} 耗时:{stopwatch.ElapsedMilliseconds} 毫秒");
        _testOutputHelper.WriteLine(result.IsSuccess() ? result.ToString() : $"{key1} 不存在!");
    }
}