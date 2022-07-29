using System.Diagnostics;
using Xunit.Abstractions;

namespace LSMDatabase.Tests;

public class GetSetTests : TestBase
{
    private readonly ITestOutputHelper _testOutputHelper;

    public GetSetTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void SetTest()
    {
        var dataBase = new Database(new DatabaseConfig()
            { DataDir = Path.Combine(AppContext.BaseDirectory, "TempDB") });

        var keys = new List<string>();
        for (int i = 0; i < 1 * 1000; i++)
        {
            var key = $"123{i}";
            dataBase.Set(key, i);
            keys.Add(key);
        }

        var list = dataBase.GetKeys();
        var cahji = keys.Except(list).ToList();
        _testOutputHelper.WriteLine($"差集{cahji.Count}");
    }

    [Fact]
    public void SetGetTest()
    {
        var dataBase = new Database(new DatabaseConfig()
            { DataDir = Path.Combine(AppContext.BaseDirectory, "TempDB") });
        Stopwatch stopwatch = Stopwatch.StartNew();
        int index = 0;
        var value = new TestValue()
        {
            Name = "by 蓝创精英团队", Age = 20,
            Value = "5465415567498498486768184978495645646546546544654654654654654564654654648964564154"
        };
        var testLength = 100 * 10000;
        for (int i = 0; i < testLength; i++)
        {
            var key = $"bbbbb{index}";
            value.Key = key;
            dataBase.Set(key, value);
            index++;
        }

        stopwatch.Stop();
        var size = GetDirSize(dataBase.DatabaseConfig.DataDir);
        Log.Info(
            $"keyvalue 数据长度:{value.AsBytes().Length} 实际文件大小:{size} MB 插入{testLength}条数据 耗时:{stopwatch.ElapsedMilliseconds}毫秒 或 {stopwatch.Elapsed.TotalSeconds} 秒,平均每秒插入:{testLength / stopwatch.Elapsed.Seconds}条");

        Task.Run(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                var key1 = $"bbbbb{testLength - 1}";
                var key2 = $"bbbbb0";
                stopwatch.Restart();
                var result = dataBase.Get($"bbbbb{testLength - 1}");
                if (result.IsExist())
                {
                    _testOutputHelper.WriteLine(result.Get<TestValue>().Name.ToString());
                }
                else
                {
                    _testOutputHelper.WriteLine($"{key1} 不存在!");
                }

                stopwatch.Stop();
                Log.Info($"查询key1:{key1} 耗时:{stopwatch.ElapsedMilliseconds} 毫秒");
                stopwatch.Restart();
                var result2 = dataBase.Get($"bbbbb{testLength - 1}");
                if (result2.IsExist())
                {
                    _testOutputHelper.WriteLine(result2.Get<TestValue>().Name.ToString());
                }
                else
                {
                    _testOutputHelper.WriteLine($"{key2} 不存在!");
                }

                stopwatch.Stop();
                Log.Info($"查询key2:{key2} 耗时:{stopwatch.ElapsedMilliseconds} 毫秒");
                Thread.Sleep(1000);
            }
        });
    }

    [Fact]
    public void GetTest()
    {
        var dataBase = new Database(new DatabaseConfig()
            { DataDir = Path.Combine(AppContext.BaseDirectory, "TempDB") });
        var stopwatch = Stopwatch.StartNew();
        var list = dataBase.GetKeys();
        stopwatch.Stop();
        Log.Info($"查询Keys:{list.Count} 耗时:{stopwatch.ElapsedMilliseconds} 毫秒");
        list.Sort();
        var key1 = list.Last();
        List<long> times = new List<long>();
        for (int i = 0; i < 100; i++)
        {
            stopwatch.Restart();
            var result = dataBase.Get(key1);
            stopwatch.Stop();
            times.Add(stopwatch.ElapsedMilliseconds);
            Log.Info($"查询Key:{key1} 耗时:{stopwatch.ElapsedMilliseconds} 毫秒");
            if (result.IsSuccess())
            {
                _testOutputHelper.WriteLine(result.Get<TestValue>().Name.ToString());
            }
            else
            {
                _testOutputHelper.WriteLine($"{key1} 不存在!");
            }
        }

        //平均每条耗时
        _testOutputHelper.WriteLine($"百条查询平均每条查询耗时:{times.Sum() / 100}毫秒");
    }
}